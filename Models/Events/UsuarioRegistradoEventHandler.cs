using caso_de_uso_6_ejercer_turno.Models.Events;
using System;
using System.Threading.Tasks;

namespace caso_de_uso_6_ejercer_turno.Events
{
    public class UsuarioRegistradoEventHandler
    {
        public Task HandleAsync(UsuarioRegistradoEvent e)
        {
            Console.WriteLine($"[EVENTO] Nuevo usuario registrado: {e.Username}, email: {e.Email}");

            // Aquí puedes poner lógicas reales:
            // - enviar email de bienvenida
            // - guardar log de auditoría
            // - crear avatar por defecto
            // - asignar estadísticas iniciales

            return Task.CompletedTask;
        }
    }
}
