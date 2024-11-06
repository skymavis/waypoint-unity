#if UNITY_STANDALONE
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkyMavis.Waypoint.Types;
using SkyMavis.Waypoint.Utils;
using UnityEngine.Events;
using UnityEngine;
using WebSocketSharp;

namespace SkyMavis.Waypoint
{
    internal static partial class Overlay
    {
        private class OverlayHook : MonoBehaviour
        {
            public UnityAction onUpdate;
            public UnityAction onDestroy;
            private void Update()
            {
                onUpdate?.Invoke();
            }

            private void OnDestroy()
            {
                onDestroy?.Invoke();
            }
        }

        const int PING_INTERVAL_MS = 5000;

        private static WebSocket _socket = null;
        private static string _url = null;
        private static string _sessionID = null;
        private static long _lastPingTimeMS;
        private static long _lastPongTimeMS;
        private static bool _isTerminated = true;
        private static bool _isConnected = false;
        public static bool IsConnected => _isConnected;
        private static int _retry = 0;
        private static float _retryDelay = 0;

        private static long NowUtcMS
        {
            get
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return Convert.ToInt64((System.DateTime.UtcNow - epoch).TotalMilliseconds);
            }
        }

        private static void InitializeSocket(int port, string sessionID)
        {
            _url = $"ws://127.0.0.1:{port}/?sessionId={sessionID}";
            _sessionID = sessionID;

            var gameObject = new GameObject("OverlayHook", typeof(OverlayHook));
            var behaviourHook = gameObject.GetComponent<OverlayHook>();
            behaviourHook.onUpdate = OnUpdate;
            behaviourHook.onDestroy = OnDestroy;
            GameObject.DontDestroyOnLoad(gameObject);

            UnityThread.initUnityThread();
            Connect();
        }

        private static float NextRetryDelay()
        {
            _retry++;
            switch (_retry)
            {
                case 0: return 3;
                case 1: return 6;
                case 2: return 12;
                case 3: return 24;
                case 4: return 48;
                default: return 96;
            }
        }

        private static void Connect()
        {
            if (string.IsNullOrEmpty(_url)) return;
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            // Initialize socket
            _socket = new WebSocket(_url);

            // Register event listeners
            _socket.OnOpen += (sender, e) =>
            {
                Log("socket Connected");
                _isConnected = true;
                _initialized = true;
                _retry = 0;
            };
            _socket.OnClose += (sender, e) =>
            {
                Log("socket Disconnect: " + e);
                _retryDelay = NextRetryDelay();
                _isTerminated = true;
                _socket = null;
            };
            _socket.OnError += (sender, e) =>
            {
                LogError("socket Error: " + e);
            };

            _socket.OnMessage += (sender, args) =>
            {
                GotPong();
                UnityThread.executeInUpdate(() =>
                {
                    if (args.IsText)
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            ReceiveText(args.Data);
                        }
                    }
                    else if (args.IsBinary)
                    {
                        if (args.RawData != null && args.RawData.Length > 0)
                        {
                            ReceiveBytes(args.RawData);
                        }
                    }
                });

            };

            // Open connection
            Log("socket Connecting...");
            _isTerminated = false;
            _isConnected = false;
            _lastPingTimeMS = 0;
            _lastPongTimeMS = 0;
            _socket.ConnectAsync();
        }

        private static void OnUpdate()
        {
            if (_isTerminated)
            {
                if (_retryDelay > 0)
                {
                    _retryDelay -= Time.unscaledDeltaTime;
                }
                else if (!_isConnected && !string.IsNullOrEmpty(_url))
                {
                    Connect();
                }
            }
            else if (_isConnected)
            {
                if (NowUtcMS - _lastPingTimeMS > PING_INTERVAL_MS)
                {
                    _lastPingTimeMS = NowUtcMS;
                    _socket.PingAsync((b) => { if (b) GotPong(); });
                }
            }
        }

        private static void OnDestroy()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
                _isTerminated = true;
                _isConnected = false;
            }
            _url = null;
        }

        private static void GotPong()
        {
            _lastPongTimeMS = NowUtcMS;
        }

        private static void ReceiveText(string text)
        {
            try
            {
                OnDataResponsed?.Invoke(text);
            }
            catch (Exception)
            {
                var error = new OverlayError { code = -1, message = "invalid Overlay response" };
                LogError(error, "[Overlay] On response: ");
            }
        }

        private static void ReceiveBytes(byte[] data)
        {
        }

        private static void RequestToOverlay<T>(string eventName, string state, T requestData, string from = null)
        {
            ThrowIfNotConnected();

            JObject jRequest = new JObject
            {
                new JProperty("eventName", eventName),
                new JProperty("state", state),
                new JProperty("data", JObject.FromObject(requestData))
            };
            if (from != null)
            {
                jRequest.Add(new JProperty("from", from));
            }
            SendToOverlay(jRequest.ToString());
        }

        private static void RequestToOverlay(string eventName, string state)
        {
            ThrowIfNotConnected();

            JObject jRequest = new JObject
            {
                new JProperty("eventName", eventName),
                new JProperty("state", state),
                new JProperty("data", null)
            };
            SendToOverlay(jRequest.ToString());
        }

        private static void SendToOverlay(string jData)
        {
            Log(jData, $"[Overlay] On request: ");
            long startRequestTimeMs = NowUtcMS;
            _lastPingTimeMS = startRequestTimeMs;

            _socket.SendAsync(jData, b =>
            {
                if (!b)
                {
                    UnityThread.executeInUpdate(() => Log("SendAsync error"));
                }
            });

        }

        private static void ThrowIfNotInitialized()
        {
            if (!_initialized)
                throw new Exception($"[Overlay] SDK is not initialized!");
        }

        private static void ThrowIfNotConnected()
        {
            if (!_isConnected)
            {
                _retry = 0;
                _retryDelay = Mathf.Min(_retryDelay, 1);
                throw new Exception($"[Overlay] Overlay is not connected!");
            }
        }

        private static (T, OverlayError) WithInvoke<T>((T, OverlayError) value, Action<(T, OverlayError)> action)
        {
            action?.Invoke(value);
            return value;
        }

        internal static void Log(string content, string prefix = "[Overlay] ")
        {
            if (_verboseLog)
            {
                Debug.Log(prefix + content);
            }
        }

        internal static void LogObject(object obj, string prefix = "[Overlay] ")
        {
            if (_verboseLog)
            {
                string content = JsonConvert.SerializeObject(obj);
                Debug.Log(prefix + content);
            }
        }

        internal static void LogError(string content, string prefix = "[Overlay] ")
        {
            if (_verboseLog)
            {
                Debug.LogError(prefix + content);
            }
        }
        internal static void LogError(OverlayError err, string prefix = "[Overlay] ")
        {
            if (_verboseLog)
            {
                string content = JsonConvert.SerializeObject(err);
                Debug.LogError(prefix + content);
            }
        }
    }
}
#endif
