using DominandoEFCoreFinal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DominandoEFCoreFinal.Data;

public class ApplicationContext : DbContext
{
    public DbSet<Pessoa> Pessoas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string strConnection = "Server=127.0.0.1,1433\\rssstore-sql-server;Database=DominandoEntityFinal;MultipleActiveResultSets=true;User Id=sa;Password=MeuDB@123;pooling=true;MultipleActiveResultSets=true";
        if(!optionsBuilder.IsConfigured)
            optionsBuilder
            .UseSqlServer(strConnection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pessoa>(conf => 
        {
            conf.HasKey(x => x.Id);
            //unicode especifica se é NVARCHAR ou VARCHAR
            conf.Property(x => x.Nome).HasMaxLength(60).IsUnicode(false);
        });
    }
}
