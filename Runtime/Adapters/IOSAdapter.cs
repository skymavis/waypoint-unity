using System.Runtime.InteropServices;

namespace SkyMavis.WaypointInternal.Adapters
{
    internal class IOSAdapter : IAdapter
    {
        private readonly string _deepLink;

        internal IOSAdapter(WaypointSettings settings)
        {
            bool isTestnet = settings.network.chainID == WaypointSettings.Network.Testnet.chainID;
            _deepLink = settings.deepLinkCallbackURL;
            initClient(settings.endpoint, settings.clientID, _deepLink, ref isTestnet);
        }

        public void Dispose() { }

        bool IAdapter.IsConnected => true;

        void IAdapter.Authorize(string state, string scope) =>
            authorize(state, scope);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            sendTransaction(state, to, data, value, from);

        void IAdapter.PersonalSign(string state, string message, string from) =>
            personalSign(state, message, from);

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            sendNativeToken(state, to, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            signTypedData(state, typedData, from);

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void initClient(string waypointOrigin, string clientId, string redirectUri, ref bool isTestnet);

        [DllImport("__Internal")]
        private static extern void authorize(string state, string scope);

        [DllImport("__Internal")]
        private static extern void sendNativeToken(string state, string to, string value, string from);

        [DllImport("__Internal")]
        private static extern void personalSign(string state, string message, string from);

        [DllImport("__Internal")]
        private static extern void signTypedData(string state, string typedData, string from);

        [DllImport("__Internal")]
        private static extern void sendTransaction(string state, string to, string data, string value, string from);
#else
        private static void initClient(string waypointOrigin, string clientId, string redirectUri, ref bool isTestnet) =>
            throw new System.NotImplementedException();

        private static void authorize(string state, string scope) =>
            throw new System.NotImplementedException();

        private static void sendNativeToken(string state, string to, string value, string from) =>
            throw new System.NotImplementedException();

        private static void personalSign(string state, string message, string from) =>
            throw new System.NotImplementedException();

        private static void signTypedData(string state, string typedData, string from) =>
            throw new System.NotImplementedException();

        private static void sendTransaction(string state, string to, string data, string value, string from) =>
            throw new System.NotImplementedException();
#endif
    }
}
