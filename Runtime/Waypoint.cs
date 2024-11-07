using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SkyMavis.Waypoint.Utils;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace SkyMavis.Waypoint
{
    public static class Waypoint
    {
#if UNITY_IOS
        // iOS function
        [DllImport("__Internal")]
        private static extern void initClient(string waypointOrigin, string clientId, string chainRpc, int chainId);

        [DllImport("__Internal")]
        private static extern void authorize(string state, string redirects, string scope = null);

        [DllImport("__Internal")]
        private static extern void sendNativeToken(string state, string redirect, string to, string value, string from = null);

        [DllImport("__Internal")]
        private static extern void personalSign(string state, string redirect, string message, string from = null);

        [DllImport("__Internal")]
        private static extern void signTypedData(string state, string redirect, string typedData, string from = null);

        [DllImport("__Internal")]
        private static extern void sendTransaction(string state, string redirect, string to, string data, string value = "0x0", string from = null);
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

            string endpoint = "https://waypoint.roninchain.com";
            string rpcUrl = "https://api.roninchain.com/rpc";
            int chainId = 2020;
            if (isTestnet)
            {
                rpcUrl = "https://saigon-testnet.roninchain.com/rpc";
                chainId = 2021;
            }

#if UNITY_ANDROID
            AndroidJavaObject chainIdObj = new AndroidJavaObject("java.lang.Integer", chainId);
            AndroidJavaObject clientObj = new AndroidJavaObject("com.skymavis.sdk.waypoint.Waypoint", endpoint, clientId, rpcUrl, chainIdObj);
            _client = clientObj;
#elif UNITY_IOS
            initClient(endpoint, clientId, rpcUrl, chainId);
#endif
            Application.deepLinkActivated += OnDeepLinkActivated;
        }
#endif

        public static string OnAuthorize(string scope = null)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("authorize", context, state, _deeplink, scope);
#elif UNITY_IOS
            authorize(state, _deeplink, scope);
#else
            Overlay.GetIDToken(state);
#endif
            return state;
        }
        public static string OnPersonalSign(string message, string from = null)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            if (from != null)
            {
                _client.Call("personalSign", context, state, _deeplink, message, from);
            }
            else
            {
                _client.Call("personalSign", context, state, _deeplink, message);
            }
#elif UNITY_IOS
            personalSign(state, _deeplink, message, from);
#else
            Overlay.PersonalSign(state, message, from);
#endif
            return state;
        }

        public static string OnSignTypeData(string typedData, string from = null)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            if (from != null)
            {
                _client.Call("signTypedData", context, state, _deeplink, typedData, from);
            }
            else
            {
                _client.Call("signTypedData", context, state, _deeplink, typedData);
            }
#elif UNITY_IOS
            signTypedData(state, _deeplink, typedData, from);
#else
            Overlay.SignTypedData(state, typedData);
#endif
            return state;
        }

        public static string SendNativeToken(string to, string value, string from = null)
        {
            string state = GenerateRandomState();
#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("sendNativeToken", context, state, _deeplink, to, value, from);
#elif UNITY_IOS
            sendNativeToken(state, _deeplink, to, value, from);
#else
            Overlay.SendNativeToken(state, to, value, from);
#endif
            return state;
        }

        public static string SendTransaction(string to, string data, string value = "0x0", string from = null)
        {
            string state = GenerateRandomState();

#if UNITY_ANDROID
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("sendTransaction", context, state, _deeplink, to, data, value, from);
#elif UNITY_IOS
            sendTransaction(state, _deeplink, to, data, value, from);
#else
            Overlay.SendTransaction(state, to, value, data, from);
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
