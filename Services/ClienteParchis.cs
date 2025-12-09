using System.Net.WebSockets;
using System.Text;

public class ClienteParchis
{
    public async Task ConectarYJugar()
    {
        using (ClientWebSocket ws = new ClientWebSocket())
        {
            // Conectar
            Uri serverUri = new Uri("ws://192.168.100.17:9000/parchis");
            await ws.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Conectado al servidor via C#");

            // Enviar mensaje "roll"
            string mensaje = "{\"type\":\"roll\"}";
            byte[] bytes = Encoding.UTF8.GetBytes(mensaje);

            await ws.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            // Aquí deberías tener un bucle para recibir mensajes (ReceiveAsync)
        }
    }
}