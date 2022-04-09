using System.Transactions;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using DominandoEFCoreDevIo.Data.Context;
using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using DominandoEFCoreDevIo.Data.Functions;

int contador;

Console.WriteLine("Hello, World!");
await CanConnectToDatabase();
await ConsultaComNOLOCK();

//GerarScriptBanco();


//await EnsureCreatedAndEnsureDeleted();

// new ApplicationDbContext().Departamentos.Any();

// contador = 0;
// GerenciarEstadoConexaoSincrono(false, contador);
// contador = 0;
// GerenciarEstadoConexaoSincrono(true, contador);
// contador = 0;
// await GerenciarEstadoConexao(false, contador);
// contador = 0;
// await GerenciarEstadoConexao(true, contador);

static async Task ConsultaComNOLOCK()
{
    using var db = new ApplicationDbContext();
    var resposta = await db.Departamentos
                            .TagWith("Use NOLOCK")
                            .ToListAsync();
}

static async Task EnsureCreatedAndEnsureDeleted()
{
    using var db = new ApplicationDbContext();
    //Verificar se um banco foi criado senão o cria
    await db.Database.EnsureCreatedAsync();
    //Verificar se um banco foi deletado senão deleta
    //await db.Database.EnsureDeletedAsync();
}

static async Task EnsureCreatedGapResolve()
{
    //Servico responsavel por criar, deletar , verificar 
    // se tabelas e bancos existem
    //Podemos utilizar para criar tabelas quando existem mais de um contexto
    //sem aplicar migration
    using var db = new ApplicationDbContext();
    var databaseCreator = db.GetService<IRelationalDatabaseCreator>();
    await databaseCreator.CreateTablesAsync();
}

static async Task CanConnectToDatabase()
{
    //Verifica se existe conexão com o banco de dados especificado na connection string
    //Caso o banco não exista e seja possível acessar o servido o status da conexão é inativa
    using var db = new ApplicationDbContext();
    if(await db.Database.CanConnectAsync())
        Console.WriteLine("Conexão ativa...");
    else
        Console.WriteLine("Sem conexão...");
}

static async Task GerenciarEstadoConexao(bool gerenciarEstado, int contador)
{
    using var db = new ApplicationDbContext();
    var timer = Stopwatch.StartNew();

    var conexao = db.Database.GetDbConnection();
    conexao.StateChange += (_, _) => ++contador;

    if(gerenciarEstado)
        await conexao.OpenAsync();

    for (var i = 0; i < 200; i++)
    {
        await db.Departamentos.AsNoTrackingWithIdentityResolution()
            .AnyAsync();
    }

    if(gerenciarEstado)
        await conexao.CloseAsync();

    timer.Stop();

    var mensagem = $"Tempo {timer.Elapsed.ToString()}, conexão gerenciada : {gerenciarEstado}, Contador {contador}";
    System.Console.WriteLine(mensagem);
}

//Ao antecipar a abertura da conexão estamos gerenciando ela para o entity
//Evitando que o mesmo gerencie, evitando abrir e fechar multiplas vezes
static void GerenciarEstadoConexaoSincrono(bool gerenciarEstado, int contador)
{
    using var db = new ApplicationDbContext();
    var timer = Stopwatch.StartNew();

    var conexao = db.Database.GetDbConnection();
    conexao.StateChange += (_, _) => ++contador;

    if(gerenciarEstado)
        conexao.Open();

    for (var i = 0; i < 200; i++)
    {
        db.Departamentos.AsNoTrackingWithIdentityResolution()
            .Any();
    }

    if(gerenciarEstado)
        conexao.Close();

    timer.Stop();

    var mensagem = $"Tempo {timer.Elapsed.ToString()}, conexão gerenciada : {gerenciarEstado}, Contador {contador}";
    System.Console.WriteLine(mensagem);
}

//Formas de excutar sql puro entity
static async Task ExecuteSQL()
{
    //1 opção
    using var db = new ApplicationDbContext();
    using var cmd = db.Database.GetDbConnection().CreateCommand();
    cmd.CommandText = "SELECT 1";
    await cmd.ExecuteNonQueryAsync();

    //2 opcao
    var descricao = "Teste";
    await db.Database.ExecuteSqlRawAsync("UPDATE departamentos SET descricao = {0} WHERE id = 1", descricao);

    //3 opcao
    await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE departamentos SET descricao = {descricao} WHERE id = 1");
}

//Verificar migrações pendentes
static async Task MigracoesPendentes()
{
    using var db = new ApplicationDbContext();
    var migrationsPending = await db.Database.GetPendingMigrationsAsync();

    System.Console.WriteLine($"Total de migrações pendentes:{migrationsPending.Count()}");

    foreach(var item in migrationsPending) 
        System.Console.WriteLine($"Migração:{item} está pendente");
}

//mostra todas migrações na aplicação
//não consulta a tabela de migrations no banco
static void TodasMigracoes()
{
    using var db = new ApplicationDbContext();
    var migrations = db.Database.GetMigrations();

    System.Console.WriteLine($"Total de migrações pendentes:{migrations.Count()}");

    foreach(var item in migrations) 
        System.Console.WriteLine($"Migração:{item} está pendente");
}

//Busca todas migrations aplicadas gravadas na respectiva tabela
//O comando dotnet ef migrations --list --context "NomeContexto" mostra todas aplicas e pendentes
static async Task MigracoesAplicadas()
{
    using var db = new ApplicationDbContext();
    var migrations = await db.Database.GetAppliedMigrationsAsync();

    System.Console.WriteLine($"Total de migrações pendentes:{migrations.Count()}");

    foreach(var item in migrations) 
        System.Console.WriteLine($"Migração:{item} está pendente");
}

//força a aplicação de migrations em tempo de execução
//Em um cenário com escala pode ser problematico pois todas aplicações tentarão aplicar a migration
static async Task AplicarMigracaoEmTempoDeExecucao()
{
    using var db = new ApplicationDbContext();

    if((await db.Database.GetPendingMigrationsAsync()).Any())
        await db.Database.MigrateAsync();
}

//cria texto de script do banco
//Porém existe comando do ef para gerar o script fisico
static void GerarScriptBanco()
{
    using var db = new ApplicationDbContext();
    var script = db.Database.GenerateCreateScript();

    System.Console.WriteLine(script);
}

static async Task CarregamentoAdiantado()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);
    //Include gera um left join
    var departamentos = db.Departamentos
                        .Include(d => d.Funcionarios)
                        .AsNoTrackingWithIdentityResolution();

    foreach(var dep in departamentos)
    {
        System.Console.WriteLine("-------------------------");
        System.Console.WriteLine($"Departamento:{dep.Descricao}");

        if(dep.Funcionarios?.Any() ?? false)
            foreach(var func in dep.Funcionarios)
                System.Console.WriteLine($"\tFuncionario : {func.Nome}");
        else
            System.Console.WriteLine("\tDepartamento não possui funcionários");
    }
}


static async Task CarregamentoExplicito()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    //Include gera um left join
    //Um Enumerable ou IQueryable posterga a consulta 
    //e deixa a conexão aberta até o fim da iteração
    var departamentos = await db.Departamentos.ToListAsync();

    foreach(var dep in departamentos)
    {
        if(dep.Id == 2)
            await db.Entry(dep).Collection(x => x.Funcionarios).LoadAsync();
            
        //await db.Entry(dep).Collection(x => x.Funcionarios).Query().Where(x => x.Id > 2).ToListAsync();

        System.Console.WriteLine("-------------------------");
        System.Console.WriteLine($"Departamento:{dep.Descricao}");

        if(dep.Funcionarios?.Any() ?? false)
            foreach(var func in dep.Funcionarios)
                System.Console.WriteLine($"\tFuncionario : {func.Nome}");
        else
            System.Console.WriteLine("\tDepartamento não possui funcionários");
    }
}

static async Task SetUpTiposCarregamentos(ApplicationDbContext db)
{
    if(!await db.Departamentos.AnyAsync())
    {
        var departamentos = new List<Departamento>
        {
            new Departamento 
            {
                Descricao = "Departartamento 01",
                Funcionarios = new List<Funcionario>()
                {
                    new Funcionario {Nome = "Raphael", CPF = "9887665155829"}
                }
            },
            new Departamento 
            {
                Descricao = "Departartamento 02",
                Funcionarios = new List<Funcionario>()
                {
                    new Funcionario {Nome = "Junior", CPF = "98876651098787"}
                }
            }
        };

        db.Departamentos.AddRange(departamentos);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }
}

static async Task IgnorarFiltrosGlobais()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    var departamentos = await db.Departamentos.IgnoreQueryFilters()
                                .Where(x => x.Id > 1).ToListAsync();
    foreach(var dep in departamentos)
        System.Console.WriteLine($"Descrição:{dep.Descricao} \t Excluido:{dep.Excluido}");
}

//Uma consulta projetada é quando selecionamos os campos de retorno
static async Task ConsultaProjetada()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    var departamentos = await db.Departamentos
                                .Where(x => x.Id > 0)
                                .Select(x => new {x.Descricao, Funcionarios = x.Funcionarios.Select(f => f.Nome)})
                                .ToListAsync();

    foreach(var dep in departamentos)
    {
        System.Console.WriteLine($"Descrição:{dep.Descricao} \t");
        foreach(var func in dep.Funcionarios)
            System.Console.WriteLine($"\t Nome: {func}");
    }
}

static async Task ConsultaParametrizada()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    var id = new SqlParameter
    {
        DbType = System.Data.DbType.Int32,
        Value = 0,
    };

    var departamentos = await db.Departamentos
                                .FromSqlRaw("SELECT * FROM Departamentos WHERE Id > {0}", id)
                                .ToListAsync();

    foreach(var dep in departamentos)
        System.Console.WriteLine($"Descrição:{dep.Descricao} \t");
    
}

//Adiciona nos logs das consultas uma forma de rastrear consultas e manipulações de dados
static async Task ConsultaComTag()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    var departamentos = await db.Departamentos
                                .TagWith("Adicionando tag comentário na consulta")
                                .ToListAsync();

    foreach(var dep in departamentos)
        System.Console.WriteLine($"Descrição:{dep.Descricao} \t");
    
}

//Divisão de consultas evita dados duplicados quando existem relacionacimentos 1xN
static async Task DivisaoConsulta()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    var departamentos = await db.Departamentos
                                .Include(x => x.Funcionarios)
                                .Where(x => x.Id < 3)
                                .AsSplitQuery()
                                .ToListAsync();

    foreach(var dep in departamentos)
        System.Console.WriteLine($"Descrição:{dep.Descricao} \t");
}

static async Task CriarStoreProcedure()
{
    var criarDep = @"
    CREATE OR ALTER PROCEDURE CriarDepartamento
        @Descricao VARCHAR(50),
        @Ativo BIT
    AS
    BEGIN
        INSERT INTO 
            Departamentos(Descricao,Ativo,Excluido)
            VALUES(@Descricao,@Ativo,0)
    END";

    using var db = new ApplicationDbContext();
    await db.Database.ExecuteSqlRawAsync(criarDep);
}

static async Task CriarStoreProcedureConsulta()
{
    var criarDep = @"
    CREATE OR ALTER PROCEDURE BuscarDepartamentos
        @Descricao VARCHAR(50)
    AS
    BEGIN
        SELECT * FROM Departamentos WHERE Descricao LIKE @Descricao + '%'
    END";

    using var db = new ApplicationDbContext();
    await db.Database.ExecuteSqlRawAsync(criarDep);
}

static async Task ConsultaViaProcedure()
{
    using var db = new ApplicationDbContext();
    var departamentos = await db.Departamentos.FromSqlRaw("EXECUTE BuscarDepartamentos @p1", "Departamento").ToListAsync();

    foreach(var dep in departamentos)
        System.Console.WriteLine(dep.Descricao);
}

static async Task InserirViaProcedure()
{
    using var db = new ApplicationDbContext();
    await db.Database.ExecuteSqlRawAsync("EXECUTE CriarDepartamento @p1,@p2", "Departamento Proc", true);
}

static async Task ConsultaInterpolada()
{
    using var db = new ApplicationDbContext();
    await SetUpTiposCarregamentos(db);

    var id = 0;

    var departamentos = await db.Departamentos
                                .FromSqlInterpolated($"SELECT * FROM Departamentos WHERE Id > {id}")
                                .ToListAsync();

    foreach(var dep in departamentos)
        System.Console.WriteLine($"Descrição:{dep.Descricao} \t");
}

//OBS: Ao realizar consultas de tabelas que possuem dependentes utilizando um include
// é feito um left join pois podem não existir dependentes
// Ao realizar consultas a partir de dependentes para suas dependencias é feito
// um inner join caso a dependencia indique um obrigatoriedade


//Podemos mudar o tempo de timeout individual de uma consulta
static async Task ConfigurarCommandTimeoutIndividual()
{
    using var db = new ApplicationDbContext();

    db.Database.SetCommandTimeout(10);

    await db.Database.ExecuteSqlRawAsync("WAITFOR DELAY '00:00:07'; SELECT 1");
}

//Ao utilizar uma estratégia de resiliencia para evitar dados duplicados devemos criar um estrategia de execução
static async Task ExecutarEstrategiaResiliencia()
{
    using var db = new ApplicationDbContext();

    var strategy = db.Database.CreateExecutionStrategy();

    await strategy.ExecuteAsync(async () => 
            {
                using var transaction = await db.Database.BeginTransactionAsync();

                db.Departamentos.Add(new Departamento { Descricao = "Departamento transação" });
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            });
}

static async Task TrabalhandoComPropriedadesSombras()
{
    using var db = new ApplicationDbContext();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    var departamento = new Departamento
    {
        Descricao = "Departamento com Propriedade Sombra"
    };

    db.Departamentos.Add(departamento);
    
    //Alterando o valor de uma propriedade sombra
    db.Entry(departamento).Property("UltimaAtualizacao").CurrentValue = DateTime.Now;

    //Consulta utilizando propriedade sombra
    var departamentos = db.Departamentos
                            .Where(x => EF.Property<DateTime>(x, "UltimaAtualizacao") < DateTime.Now);

    await db.SaveChangesAsync();
}

static async Task PacotesDePropriedades()
{
    using var db = new ApplicationDbContext();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    var configuracao = new Dictionary<string, object>
    {
        ["Chave"] = "SenhaBancoDados",
        ["Valor"] = Guid.NewGuid().ToString()
    };

    db.Configuracoes.Add(configuracao);
    await db.SaveChangesAsync();

    //Consulta para tabela dicionario
    var configuracoes = await db.Configuracoes
                                .AsNoTracking()
                                .Where(x => x["Chave"].Equals("SenhaBancoDados"))
                                .ToListAsync();
}

static async Task ApagarECriarBancoDados()
{
    using var db = new ApplicationDbContext();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    db.Funcoes.AddRange(
        new Funcao
        {
            Data1 = DateTime.Now.AddDays(2),
            Data2 = "2022-02-02",
            Descricao1 = "Bala 1",
            Descricao2 = "Bala 2"
        },
        new Funcao
        {
            Data1 = DateTime.Now.AddDays(1),
            Data2 = "XX22-02-02",
            Descricao1 = "Bola 1",
            Descricao2 = "Bola 2"
        },
        new Funcao
        {
            Data1 = DateTime.Now.AddDays(1),
            Data2 = "XX22-02-02",
            Descricao1 = "Tela",
            Descricao2 = "Tela"
        }
    );
    await db.SaveChangesAsync();
}

static async Task FuncoesDatas()
{
    await ApagarECriarBancoDados();

    using var db = new ApplicationDbContext();
    var script = db.Database.GenerateCreateScript();

    System.Console.WriteLine(script);

    var dados = db.Funcoes.AsNoTracking().Select(x =>
        new 
        {
            //Funções que existem dentro dos bancos de dados trazidos pelo entity ficam em Ef.Functions
            Dias = EF.Functions.DateDiffDay(DateTime.Now, x.Data1),
            Data = EF.Functions.DateFromParts(2022,2,2),
            Datavalida = EF.Functions.IsDate(x.Data2)
        }
    );

    foreach(var f in dados)
        System.Console.WriteLine(f);

    var funcoes = await db.Funcoes
                        .AsNoTracking()
                        //Utilizando um operado like explicito em uma consulta
                        .Where(x => EF.Functions.Like(x.Descricao1, "B[ao]%"))
                        .Select(x => x.Descricao1)
                        .ToArrayAsync();
}

static async Task FuncoesDataLength()
{
    await ApagarECriarBancoDados();

    using var db = new ApplicationDbContext();
    var script = db.Database.GenerateCreateScript();

    System.Console.WriteLine(script);

    var resultado = await db.Funcoes.AsNoTracking().Select(x =>
        new 
        {
            //Funções que existem dentro dos bancos de dados trazidos pelo entity ficam em Ef.Functions
            TotalBytesCompoData = EF.Functions.DataLength(x.Data1),
            //utiliza a função DATALENGTH() para calcular o tamanho da alocação
            TotalBytesNVarchar = EF.Functions.DataLength(x.Descricao1),
            TotalBytesVarchar = EF.Functions.DataLength(x.Descricao2),
            //Utiliza o metodo LEN() para calcular o tamanho do texto
            Total1 = x.Descricao1.Length,
            Total2 = x.Descricao2.Length
        }
    ).FirstOrDefaultAsync();
    
    System.Console.WriteLine(resultado);
}

static async Task FuncaoProperty()
{
    await ApagarECriarBancoDados();

    using var db = new ApplicationDbContext();
    var script = db.Database.GenerateCreateScript();

    System.Console.WriteLine(script);

    //Recuperando dados do banco usando a propriedade sombra
    var resultado = await db.Funcoes
                    //Propriedades sombra ao serem buscadas sem tracking não ficam ligadas aos objetos que pertencem
                    //Por isso se deseja utilizar a propriedade devemos utilizar consultas rastreadas
                    //.AsNoTracking() 
                    .FirstOrDefaultAsync(x => EF.Property<string>(x,"PropriedadeSombra") == "Teste");

    //lendo o valor da propriedade
    var valorPropriedade = db.Entry(resultado)
                            .Property<string>("PropriedadeSombra")
                            .CurrentValue;

    var consultaCollate = await db.Funcoes
                    //Forçando uma consulta utilizar case sensitive e acentuação
                    .AsNoTracking() 
                    .FirstOrDefaultAsync(x => EF.Functions.Collate(x.Descricao1,"SQL_Latin1_General_CP1_CS_AS") == "tela");

    System.Console.WriteLine(resultado);
}

static async Task CadastrarDepartamento()
{
    using var db = new ApplicationDbContext();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    db.Departamentos.Add(new Departamento{Descricao = "Departamento 1"});

    await db.SaveChangesAsync();
}

//Apesar de utilizar o savechanges mais de uma vez se a transacao não receber um commit ao fechar a conexão o entity irá efetuar o roolback de todas abertas
static async Task gerenciarTransacaoManualmente()
{
    await CadastrarDepartamento();
    
    using var db = new ApplicationDbContext();
    using var transaction = await db.Database.BeginTransactionAsync();
    var departamento = await db.Departamentos.FirstOrDefaultAsync(x => x.Id == 1);
    departamento.Excluido = true;
    await db.SaveChangesAsync();

    db.Departamentos.Add(new Departamento { Descricao = "Departamento 2" });
    await db.SaveChangesAsync();
    await transaction.CommitAsync();
}

static async Task ReveterTransacaoManualmente()
{
    await CadastrarDepartamento();
    
    using var db = new ApplicationDbContext();
    using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
         var departamento = await db.Departamentos.FirstOrDefaultAsync(x => x.Id == 1);
        departamento.Excluido = true;
        await db.SaveChangesAsync();

        db.Departamentos.Add(new Departamento { Descricao = "Departamento 2".PadRight(300,'*') });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
    }
}

static async Task ReveterSavePointTransacaoManualmente()
{
    await CadastrarDepartamento();
    
    using var db = new ApplicationDbContext();
    using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
         var departamento = await db.Departamentos.FirstOrDefaultAsync(x => x.Id == 1);
        departamento.Excluido = true;
        await db.SaveChangesAsync();

        await transaction.CreateSavepointAsync("desfazer_insercoes");

        db.Departamentos.Add(new Departamento { Descricao = "Departamento 2"});
        await db.SaveChangesAsync();

        db.Departamentos.Add(new Departamento { Descricao = "Departamento 3".PadRight(300,'*') });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch(DbUpdateException e)
    {
        transaction.RollbackToSavepoint("desfazer_insercoes");

        if(e.Entries.Count(x => x.State == EntityState.Added) == e.Entries.Count)
            await transaction.CommitAsync();
    }
}

static async Task TransactionScope()
{
    await CadastrarDepartamento();

    var options = new TransactionOptions
    {
        IsolationLevel = IsolationLevel.ReadUncommitted,
        Timeout = TimeSpan.FromSeconds(30)
    };
    //Todas operações envolvendo banco de dados dentro do escopo serão executando usando transaction
    //Mesmo que as mesmas não possuam transações internas
    using var scope = new TransactionScope(TransactionScopeOption.Required, options);
    await CadastrarDepartamento();
    scope.Complete();
}

static async Task FuncaoLeft()
{
    await CadastrarDepartamento();

    using var db = new ApplicationDbContext();
    var departamentos = db.Departamentos.Select(x => DbFunctionsMapping.Left(x.Descricao, 10));
    foreach(var dep in departamentos)
        System.Console.WriteLine(dep);
}

static async Task FuncaoDefinidaPeloUsuario()
{
    await CadastrarDepartamento();

    using var db = new ApplicationDbContext();

    //Aplicando o metodo de rastreamento para a instancia
    db.ChangeTracker.QueryTrackingBehavior  = QueryTrackingBehavior.NoTrackingWithIdentityResolution;

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE FUNCTION ConverterParaLetrasMaiusculas(@dado VARCHAR(100))
        RETURNS VARCHAR(100)
        BEGIN
            RETURN UPPER(@dado)
        END
    ");

    var resultado = db.Departamentos.Select(x => DbFunctionsMapping.LetrasMaiusculas(x.Descricao));
    foreach(var item in resultado)
        System.Console.WriteLine(item);
}

static async Task ConsultaProjetadaRastreada()
{
    await CadastrarDepartamento();

    using var db = new ApplicationDbContext();
    //Apesar de um objeto anônimo quando incluimos entidades completas dentro do retorno ela passa a ser rastreada
    //Qualquer alteração nessa entidade rastreada será salva ao persistir mesmo que não seja a inteção mudar
    var departamentos = db.Departamentos.Include(x => x.Funcionarios)
    .Select(x =>
    new
    {
        Departamento = x,
        TotalFuncionarios = x.Funcionarios.Count()
    }).ToList();

    foreach (var dep in departamentos)
        System.Console.WriteLine(dep.Departamento.Descricao);
}

static async Task ConsultaProjetadaVericandoAlocacao()
{
    await CadastrarDepartamento();

    using var db = new ApplicationDbContext();
    //Apesar de um objeto anônimo quando incluimos entidades completas dentro do retorno ela passa a ser rastreada
    //Qualquer alteração nessa entidade rastreada será salva ao persistir mesmo que não seja a inteção mudar
    var departamentos = db.Departamentos.Include(x => x.Funcionarios)
    .Select(x =>
    new
    {
        Departamento = x.Descricao,
        TotalFuncionarios = x.Funcionarios.Count()
    }).ToList();
    
    //Retorna a quantidade de memoria alocada em um processo
    var memoria = (Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024) + "MB";

    foreach (var dep in departamentos)
        System.Console.WriteLine(dep.Departamento);
}

static async Task GerandoTextoScript()
{
    using var db = new ApplicationDbContext();
    await db.Database.EnsureCreatedAsync();

    var query = db.Departamentos.Where(x => x.Id > 2);

    var sql = query.ToQueryString();

    System.Console.WriteLine(sql);
}

static async Task LimpandoContexto()
{
    using var db = new ApplicationDbContext();
    await db.Database.EnsureCreatedAsync();

    db.Departamentos.Add(new Departamento { Descricao = "Teste" });

    //Limpa quaisquer informações dentro do contexto no momento atual
    //Seja delete, incluir ou mudanças
    db.ChangeTracker.Clear();
}



