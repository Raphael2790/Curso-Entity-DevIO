using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DominandoEFCoreDevIo.Interceptors;

//Como boa prática sempre sobrescrever tanto o metodo sync quanto async
public class DbConnectInterceptor : DbConnectionInterceptor
{
    public static DbConnectInterceptor Instance => new DbConnectInterceptor();
    
    public override InterceptionResult ConnectionOpening(
        DbConnection connection, 
        ConnectionEventData eventData, 
        InterceptionResult result)
    {
        System.Console.WriteLine("[Sync] Entrei no metodo connection Opening");
        var connectionString = ((SqlConnection)connection).ConnectionString;
        System.Console.WriteLine(connectionString);
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            ApplicationName = "CursoEfCore",

        };
        connection.ConnectionString = connectionStringBuilder.ToString();
        System.Console.WriteLine(connectionStringBuilder.ToString());
        return result;
    }

    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection, 
        ConnectionEventData eventData, 
        InterceptionResult result, CancellationToken cancellationToken = default)
    {
        System.Console.WriteLine("[Async] Entrei no metodo connection Opening");
        var connectionString = ((SqlConnection)connection).ConnectionString;
        System.Console.WriteLine(connectionString);
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            ApplicationName = "CursoEfCore",

        };
        connection.ConnectionString = connectionStringBuilder.ToString();
        System.Console.WriteLine(connectionStringBuilder.ToString());
        return new ValueTask<InterceptionResult>(result);
    }
}
