#if UNITY_ANDROID
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SkyMavis.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SkyMavis
{
    public static partial class Waypoint
    {
        private static AndroidJavaObject _client;

        private static void Authorize_internal(string state) {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            _client.Call("authorize", context, state, _deeplink);
        }

        private static void PersonalSign_internal(string state, string message, string from = null) {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            if (from != null)
            {
                _client.Call("personalSign", context, state, _deeplink, message, from);
            }
            else
            {
                _client.Call("personalSign", context, state, _deeplink, message);
            }
        }

        private static void SignTypedData_internal(string state, string typedData, string from = null) {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            if (from != null)
            {
                _client.Call("signTypedData", context, state, _deeplink, typedData, from);
            }
            else
            {
                _client.Call("signTypedData", context, state, _deeplink, typedData);
            }
        }

        private static void SendTransaction_internal(string state, string receiverAddress, string value, string from = null) {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            if (from != null)
            {
                _client.Call("sendTransaction", context, state, _deeplink, receiverAddress, value, from);
            }
            else
            {
                _client.Call("sendTransaction", context, state, _deeplink, receiverAddress, value);
            }
        }

        private static void CallContract_internal(string state, string contractAddress, string data, string value = "0x0", string from = null)
        {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            if (from != null)
            {
                _client.Call("callContract", context, state, _deeplink, contractAddress, data, value, from);
            }
            else
            {
                _client.Call("callContract", context, state, _deeplink, contractAddress, data, value);
            }
        }
    }
}
#endif
