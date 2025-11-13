using System.Collections.Generic;

namespace caso_de_uso_6_ejercer_turno.Models.Domain
{
    public class Player
    {
        public string IdJugador { get; set; } = System.Guid.NewGuid().ToString();
        public string Nombre { get; set; } = "Jugador";
        public string ColorFichas { get; set; } = "rojo";
        public List<int> PosicionesFichas { get; set; } = new List<int> { -1, -1, -1, -1 };
        public string Estado { get; set; } = "esperando";
    }
}
