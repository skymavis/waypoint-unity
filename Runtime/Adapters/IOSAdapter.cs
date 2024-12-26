using System.Runtime.InteropServices;

namespace SkyMavis.WaypointInternal.Adapters
{
    internal class IOSAdapter : IAdapter
    {
        private readonly string _deepLink;

        internal IOSAdapter(WaypointSettings settings)
        {
            _deepLink = settings.deepLinkCallbackURL;
            initClient(settings.endpoint, settings.clientID, _deepLink, settings.network.rpcURL, settings.network.chainID);
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

        void IAdapter.authAsGuest(string state, string credential, string authDate, string hash, string scope) =>
            authAsGuest(state, credential, authDate, hash, scope);

        void IAdapter.registerGuestAccount(string state) =>
            registerGuestAccount(state);

        void IAdapter.createKeylessWallet(string state) =>
            createKeylessWallet(state);

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void initClient(string waypointOrigin, string clientId, string redirectUri, string chainRpc, int chainId);

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

        [DllImport("__Internal")]
        private static extern void authAsGuest(string state, string credential, string authDate, string hash, string scope);

        [DllImport("__Internal")]
        private static extern void registerGuestAccount(string state);

        [DllImport("__Internal")]
        private static extern void createKeylessWallet(string state);
#else
        private static void initClient(string waypointOrigin, string clientId, string redirectUri, string chainRpc, int chainId) =>
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

        private static void authAsGuest(string state, string credential, string authDate, string hash, string scope) =>
            throw new System.NotImplementedException();

        private static void registerGuestAccount(string state) =>
            throw new System.NotImplementedException();

        private static void createKeylessWallet(string state) =>
            throw new System.NotImplementedException();
#endif
    }
}
