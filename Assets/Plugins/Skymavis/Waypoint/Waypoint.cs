using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SkyMavis.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SkyMavis
{
    public static partial class Waypoint
    {
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

        private static string ExecuteWithRandomState(Action<string> callback)
        {
            var state = GenerateRandomState();
            callback(state);
            return state;
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

        public static string OnAuthorize() => ExecuteWithRandomState(Authorize_internal);

        public static string OnPersonalSign(string message, string from = null) =>
            ExecuteWithRandomState(state => PersonalSign_internal(state, message, from));

        public static string OnSignTypeData(string typedData, string from = null) =>
            ExecuteWithRandomState(state => SignTypedData_internal(state, typedData, from));

        public static string SendTransaction(string receiverAddress, string value, string from = null) =>
            ExecuteWithRandomState(state => SendTransaction_internal(state, receiverAddress, value, from));

        public static string OnCallContract(string contractAddress, string data, string value = "0x0", string from = null) =>
            ExecuteWithRandomState(state => CallContract_internal(state, contractAddress, data, value, from));

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
