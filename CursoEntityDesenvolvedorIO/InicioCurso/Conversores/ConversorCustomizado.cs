using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DominandoEFCoreDevIo.Conversores;

public class ConversorCustomizado : ValueConverter<Status,string>
{
    public ConversorCustomizado() 
            : base(x => ConverterStatusParaCaractere(x),
                    x => ConverterCaractereParaStatus(x),
                    new ConverterMappingHints(1))
    {
        
    }

    static string ConverterStatusParaCaractere(Status status)
    {
        return status.ToString()[0..1];
    }

    static Status ConverterCaractereParaStatus(string caractere)
    {
        return Enum.GetValues<Status>()
                    .FirstOrDefault(x => x.ToString()[0..1] == caractere);
    }
}
