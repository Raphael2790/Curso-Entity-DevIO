using Microsoft.EntityFrameworkCore;

namespace DominandoEFCoreDevIo.Data.Functions;

public static class DbFunctionsMapping
{
    public static void RegistrarFuncoes(ModelBuilder modelBuilder)
    {   
        //procura dentro da classe todos metodos que possuam atributo DbFunction
        var funcoes = typeof(DbFunctionsMapping).GetMethods().Where(x => Attribute.IsDefined(x, typeof(DbFunctionAttribute)));

        foreach(var func in funcoes)
            modelBuilder.HasDbFunction(func);
    }

    //Mapeamento de uma função integrada do provider para uso interno da aplicação
    [DbFunction(name:"LEFT", IsBuiltIn = true, Schema = "")]
    public static string Left(string dados, int quantidade)
    {
        throw new NotImplementedException("Built-In Function Database");
    }

    public static string LetrasMaiusculas(string dados)
    {
        throw new NotImplementedException();
    }

    public static int DateDiff(string identificador, DateTime inicial, DateTime final)
    {
        throw new NotImplementedException();
    }
}
