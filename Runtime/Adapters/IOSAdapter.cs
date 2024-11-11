using System.Runtime.InteropServices;

namespace SkyMavis.Waypoint.Adapters
{
    internal class IOSAdapter : IAdapter
    {
        private readonly string _deepLink;

        internal IOSAdapter(WaypointSettings settings)
        {
            _deepLink = settings.deepLinkCallbackURL;
            initClient(settings.endpoint, settings.clientID, settings.network.rpcURL, settings.network.chainID);
        }

        public void Dispose() { }

        bool IAdapter.IsConnected => true;

        void IAdapter.Authorize(string state, string scope) =>
            authorize(state, _deepLink, scope);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            sendTransaction(state, _deepLink, to, data, value, from);

        void IAdapter.PersonalSign(string state, string message, string from) =>
            personalSign(state, _deepLink, message, from);

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            sendNativeToken(state, _deepLink, to, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            signTypedData(state, _deepLink, typedData, from);

        [DllImport("__Internal")]
        private static extern void initClient(string waypointOrigin, string clientId, string chainRpc, int chainId);

        [DllImport("__Internal")]
        private static extern void authorize(string state, string redirects, string scope);

        [DllImport("__Internal")]
        private static extern void sendNativeToken(string state, string redirect, string to, string value, string from);

        [DllImport("__Internal")]
        private static extern void personalSign(string state, string redirect, string message, string from);

        [DllImport("__Internal")]
        private static extern void signTypedData(string state, string redirect, string typedData, string from);

        [DllImport("__Internal")]
        private static extern void sendTransaction(string state, string redirect, string to, string data, string value, string from);
    }
}
