using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DominandoEFCoreDevIo.Interceptors;

public class PersistenceInterceptor : SaveChangesInterceptor
{
    public static PersistenceInterceptor Instance = new PersistenceInterceptor();
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        System.Console.WriteLine(eventData.Context.ChangeTracker.DebugView.LongView);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        System.Console.WriteLine(eventData.Context.ChangeTracker.DebugView.LongView);
        return new ValueTask<InterceptionResult<int>>(result);
    }
}
