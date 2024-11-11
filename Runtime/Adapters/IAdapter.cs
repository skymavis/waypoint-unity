using System;

namespace SkyMavis.WaypointInternal.Adapters
{
    internal interface IAdapter : IDisposable
    {
        internal bool IsConnected { get; }
        internal void Authorize(string state, string scope = null);
        internal void PersonalSign(string state, string message, string from = null);
        internal void SignTypedData(string state, string typedData, string from = null);
        internal void SendNativeToken(string state, string to, string value, string from = null);
        internal void SendTransaction(string state, string to, string data, string value = "0x0", string from = null);
    }
}
