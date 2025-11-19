using System;
using System.ComponentModel.DataAnnotations;


namespace caso_de_uso_6_ejercer_turno.Models.Domain
{
    public class Cuenta
    {
        [Key]                   
        public int IdUsuario { get; set; }

        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        public string NombreUsuarioNormalizado { get; set; }

        [Required]
        public string CorreoElectronico { get; set; }

        public bool CorreoElectronicoConfirmado { get; set; } = true;

        [Required]
        public string ContraseñaHash { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaAlta { get; set; } = DateTime.Now;

        public string UsuarioAlta { get; set; } = "Sistema";
    }
}