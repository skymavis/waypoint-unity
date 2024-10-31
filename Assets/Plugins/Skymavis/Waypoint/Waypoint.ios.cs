#if UNITY_IOS
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using SkyMavis.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SkyMavis
{
    public static partial class Waypoint
    {
        private static void Authorize_internal(string state) {
            authorize(state, _deeplink);
        }

        private static void PersonalSign_internal(string state, string message, string from = null) {
            personalSign(state, _deeplink, message, from);
        }

        private static void SignTypedData_internal(string state, string typedData, string from = null) {
            signTypedData(state, _deeplink, typedData, from);
        }

        private static void SendTransaction_internal(string state, string receiverAddress, string value, string from = null) {
            sendTransaction(state, _deeplink, receiverAddress, value, from);
        }

        private static void CallContract_internal(string state, string contractAddress, string data, string value = "0x0", string from = null)
        {
            callContract(state, _deeplink, contractAddress, data, value, from);
        }

        [DllImport("__Internal")]
        private static extern void initClient(string waypointOrigin, string clientId, string chainRpc, int chainId);

        [DllImport("__Internal")]
        private static extern void authorize(string state, string redirects);

        [DllImport("__Internal")]
        private static extern void sendTransaction(string state, string redirect, string to, string value, string from = null);

        [DllImport("__Internal")]
        private static extern void personalSign(string state, string redirect, string message, string from = null);

        [DllImport("__Internal")]
        private static extern void signTypedData(string state, string redirect, string typedData, string from = null);

        [DllImport("__Internal")]
        private static extern void callContract(string state, string redirect, string contractAddress, string data, string value = "0x0", string from = null);
    }
}
#endif
