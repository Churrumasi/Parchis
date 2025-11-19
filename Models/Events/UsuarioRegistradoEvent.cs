namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class UsuarioRegistradoEvent
    {
        public string Username { get; }
        public string Email { get; }

        public UsuarioRegistradoEvent(string username, string email)
        {
            Username = username;
            Email = email;
        }
    }
}