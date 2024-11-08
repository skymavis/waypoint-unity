using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SkyMavis.Waypoint.Adapters;
using SkyMavis.Waypoint.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SkyMavis.Waypoint
{
    public static class Waypoint
    {
        private static IAdapter _adapter;
        public static event Action<string, string> ResponseReceived;

        public static bool IsConnected => _adapter?.IsConnected ?? false;

        public static void SetUp(string sessionID, int port)
        {
            ThrowIfInitialized();
            _adapter = new OverlayAdapter(sessionID, port);
            Overlay.OnDataResponsed += OnOverlayResponse;
        }

        public static void SetUp(string clientID, string deepLinkSchema, bool isTestNet = false)
        {
            ThrowIfInitialized();

            var deepLink = $"{deepLinkSchema}://open";
            var endpoint = "https://waypoint.roninchain.com";
            var rpcURL = "https://api.roninchain.com/rpc";
            var chainID = 2020;

            if (isTestNet)
            {
                rpcURL = "https://saigon-testnet.roninchain.com/rpc";
                chainID = 2021;
            }

            _adapter = Application.platform switch
            {
                RuntimePlatform.Android => new AndroidAdapter(clientID, deepLink, endpoint, rpcURL, chainID),
                RuntimePlatform.IPhonePlayer => new IOSAdapter(clientID, deepLink, endpoint, rpcURL, chainID),
                _ => throw new NotSupportedException($"Platform not supported: {Application.platform}"),
            };
            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        public static void CleanUp()
        {
            Overlay.OnDataResponsed -= OnOverlayResponse;
            Application.deepLinkActivated -= OnDeepLinkActivated;
            _adapter.Dispose();
            _adapter = null;
        }

        public static string Authorize(string scope = null) =>
            ExecuteWithRandomState(state => _adapter.Authorize(state, scope));

        public static string PersonalSign(string message, string from = null) =>
            ExecuteWithRandomState(state => _adapter.PersonalSign(state, message, from));

        public static string SignTypedData(string typedData, string from = null) =>
            ExecuteWithRandomState(state => _adapter.SignTypedData(state, typedData, from));

        public static string SendNativeToken(string to, string value, string from = null) =>
            ExecuteWithRandomState(state => _adapter.SendNativeToken(state, to, value, from));

        public static string SendTransaction(string to, string data, string value = "0x0", string from = null) =>
            ExecuteWithRandomState(state => _adapter.SendTransaction(state, to, data, value, from));

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
                ResponseReceived?.Invoke((string)state, dataStr);
            }
        }

        private static void OnOverlayResponse(string jsonRaw)
        {
            if (jsonRaw == null) return;
            var resp = JObject.Parse(jsonRaw);

            if (!resp.TryGetValue("state", out var state)) return;
            if (!resp.TryGetValue("data", out var data)) return;
            string dataStr = data.ToString();
            ResponseReceived?.Invoke((string)state, dataStr);
        }

        private static void ThrowIfInitialized()
        {
            if (_adapter != null)
            {
                throw new InvalidOperationException("Waypoint is already initialized. Consider calling Waypoint.CleanUp() before setting up again.");
            }
        }

        private static string GenerateRandomState() => Guid.NewGuid().ToString();

        private static string ExecuteWithRandomState(Action<string> callback)
        {
            var state = GenerateRandomState();
            callback(state);
            return state;
        }
    }
}
