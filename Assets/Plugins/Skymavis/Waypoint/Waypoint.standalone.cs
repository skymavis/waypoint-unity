#if UNITY_STANDALONE
namespace SkyMavis
{
    public static partial class Waypoint
    {
        public static string OnGetIDToken() => ExecuteWithRandomState(Overlay.GetIDToken);

        private static void Authorize_internal(string state)
        {
            Overlay.GetIDToken(state);
        }

        private static void PersonalSign_internal(string state, string message, string from = null)
        {
            Overlay.PersonalSign(state, message, from);
        }

        private static void SignTypedData_internal(string state, string typedData, string from = null)
        {
            Overlay.SignTypedData(state, typedData);
        }

        private static void SendTransaction_internal(string state, string receiverAddress, string value, string from = null)
        {
            Overlay.SendNativeToken(state, receiverAddress, value, from);
        }

        private static void CallContract_internal(string state, string contractAddress, string data, string value = "0x0", string from = null)
        {
            Overlay.SendTransaction(state, contractAddress, value, data, from);
        }
    }
}
#endif
