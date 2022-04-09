// See https://aka.ms/new-console-template for more information
using DominandoEFCoreFinal.Data;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hello, World!");

using var db = new ApplicationContext();
//aplicar migrações in runtime
db.Database.Migrate();

//buscar migrações que estejam pendentes
db.Database.GetPendingMigrations();
