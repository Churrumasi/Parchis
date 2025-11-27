using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class SocketGameService
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public bool Conectado => _client?.Connected ?? false;

        public async Task<bool> ConectarAsync(string host, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(host, port);
                _stream = _client.GetStream();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task EnviarAsync(object mensaje)
        {
            var json = JsonConvert.SerializeObject(mensaje);
            var bytes = Encoding.UTF8.GetBytes(json + "\n");
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public async Task<string> RecibirAsync()
        {
            byte[] buffer = new byte[4096];
            int read = await _stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, read);
        }
    }
}