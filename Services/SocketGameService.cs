using System.Net.WebSockets;
using System.Text;
using System.Text.Json; // Usamos System.Text.Json por defecto

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class SocketGameService
    {
        private ClientWebSocket _ws;

        public SocketGameService()
        {
            _ws = new ClientWebSocket();
        }

        // ✅ SOLUCIÓN ERROR 1: Propiedad 'Conectado' que faltaba
        public bool Conectado => _ws.State == WebSocketState.Open;

        // ✅ SOLUCIÓN ERROR 2: Método 'ConectarAsync' que acepta IP y Puerto
        public async Task ConectarAsync(string ip, int port)
        {
            // Si el socket murió o se cerró, hay que crear uno nuevo porque no son reutilizables
            if (_ws.State == WebSocketState.Closed || _ws.State == WebSocketState.Aborted)
            {
                _ws = new ClientWebSocket();
            }

            if (_ws.State != WebSocketState.Open)
            {
                // Construimos la URL usando la IP y Puerto que recibimos
                var uri = new Uri($"ws://{ip}:{port}/parchis");
                await _ws.ConnectAsync(uri, CancellationToken.None);
            }
        }

        public async Task EnviarAsync(object data)
        {
            if (!Conectado) return;

            string json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _ws.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        public async Task<string> RecibirAsync()
        {
            if (!Conectado) return null;

            var buffer = new byte[4096];
            var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cierre servidor", CancellationToken.None);
                return null;
            }

            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
    }
}