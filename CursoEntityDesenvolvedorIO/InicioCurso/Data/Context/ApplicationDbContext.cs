using System.Reflection;
using System.Text;
using DominandoEFCoreDevIo.Conversores;
using DominandoEFCoreDevIo.Data.Functions;
using DominandoEFCoreDevIo.Domain.Entidades;
using DominandoEFCoreDevIo.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace DominandoEFCoreDevIo.Data.Context;
public class ApplicationDbContext : DbContext
{
    private readonly StreamWriter _writer = new StreamWriter("log_entity_curso.txt", append: true,encoding: Encoding.UTF8);
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Conversor> Conversores { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Ator> Atores { get; set; }
    public DbSet<Filme> Filmes { get; set; }
    public DbSet<Funcao> Funcoes { get; set; }
    public DbSet<Dictionary<string, object>> Configuracoes => Set<Dictionary<string, object>>("Configuracoes");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //polling para pegar uma conexão do poll de conexões do banco
        //MultipleActiveResultSets para permitir que a mesma conexão faça varias consultas simultaneas
        const string strConnection = "Server=127.0.0.1,1433\\rssstore-sql-server;Database=DominandoEntity;MultipleActiveResultSets=true;User Id=sa;Password=MeuDB@123;pooling=true;MultipleActiveResultSets=true";

        if(!optionsBuilder.IsConfigured)
            optionsBuilder
            .UseSqlServer(strConnection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)//Apontando o tipo de rastreamento global das consultas
            .AddInterceptors(CommandInterceptor.Instance, DbConnectInterceptor.Instance, PersistenceInterceptor.Instance)
            //.UseSqlServer(strConnection, options => options.MaxBatchSize(100).CommandTimeout(15)) // O padrão do batch size é 42, podemos reconfigurar para até 2100 quantidade maxima de parametros sqlserver, recomendado para redes mais instáveis
            .UseSqlServer(strConnection, options => options.EnableRetryOnFailure(4, TimeSpan.FromSeconds(10), null)) //Habilitar o retry em casos de falhas, o padrão é 6 retries durante 30 seg 
            //.UseSqlServer(strConnection, options => { options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); })
            .EnableSensitiveDataLogging() //Habilita a visualização de dados que estão sendo enviados para o banco
            //.UseLazyLoadingProxies()//Habilitar o lazy load global
            .LogTo(Console.WriteLine, LogLevel.Information)
            // .LogTo(Console.WriteLine, 
            // new [] {CoreEventId.ContextInitialized, RelationalEventId.CommandExecuted},
            // LogLevel.Information, DbContextLoggerOptions.LocalTime | DbContextLoggerOptions.SingleLine);//Permite filtrar os tipos de eventos que serão mostrados nos logs
            //.LogTo(_writer.Write,LogLevel.Information)//Criar arquivo texto com logs
            //.EnableDetailedErrors() //Trazer erros mais detalhados, utilizar somente em desenvolvimento para evitar sobrecarga de logs
            ;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Filtro global na tabela que é aplicado a todas consultas
        //Impedindo trazer departamentos desativados logicamente
        modelBuilder.Entity<Departamento>().HasQueryFilter(x => !x.Excluido);

        //DbFunctionsMapping.RegistrarFuncoes(modelBuilder);

        //mapeamento função built in via fluent api
        modelBuilder.HasDbFunction(typeof(DbFunctionsMapping)
                    .GetRuntimeMethod("Left", new[] { typeof(string), typeof(int) }))
                    .HasName("LEFT")
                    .IsBuiltIn();

        //ao criar o banco diz que todas as tabelas devem ser Case Insensitive e Acentuação Ignorada
        //Padrão do sqlserver/Mysql mas não é no Postregres/SqLite
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AI");

        //Podemos indicar que apenas um campo possui case sensitive e acentuação sensitiva
        modelBuilder.Entity<Departamento>()
                    .Property(x => x.Descricao)
                    .UseCollation("SQL_Latin1_General_CP1_CS_AS");


        modelBuilder.HasSequence<int>("MySequence", "sequencies")
                    .StartsAt(1)
                    .IncrementsBy(2)
                    .IsCyclic()
                    .HasMin(1)
                    .HasMax(1000);
        
        //Maneira de utilizar sequencias no SqlServer
        modelBuilder.Entity<Departamento>()
                    .Property(x => x.Id)
                    .HasDefaultValueSql("NEXT VALUE FOR sequencies.MySequence");

        //Indice composto
        modelBuilder.Entity<Departamento>()
                    .HasIndex(x => new { x.Ativo, x.Descricao })
                    .HasDatabaseName("IDX_Indice_Composto_Descricao_Ativo")
                    .HasFilter("Descricao IS NOT NULL") //Será indexado apenas departamentos com descrição não nula
                    .HasFillFactor(80) //Indica o fator de preenchimento por folha na paginação do SqlServer
                    .IsUnique();

        modelBuilder.HasDefaultSchema("cadastros"); //Setar esquema default para todas as tabelas

        modelBuilder.Entity<Departamento>().ToTable("Departamentos", "dbo"); //Setar esquema para uma tabela

        var conversor = new ValueConverter<Versao, string>(x => x.ToString(), x => (Versao)Enum.Parse(typeof(Versao), x));
        var conversorEnum = new EnumToStringConverter<Versao>();
        // Microsoft.EntityFrameworkCore.Storage.ValueConversion namespace que contem todos conversores suportados
        modelBuilder.Entity<Conversor>()
                        .Property(x => x.Versao)
                        //.HasConversion(conversorEnum)
                        //.HasConversion(conversor) //Podemos separar tipos especificos de conversão e apenas utilizar
                        //.HasConversion<string>() //Converte o para texto e o padrão é inteiro
                        .HasConversion(x => x.ToString(), x => (Versao)Enum.Parse(typeof(Versao), x)); //Indica via expression como salvar e como buscar

        modelBuilder.Entity<Conversor>()
                    .Property(x => x.Status)
                    .HasConversion(new ConversorCustomizado());

        //Será criada um propriedade sombra auto gerenciada pelo entity
        modelBuilder.Entity<Departamento>()
                    .Property<DateTime>("UltimaAtualizacao");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        //Cria uma tabela para configuracoes sem modelar uma classe especifica
        modelBuilder.SharedTypeEntity<Dictionary<string, object>>("Configuracoes", x =>
         {
             x.Property<int>("Id");

             x.Property<string>("Chave")
                .HasColumnType("VARCHAR(40)")
                .IsRequired();

             x.Property<string>("Valor")
                .HasColumnType("VARCHAR(255)")
                .IsRequired();
         });

         modelBuilder.Entity<Funcao>()
                    .Property<string>("PropriedadeSombra")
                    .HasColumnType("VARCHAR(50)")
                    .HasDefaultValueSql("'Teste'");

        modelBuilder.HasDbFunction(_letrasMaiusculas)
                    .HasName("ConverterParaLetrasMaiusculas")
                    .HasSchema("dbo");

        //quando mapeamos funcoes built in que contenham constantes proprias do banco
        //devemos mapear atraves de um parametro texto a criacao de uma constante sql
        modelBuilder.HasDbFunction(_dateDiff)
                    .HasName("DATEDIFF")
                    .HasTranslation(x => 
                    {
                        var argumentos = x.ToList();
                        var constante = (SqlConstantExpression)argumentos[0];
                        argumentos[0] = new SqlFragmentExpression(constante.Value.ToString());

                        return new SqlFunctionExpression("DATEDIFF", argumentos, false, new[] {false, false, false}, typeof(int), null);
                    })
                    .IsBuiltIn();
    }

    private static MethodInfo _letrasMaiusculas = typeof(DbFunctionsMapping)
        .GetRuntimeMethod(nameof(DbFunctionsMapping.LetrasMaiusculas), new[] { typeof(string) });

    private static MethodInfo _dateDiff = typeof(DbFunctionsMapping)
        .GetRuntimeMethod(nameof(DbFunctionsMapping.DateDiff), new[] { typeof(string), typeof(DateTime), typeof(DateTime) });

    public override void Dispose()
    {
        base.Dispose();
        _writer?.Dispose();
    }
}
