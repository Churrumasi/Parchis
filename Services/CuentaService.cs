using System.Threading.Tasks;
using caso_de_uso_6_ejercer_turno.Events;
using caso_de_uso_6_ejercer_turno.Models;
using caso_de_uso_6_ejercer_turno.Models.Events;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class CuentaService
    {
        private readonly UsuarioRegistradoEventHandler _eventHandler;

        public CuentaService(UsuarioRegistradoEventHandler handler)
        {
            _eventHandler = handler;
        }

        public async Task RegistrarUsuarioAsync(RegisterViewModel model)
        {
            // Aqu vamos a guardar en la base de dato we
            // Ej: _context.Usuarios.Add(...) y asi...

            // Lanzar evento
            var evento = new UsuarioRegistradoEvent(model.Username, model.Email);
            await _eventHandler.HandleAsync(evento);
        }
    }
}
