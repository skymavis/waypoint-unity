namespace SkyMavis.Waypoint.Adapters
{
    internal class OverlayAdapter : IAdapter
    {
        internal OverlayAdapter(WaypointSettings settings)
        {
            Overlay.Initialize(settings.MavisHubSessionID, settings.MavisHubPort);
        }

        public void Dispose() { }

        bool IAdapter.IsConnected => Overlay.IsConnected;

        void IAdapter.Authorize(string state, string scope) =>
            Overlay.GetIDToken(state);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            Overlay.SendTransaction(state, to, value, data, from);

        void IAdapter.PersonalSign(string state, string message, string from) =>
            Overlay.PersonalSign(state, message, from);

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            Overlay.SendNativeToken(state, to, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            Overlay.SignTypedData(state, typedData, from);
    }
}
