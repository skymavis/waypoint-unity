using System;
using Newtonsoft.Json.Linq;
using SkyMavis.Utils;
using SkyMavis.WaypointInternal.Adapters;
using UnityEngine;
using UnityEngine.Events;

namespace SkyMavis
{
    public static class Waypoint
    {
        public static event Action<string, string> ResponseReceived;

        public static readonly ILogger Logger = new Logger(Debug.unityLogger)
        {
            filterLogType = LogType.Warning,
        };

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
                _ => new OverlayAdapter(settings, OnOverlayResponse),
            };
            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        public static void CleanUp()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;

            if (_adapter != null)
            {
                _adapter.Dispose();
                _adapter = null;
            }
        }

        public static string Authorize(string scope = null) =>
            ExecuteWithRandomState(state => _adapter.Authorize(state, scope));

        public static string PersonalSign(string message, string from = null) =>
            ExecuteWithRandomState(state => _adapter.PersonalSign(state, message, from));

        public static string SignTypedData(string typedData, string from = null) =>
            ExecuteWithRandomState(state => _adapter.SignTypedData(state, typedData, from));

        public static string SendNativeToken(string receiverAddress, string value, string from = null) =>
            SendTransaction(receiverAddress, null, value, from);

        public static string WriteContract(string contractAddress, string humanReadableABI, object functionParameters, string value = "0x0", string from = null) =>
            SendTransaction(contractAddress, ABI.EncodeFunctionData(humanReadableABI, functionParameters), value, from);

        internal static string SendTransaction(string address, string data, string value, string from) =>
            ExecuteWithRandomState(state => _adapter.SendTransaction(state, address, data, value, from));

        public static string AuthAsGuest(string credential, string authDate, string hash, string scope) =>
            ExecuteWithRandomState(state => _adapter.AuthAsGuest(state, credential, authDate, hash, scope));

        public static string RegisterGuestAccount() =>
            ExecuteWithRandomState(state => _adapter.RegisterGuestAccount(state));

        public static string CreateKeylessWallet() =>
            ExecuteWithRandomState(state => _adapter.CreateKeylessWallet(state));

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

        [Obsolete("To be removed in 0.5.0. Use event ResponseReceived instead.")]
        public static void BindOnResponse(UnityAction<string, string> cb) => ResponseReceived += cb.Invoke;

        [Obsolete("To be removed in 0.5.0. Use event ResponseReceived instead.")]
        public static void UnBindOnResponse(UnityAction<string, string> cb) => ResponseReceived -= cb.Invoke;

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
        public static void Init(string clientId, string deeplinkSchema, bool isTestnet = false)
        {
            WaypointSettings.Network network = new WaypointSettings.Network();
            if (isTestnet)
            {
                network = WaypointSettings.Network.Testnet;
            }
            else
            {
                network = WaypointSettings.Network.Mainnet;
            }

            WaypointSettings waypointSettings = new WaypointSettings()
            {
                clientID = clientId,
                deepLinkCallbackURL = $"{deeplinkSchema}://open",
                network = network,
            };

            SetUp(waypointSettings);
        }

        [Obsolete("To be removed in 0.5.0. Use Authorize() instead.")]
        public static string OnAuthorize(string scope = null) => Authorize(scope);

        [Obsolete("To be removed in 0.5.0. Use Authorize() instead.")]
        public static string OnGetIDToken() => Authorize();

        [Obsolete("To be removed in 0.5.0. Use PersonalSign() instead.")]
        public static string OnPersonalSign(string message, string from = null) => PersonalSign(message, from);

        [Obsolete("To be removed in 0.5.0. Use SignTypedData() instead.")]
        public static string OnSignTypeData(string typedData, string from = null) => SignTypedData(typedData, from);

        [Obsolete("To be removed in 0.5.0. Use SendNativeToken() instead.")]
        public static string SendTransaction(string receiverAddress, string value, string from = null) => SendNativeToken(receiverAddress, value, from);

        [Obsolete("To be removed in 0.5.0. Use WriteContract() instead.")]
        public static string OnCallContract(string contractAddress, string data, string value = "0x0", string from = null) => SendTransaction(contractAddress, data, value, from);

        #endregion
    }
}
