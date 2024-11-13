using System;
using Newtonsoft.Json.Linq;
using SkyMavis.Utils;
using SkyMavis.WaypointInternal;
using SkyMavis.WaypointInternal.Adapters;
using UnityEngine;

namespace SkyMavis
{
    public static class Waypoint
    {
        public static event Action<string, string> ResponseReceived;

        public static bool IsConnected => _adapter?.IsConnected ?? false;

        private static IAdapter _adapter;

        public static void SetUp(WaypointSettings settings)
        {
            if (_adapter != null)
            {
                throw new InvalidOperationException("Waypoint is already initialized. Consider calling Waypoint.CleanUp() before setting up again.");
            }

            _adapter = Application.platform switch
            {
                RuntimePlatform.Android => new AndroidAdapter(settings),
                RuntimePlatform.IPhonePlayer => new IOSAdapter(settings),
                _ => new OverlayAdapter(settings),
            };
            Overlay.OnDataResponsed += OnOverlayResponse;
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

        private static string GenerateRandomState() => Guid.NewGuid().ToString();

        private static string ExecuteWithRandomState(Action<string> callback)
        {
            var state = GenerateRandomState();
            callback(state);
            return state;
        }

        #region Deprecated API

        [Obsolete("To be removed in 0.5.0. Use SetUp() instead.")]
        public static void Init(string sessionID, int port)
        {

            WaypointSettings waypointSettings = new WaypointSettings()
            {
                mavisHubSessionID = sessionID,
                mavisHubPort = port
            };

            SetUp(waypointSettings);
        }

        [Obsolete("To be removed in 0.5.0. Use SetUp() instead.")]
        public static void Init(string waypointOrigin, string clientId, string deeplinkSchema, string rpcUrl, int chainId)
        {
            WaypointSettings waypointSettings = new WaypointSettings()
            {
                endpoint = waypointOrigin,
                clientID = clientId,
                deepLinkCallbackURL = deeplinkSchema,
                network = new WaypointSettings.Network()
                {
                    rpcURL = rpcUrl,
                    chainID = chainId
                }
            };

            SetUp(waypointSettings);
        }

        [Obsolete("To be removed in 0.5.0. Use Authorize() instead.")]
        public static string OnAuthorize(string scope = null) => Authorize(scope);

        [Obsolete("To be removed in 0.5.0. Use PersonalSign() instead.")]
        public static string OnPersonalSign(string message, string from = null) => PersonalSign(message, from);

        [Obsolete("To be removed in 0.5.0. Use SignTypedData() instead.")]
        public static string OnSignTypeData(string typedData, string from = null) => SignTypedData(typedData, from);

        [Obsolete("To be removed in 0.5.0. Use SendNativeToken() instead.")]
        public static string OnSendTransaction(string receiverAddress, string value, string from = null) => SendNativeToken(receiverAddress, value, from);

        [Obsolete("To be removed in 0.5.0. Use SendTransaction() instead.")]
        public static string OnCallContract(string contractAddress, string data, string value = "0x0", string from = null) => SendTransaction(contractAddress, data, value, from);

        #endregion

    }
}
