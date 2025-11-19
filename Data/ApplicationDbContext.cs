using Microsoft.EntityFrameworkCore;
using caso_de_uso_6_ejercer_turno.Models.Domain;

namespace caso_de_uso_6_ejercer_turno.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cuenta> Cuentas { get; set; }
    }
}