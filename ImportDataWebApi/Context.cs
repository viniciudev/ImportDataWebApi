using Microsoft.EntityFrameworkCore;
using static ImportDataWebApi.Controllers.ClientsController;


namespace ImportDataWebApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> tb_client { get; set; }
        public DbSet<ClientClinic> tb_clientClinic { get; set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //{
        //	if (!options.IsConfigured)
        //	{
        //              options.UseSqlServer(@"Server=db-homolog-igic.database.windows.net;Database=homologdb_igic;User Id=homologadmin;Password=r43mwT03vdXbqS1eH0CeEnDAB;trustservercertificate=true;");

        //              //options.UseSqlServer(@"Server=igic-database-server.database.windows.net;Database=producaodb;User Id=igicadmin;Password=LTJY!DpfAt4WyXVgsz7GSU;trustservercertificate=true;");
        //	}
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //ConfiguraClient(modelBuilder);
            //ConfigureClientClinic(modelBuilder);
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
         .SelectMany(t => t.GetForeignKeys())
         .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);
            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    var attributes = property
                            .PropertyInfo
                            .GetCustomAttributes(typeof(SensitiveDataAttribute), false);

                    if (attributes.Length > 0)
                    {
                        property.SetValueConverter(new DataProtectionConverter());
                    }
                }
            }
        }

        //private void ConfiguraClient(ModelBuilder modelBuilder)
        //{
        //	modelBuilder.Entity<Client>(client =>
        //	{
        //		client.ToTable("tb_client");
        //		client.HasKey(c => c.Id);
        //		client.Property(c => c.Id).ValueGeneratedOnAdd();
        //	});
        //	}
        //private void ConfigureClientClinic(ModelBuilder builder)
        //{
        //builder.Entity<ClientClinic>(client =>
        //{
        //	client.ToTable("tb_clientClinic");
        //	client.HasKey(c => c.Id);
        //	client.Property(c => c.Id).ValueGeneratedOnAdd();
        //});
        //builder.Entity<ClientClinic>()
        //	 .HasKey(uc => new { uc.IdClient, uc.IdClinic });
        //builder.Entity<ClientClinic>()
        //	 .HasOne(cc => cc.Client)
        //	 .WithMany(c => c.ClientClinics)
        //	 .HasForeignKey(cc => cc.IdClient);
        //builder.Entity<ClientClinic>()
        //	 .HasOne(cc => cc.Clinic)
        //	 .WithMany(c => c.ClientClinics)
        //	 .HasForeignKey(cc => cc.IdClinic);
        //}
    }
}
