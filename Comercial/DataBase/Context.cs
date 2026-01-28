using Comercial.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace Comercial.DataBase
{
    public partial class Context : DbContext
    {
        public Context() { }
        public Context(DbContextOptions<Context> options) : base(options) { }

        private DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public virtual DbSet<ComercialPropostaViewQuadroPrecoModel> PropostaViewQuadroPrecos { get; set; }

        static Context() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                $"host={BaseSettings.Host};" +
                $"user id={BaseSettings.Username};" +
                $"password={BaseSettings.Password};" +
                $"database={BaseSettings.Database};" +
                $"Pooling=false;" +
                $"Timeout=300;" +
                $"CommandTimeout=300;" +
                $"Application Name=SIG Comercial <{BaseSettings.Database}>;",
                options => { options.EnableRetryOnFailure(); }
                );
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
