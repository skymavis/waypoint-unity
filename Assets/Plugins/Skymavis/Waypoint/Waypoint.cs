using System.Collections.Generic;
using SkyMavis.Utils;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace SkyMavis
{
    public static class Waypoint
    {
#if UNITY_IOS
        // iOS function
        [DllImport("__Internal")]
        private static extern void initClient(string address, string clientId, string chainRpc, int chainId);


        [DllImport("__Internal")]
        private static extern void authorize(string state, string redirects);

        [DllImport("__Internal")]
        private static extern void sendTransaction(string state, string redirect, string to, string value);

        [DllImport("__Internal")]
        private static extern void personalSign(string state, string redirect, string message);

        [DllImport("__Internal")]
        private static extern void signTypeData(string state, string redirect, string typedData);

        [DllImport("__Internal")]
        private static extern void callContract(string state, string redirect, string contractAddress, string data, string value = "0x0");
#endif

#if UNITY_ANDROID
        private static AndroidJavaObject _client;
#endif

        private static string _deeplink;
        private static bool _isInitialized = false;
        private static List<UnityAction<string, string>> _subscribers = new List<UnityAction<string, string>>();

        public static void BindOnResponse(UnityAction<string, string> cb) { BindWaitForMessage(cb); }
        public static void UnBindOnResponse(UnityAction<string, string> cb) { UnbindWaitForMessage(cb); }

        private static void BindWaitForMessage(UnityAction<string, string> cb)
        {
            _subscribers.Add(cb);
        }

        private static void UnbindWaitForMessage(UnityAction<string, string> cb)
        {
            _subscribers.Remove(cb);
        }

        private static string GenerateRandomState()
        {
            return System.Guid.NewGuid().ToString();
        }

        public static bool IsConnected
        {
            get
            {
#if UNITY_STANDALONE
                return Overlay.IsConnected;
#else
                return true;
#endif
            }
        }

#if UNITY_STANDALONE
        public static void Init(string sessionID, int port)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            Overlay.Initialize(sessionID, port);
            Overlay.OnDataResponsed += OnOverlayResponse;
        }
#else
        public static void Init(string clientId, string deeplinkSchema, bool isTestnet = false)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _deeplink = $"{deeplinkSchema}://open";

            string endpoint = "https://id.skymavis.com";
            string rpcUrl = "https://api.roninchain.com/rpc";
            int chainId = 2020;
            if (isTestnet)
            {
                rpcUrl = "https://saigon-testnet.roninchain.com/rpc";
                chainId = 2021;
            }

#if UNITY_ANDROID
            AndroidJavaObject clientObj = new AndroidJavaObject("com.skymavis.sdk.id.Client", endpoint, clientId, rpcUrl, chainId);
            _client = clientObj;
#elif UNITY_IOS
            initClient(endpoint, clientId, rpcUrl, chainId);
#endif
            Application.deepLinkActivated += OnDeepLinkActivated;
        }
#endif

        public static string OnAuthorize()
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("authorize", context, _deeplink, state);
#elif UNITY_IOS
            authorize(state, _deeplink);
#else
            Overlay.GetIDToken(state);
#endif
            return state;
        }
        public static string OnPersonalSign(string message)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("personalSign", context, _deeplink, state, message);
#elif UNITY_IOS
            personalSign(state, _deeplink, message);
#else
            Overlay.PersonalSign(state, message);
#endif
            return state;
        }

        public static string OnSignTypeData(string typedData)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("signTypeData", context, _deeplink, state, typedData);
#elif UNITY_IOS
            signTypeData(state, _deeplink, typedData);
#else
            SkyMavis.Overlay.SignTypedData(state, typedData);
#endif
            return state;
        }

        public static string SendTransaction(string receiverAddress, string value)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("sendTransaction", context, _deeplink, state, receiverAddress, value);
#elif UNITY_IOS
            sendTransaction(state, _deeplink, receiverAddress, value);
#else
            Overlay.SendNativeToken(state, receiverAddress, value);
#endif
            return state;
        }

        public static string OnCallContract(string contractAddress, string data, string value = "0x0")
        {
            string state = GenerateRandomState();

#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("callContract", context, _deeplink, state, contractAddress, data, value);
#elif UNITY_IOS
            callContract(state, _deeplink, contractAddress, data, value);
#else

            Overlay.SendTransaction(state, contractAddress, value, data);
#endif
            return state;
        }

        public static string OnGetIDToken()
        {

#if UNITY_STANDALONE
            string state = GenerateRandomState();
            Overlay.GetIDToken(state);
            return state;
#else
            throw new System.NotImplementedException();
#endif
        }

        private static void OnDeepLinkActivated(string url)
        {
            if (url == null) return;
            var vals = DeeplinkHelper.ParseDeeplink(url);
            if (vals.TryGetValue("state", out var state))
            {
                JObject jData = new JObject();
                foreach (var p in vals)
                {
                    jData.Add(new JProperty(p.Key, p.Value));
                }
                string dataStr = jData.ToString();
                foreach (var p in _subscribers)
                {
                    try
                    {
                        p?.Invoke(state, dataStr);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private static void OnOverlayResponse(string jsonRaw)
        {
            if (jsonRaw == null) return;
            var resp = JObject.Parse(jsonRaw);

            if (!resp.TryGetValue("state", out var state)) return;
            if (!resp.TryGetValue("data", out var data)) return;
            string dataStr = data.ToString();
            foreach (var p in _subscribers)
            {
                try
                {
                    p?.Invoke((string)state, dataStr);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
