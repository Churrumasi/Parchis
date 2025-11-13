using System.Collections.Generic;

namespace caso_de_uso_6_ejercer_turno.Models.Domain
{
    public class GameState
    {
        public string IdPartida { get; set; } = System.Guid.NewGuid().ToString();
        public List<Player> Jugadores { get; set; } = new List<Player>();
        public int IndiceJugadorActual { get; set; } = 0;
        public string Estado { get; set; } = "en curso";
    }
}
