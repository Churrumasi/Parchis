using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class ClienteParchis
{
    private TcpClient _cliente;
    public int PlayerId { get; private set; }
    public string Color { get; private set; }

    public async Task ConectarAsync(string host = "localhost", int puerto = 9000)
    {
        _cliente = new TcpClient();
        await _cliente.ConnectAsync(host, puerto);

        // Leer asignación
        var stream = _cliente.GetStream();
        var buffer = new byte[1024];
        int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
        var json = Encoding.UTF8.GetString(buffer, 0, bytes);
        var asignacion = JsonSerializer.Deserialize<JsonElement>(json);
        PlayerId = asignacion.GetProperty("player").GetInt32();
        Color = asignacion.GetProperty("color").GetString();
    }
}
