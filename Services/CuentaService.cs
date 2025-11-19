using System.Threading.Tasks;
using caso_de_uso_6_ejercer_turno.Events;
using caso_de_uso_6_ejercer_turno.Models;
using caso_de_uso_6_ejercer_turno.Models.Events;
using caso_de_uso_6_ejercer_turno.Data;
using caso_de_uso_6_ejercer_turno.Models.Domain;
using Microsoft.EntityFrameworkCore;


namespace caso_de_uso_6_ejercer_turno.Services
{
    public class CuentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly UsuarioRegistradoEventHandler _eventHandler;

        public CuentaService(ApplicationDbContext context,UsuarioRegistradoEventHandler handler)
        {
            _context = context;
            _eventHandler = handler;
        }

public async Task<bool> RegistrarUsuarioAsync(RegisterViewModel model)
{
    string usuarioNormalizado = model.Username.Trim().ToUpper();

    bool existe = await _context.Cuentas
        .AnyAsync(u => u.NombreUsuarioNormalizado == usuarioNormalizado);

    if (existe)
        return false;

    var cuenta = new Cuenta
    {
        NombreUsuario = model.Username.Trim(),
        NombreUsuarioNormalizado = usuarioNormalizado,
        CorreoElectronico = model.Email.Trim(),
        CorreoElectronicoConfirmado = false,
        ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
        Activo = true,
        FechaAlta = DateTime.Now,
        UsuarioAlta = model.Username
    };

    _context.Cuentas.Add(cuenta);
    await _context.SaveChangesAsync();

    var evento = new UsuarioRegistradoEvent(model.Username, model.Email);
    await _eventHandler.HandleAsync(evento);

    return true;
}

public async Task<Cuenta?> LoginAsync(string username, string password)
{
    string normalizado = username.Trim().ToUpper();

    var usuario = await _context.Cuentas
        .FirstOrDefaultAsync(u => u.NombreUsuarioNormalizado == normalizado);

    if (usuario == null)
        return null;

    bool contraseñaCorrecta = BCrypt.Net.BCrypt.Verify(password, usuario.ContraseñaHash);

    return contraseñaCorrecta ? usuario : null;
        }

    }
}
