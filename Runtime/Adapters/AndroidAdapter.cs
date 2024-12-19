using UnityEngine;

namespace SkyMavis.WaypointInternal.Adapters
{
    internal class AndroidAdapter : IAdapter
    {
        private readonly string _deepLink;
        private readonly AndroidJavaObject _client;
        private readonly AndroidJavaObject _context;

        internal AndroidAdapter(WaypointSettings settings)
        {
            using var jPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var jIsTestnet = new AndroidJavaObject("java.lang.Boolean", settings.network.chainID == WaypointSettings.Network.Testnet.chainID);
            _deepLink = settings.deepLinkCallbackURL;
            _client = new AndroidJavaObject("com.skymavis.sdk.waypoint.Waypoint", settings.endpoint, settings.clientID, _deepLink, jIsTestnet);
            _context = jPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        public void Dispose()
        {
            _client.Dispose();
            _context.Dispose();
        }

        bool IAdapter.IsConnected => true;

        void IAdapter.Authorize(string state, string scope) =>
            _client.Call("authorize", _context, state, scope);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            _client.Call("sendTransaction", _context, state, to, data, value, from);

        void IAdapter.PersonalSign(string state, string message, string from) =>
            _client.Call("personalSign", _context, state, message, from);

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            _client.Call("sendNativeToken", _context, state, to, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            _client.Call("signTypedData", _context, state, typedData, from);
    }
}
