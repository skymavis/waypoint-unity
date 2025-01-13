using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyMavis.Utils;
using UnityEngine;

namespace SkyMavis.WaypointInternal.Adapters
{
    internal class OverlayAdapter : IAdapter
    {
        private const int BufferSize = 4 * 1024;

        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });

        private readonly Action<string> _messageReceivedCallback;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private ClientWebSocket _ws = new ClientWebSocket();

        internal OverlayAdapter(WaypointSettings settings, Action<string> messageReceivedCallback)
        {
            _messageReceivedCallback = messageReceivedCallback;
            InitializeWebSocket(new UriBuilder()
            {
                Scheme = "ws",
                Host = "127.0.0.1",
                Port = settings.mavisHubPort,
                Query = $"sessionId={settings.mavisHubSessionID}",
            }.Uri);
        }

        public void Dispose()
        {
            try { _cts.Cancel(); }
            finally { _cts.Dispose(); }

            _semaphore.Dispose();

            try { _ws.Abort(); }
            finally { _ws.Dispose(); }
        }

        bool IAdapter.IsConnected => _ws.State == WebSocketState.Open;

        void IAdapter.Authorize(string state, string scope) =>
            SendWebSocket("id:token:request", state, null, null);

        void IAdapter.SendTransaction(string state, string to, string data, string value, string from) =>
            SendWebSocket(
                "provider:request",
                state,
                new
                {
                    method = "eth_sendTransaction",
                    @params = new
                    {
                        to,
                        value,
                        data,
                        from,
                    },
                },
                from
            );

        void IAdapter.PersonalSign(string state, string message, string from) =>
            SendWebSocket(
                "provider:request",
                state,
                new
                {
                    method = "personal_sign",
                    @params = message,
                },
                from
            );

        void IAdapter.SendNativeToken(string state, string to, string value, string from) =>
            (this as IAdapter).SendTransaction(state, to, null, value, from);

        void IAdapter.SignTypedData(string state, string typedData, string from) =>
            SendWebSocket(
                "provider:request",
                state,
                new
                {
                    method = "eth_signTypedData_v4",
                    @params = typedData,
                },
                from
            );

        private void InitializeWebSocket(Uri uri)
        {
            var ct = _cts.Token;
            MaintainConnectionAsync();
            StartReceiveLoopAsync();

            async void MaintainConnectionAsync()
            {
                var retryCount = 0;

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        switch (_ws.State)
                        {
                            case WebSocketState.None:
                                await _ws.ConnectAsync(uri, ct);
                                Waypoint.Logger.LogFormat(LogType.Log, "[Waypoint] Connected to Mavis Hub.");
                                retryCount = 0;
                                break;
                            case WebSocketState.Closed:
                                retryCount = Math.Min(retryCount + 1, 5);
                                var interval = CalculateRetryInterval();
                                Waypoint.Logger.LogFormat(LogType.Warning, "[Waypoint] Not connected to Mavis Hub. Retrying after {0} seconds...", interval.TotalSeconds);
                                await Task.Delay(interval, ct);
                                _ws.Dispose();
                                _ws = new ClientWebSocket();
                                await _ws.ConnectAsync(uri, ct);
                                Waypoint.Logger.LogFormat(LogType.Log, "[Waypoint] Connected to Mavis Hub after {0} retries.", retryCount);
                                retryCount = 0;
                                break;
                            case WebSocketState.Connecting:
                            case WebSocketState.Open:
                            case WebSocketState.CloseSent:
                            case WebSocketState.CloseReceived:
                                await Task.Yield();
                                break;
                            case WebSocketState.Aborted:
                                Waypoint.Logger.LogFormat(LogType.Warning, "[Waypoint] WebSocket aborted. Creating new WebSocket instance...");
                                _ws.Dispose();
                                _ws = new ClientWebSocket();
                                break;
                        }
                    }
                    catch (WebSocketException e)
                    {
                        Waypoint.Logger.LogException(e);
                    }
                    catch (OperationCanceledException) { }
                }

                TimeSpan CalculateRetryInterval()
                {
                    var interval = 3;

                    for (var i = retryCount - 1; i > 0; i--)
                    {
                        interval *= 2;
                    }

                    return TimeSpan.FromSeconds(interval);
                }
            }

            async void StartReceiveLoopAsync()
            {
                var stringBuilder = new StringBuilder(BufferSize);
                var buffer = new ArraySegment<byte>(new byte[BufferSize]);

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        if (_ws.State != WebSocketState.Open)
                        {
                            stringBuilder.Clear();
                        }

                        while (_ws.State != WebSocketState.Open)
                        {
                            await Task.Yield();
                            ct.ThrowIfCancellationRequested();
                        }

                        var result = await _ws.ReceiveAsync(buffer, ct);
                        Waypoint.Logger.LogFormat(LogType.Log, "[Waypoint] Received {0} bytes {1} frame.", result.Count, result.EndOfMessage ? "final" : null);
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer.Array, 0, result.Count));

                        if (result.EndOfMessage)
                        {
                            var message = stringBuilder.ToString();
                            stringBuilder.Clear();

                            switch (result.MessageType)
                            {
                                case WebSocketMessageType.Binary:
                                    Waypoint.Logger.Log("[Waypoint] Received unsupported binary message from Mavis Hub.");
                                    break;
                                case WebSocketMessageType.Close:
                                    Waypoint.Logger.LogFormat(LogType.Warning, "[Waypoint] Received close signal from Mavis Hub with the message:\n{0}", message);
                                    await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close received", ct);
                                    break;
                                case WebSocketMessageType.Text:
                                    Waypoint.Logger.LogFormat(LogType.Log, "[Waypoint] End of message reached. Invoking callback with the message:\n{0}", message);
                                    _messageReceivedCallback?.Invoke(message);
                                    break;
                            }
                        }
                    }
                    catch (WebSocketException e)
                    {
                        Waypoint.Logger.LogException(e);
                        _ws.Abort();
                    }
                    catch (OperationCanceledException) { }
                }
            }
        }

        private async void SendWebSocket(string eventName, string state, object data, string from)
        {
            await _semaphore.WaitAsync(_cts.Token);

            try
            {
                using var stream = new WebSocketMessageStream(_ws);
                using var buffer = new BufferedStream(stream, BufferSize);
                using var streamWriter = new StreamWriter(buffer, new UTF8Encoding(false));
                using var jsonWriter = new JsonTextWriter(streamWriter);
                JsonSerializer.Serialize(jsonWriter, new
                {
                    eventName,
                    state,
                    data,
                    from,
                });
                await jsonWriter.FlushAsync(_cts.Token);
                Waypoint.Logger.LogFormat(LogType.Log, "[Waypoint] Sent event {0} with size of {1} bytes.", eventName, stream.Length);
            }
            catch (WebSocketException)
            {
                _ws.Abort();
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
