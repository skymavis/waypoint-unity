using System.Runtime.InteropServices;

namespace SkyMavis.Waypoint.Adapters
{
    internal class IOSAdapter : IAdapter
    {
        private string _deepLink;

        internal IOSAdapter(string clientID, string deepLink, string endpoint, string rpcURL, int chainID)
        {
            _deepLink = deepLink;
            initClient(endpoint, clientID, rpcURL, chainID);
        }

        public void Dispose() { }

        bool IAdapter.IsConnected => true;

        void IAdapter.Authorize(string state, string scope) =>
            authorize(state, _deepLink, scope);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            sendTransaction(state, _deepLink, to, data, value, from);

        void IAdapter.PersonalSign(string state, string message, string from) =>
            personalSign(state, message, from);

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            sendNativeToken(state, _deepLink, to, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            signTypedData(state, _deepLink, typedData, from);

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
    }
}
