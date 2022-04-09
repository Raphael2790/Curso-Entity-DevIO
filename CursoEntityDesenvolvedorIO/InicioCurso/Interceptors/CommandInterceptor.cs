using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DominandoEFCoreDevIo.Interceptors;

//caso queiramos sobrescrever todos metodos da classe abstrata podemos implementar a interface IDbCommandInterceptor
public class CommandInterceptor : DbCommandInterceptor
{
    public static CommandInterceptor Instance => new CommandInterceptor();
    private static readonly Regex _tableRegex =
        new Regex(@"(?<tableAlias>FROM +(\[.*\]\.)?(\[.*\]) AS (\[.*\])(?! WITH \(NOLOCK\)))",
        RegexOptions.Multiline
        | RegexOptions.Compiled
        | RegexOptions.IgnoreCase);

    //intercepta a execução de consultas sincronas 
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result)
    {
        System.Console.WriteLine("[Sync] Executando reader");

        UsarNoLock(command);

        return result;
    }

    //intercepta a execução de consultas assincronas 
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        System.Console.WriteLine("[Async] Executando reader");
        UsarNoLock(command);
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    private static void UsarNoLock(DbCommand command)
    {
        //verifica se existe no lock no texto do comando e se existe a marcação com o comentário via tag
       if(!command.CommandText.Contains("WITH (NOLOCK)") &&
        command.CommandText.StartsWith("-- Use NOLOCK"))
            command.CommandText = _tableRegex.Replace(command.CommandText, "${tableAlias} WITH (NOLOCK)");
    }
}
