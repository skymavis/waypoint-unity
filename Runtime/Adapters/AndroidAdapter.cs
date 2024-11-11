using SkyMavis.Waypoint.Adapters;
using UnityEngine;

namespace SkyMavis.Waypoint
{
    internal class AndroidAdapter : IAdapter
    {
        private string _deepLink;
        private AndroidJavaObject _client;
        private AndroidJavaObject _context;

        internal AndroidAdapter(string clientID, string deepLink, string endpoint, string rpcURL, int chainID)
        {
            using var jPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var jChainID = new AndroidJavaObject("java.lang.Integer", chainID);
            _deepLink = deepLink;
            _client = new AndroidJavaObject("com.skymavis.sdk.waypoint.Waypoint", endpoint, clientID, rpcURL, jChainID);
            _context = jPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        public void Dispose()
        {
            _client.Dispose();
            _context.Dispose();
        }

        bool IAdapter.IsConnected => true;

        void IAdapter.Authorize(string state, string scope) =>
            _client.Call("authorize", _context, state, _deepLink, scope);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            _client.Call("sendTransaction", _context, state, _deepLink, to, data, value, from);

        void IAdapter.PersonalSign(string state, string message, string from) =>
            _client.Call("personalSign", _context, state, _deepLink, message, from);

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            _client.Call("sendNativeToken", _context, state, _deepLink, to, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            _client.Call("signTypedData", _context, state, _deepLink, typedData, from);
    }
}
