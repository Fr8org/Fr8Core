using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalSlack.RtmClient.Entities;
using terminalSlack.RtmClient.Events;
using WebSocketSharp;
using Logger = Fr8.Infrastructure.Utilities.Logging.Logger;

namespace terminalSlack.RtmClient
{
    public sealed class SlackRtmClient : IDisposable
    {
        private static readonly Dictionary<string, Func<JToken, EventBase>> BaseMessageConvertersByType;

        private static readonly Dictionary<Type, Action<SlackRtmClient, EventBase>> EventHandlersByType;

        private static readonly Dictionary<string, Func<JToken, MessageBase>> MessageSubtypeEventHandlersByType;

        static SlackRtmClient()
        {
            BaseMessageConvertersByType = new Dictionary<string, Func<JToken, EventBase>>(StringComparer.Ordinal)
            {
                { "hello", json => json.ToObject<Hello>() },
                { "message", ParseMessage },
                { "reconnect_url", json => json.ToObject<ReconnectUrl>() }
            };
            EventHandlersByType = new Dictionary<Type, Action<SlackRtmClient, EventBase>>
            {
                { typeof(Hello), (client, message) => client.OnHelloReceived() },
                { typeof(Message), (client, message) => client.OnMessageReceived((Message)message) },
                { typeof(ReconnectUrl), (client, message) => client.OnReconnectUrlReceived() },
                { typeof(MessageChanged), (client, message) => client.OnMessageChanged((MessageChanged)message) }

            };
            MessageSubtypeEventHandlersByType = new Dictionary<string, Func<JToken, MessageBase>>
            {
                { string.Empty, json => json.ToObject<Message>() },
                { "message_changed", json => json.ToObject<MessageChanged>() }
            };
        }

        private static EventBase ParseMessage(JToken json)
        {
            var subtype = json.Value<string>("subtype") ?? string.Empty;
            Func<JToken, MessageBase> converter;
            if (MessageSubtypeEventHandlersByType.TryGetValue(subtype, out converter))
            {
                return converter(json);
            }
            return null;
        }

        private WebSocket _socket;

        private bool _isDisposed;

        private Task<LoginResponse> _connectTask;

        private readonly string _oAuthToken;

        public SlackRtmClient(string oAuthToken)
        {
            _oAuthToken = oAuthToken;
        }
        public async Task<LoginResponse> ConnectAsync(CancellationToken token)
        {
            if (_connectTask == null)
            {
                _connectTask = ConnectInternalAsync(token);
            }
            return await _connectTask.ConfigureAwait(false);
        }

        private async Task<LoginResponse> ConnectInternalAsync(CancellationToken token)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"https://slack.com/api/rtm.start?token={_oAuthToken}&simple_latest&no_unreads&mpim_aware", null, token).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var loginResponse = JToken.Parse(responseContent).ToObject<LoginResponse>();
                _socket = new WebSocket(loginResponse.Url);
                _socket.OnMessage += SocketOnOnMessage;
                _socket.OnClose += SocketOnOnClose; 
                _socket.OnError += SocketOnOnError;
                await Task.Run(() => _socket.Connect(), token).ConfigureAwait(false);
                return loginResponse;
            }
        }

        private void SocketOnOnError(object sender, ErrorEventArgs e)
        {
            Logger.GetLogger().Error($"SlackRtmClient: Socket reported an error. Message - {e.Message}. Exception - {e.Exception}. Is connection alive - {(sender as WebSocket).IsAlive}");
        }

        private void SocketOnOnClose(object sender, CloseEventArgs e)
        {
            Logger.GetLogger().Info($"SlackRtmClient: Socket was closed. Code - {e.Code}. Reason - {e.Reason}. WasClean - {e.WasClean}. Is connection alive - {(sender as WebSocket).IsAlive}");
        }

        private void SocketOnOnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }
            var parsedMessage = ParseMessage(e.Data);
            string messageType;
            if (parsedMessage == null || string.IsNullOrEmpty(messageType = parsedMessage.Value<string>("type")))
            {
                return;
            }
            Func<JToken, EventBase> converter;
            var @event = BaseMessageConvertersByType.TryGetValue(messageType, out converter) ? converter(parsedMessage) : null;
            if (@event == null)
            {
                return;
            }
            Action<SlackRtmClient, EventBase> eventHandler;
            if (EventHandlersByType.TryGetValue(@event.GetType(), out eventHandler))
            {
                eventHandler(this, @event);
            }
        }

        private static JToken ParseMessage(string json)
        {
            try
            {
                return JToken.Parse(json);
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SlackRtmClient));
            }
            await Task.Run(() => _socket.Close(), token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _socket.Close();
        }

        public event EventHandler<DataEventArgs<Message>> MessageReceived;

        private void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, new DataEventArgs<Message>(message));
        }

        public event EventHandler HelloReceived;

        private void OnHelloReceived()
        {
            HelloReceived?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ReconnectUrlReceived;

        private void OnReconnectUrlReceived()
        {
            ReconnectUrlReceived?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<DataEventArgs<MessageChanged>> MessageChanged;

        private void OnMessageChanged(MessageChanged messageChanged)
        {
            MessageChanged?.Invoke(this, new DataEventArgs<MessageChanged>(messageChanged));
        }
    }
}
