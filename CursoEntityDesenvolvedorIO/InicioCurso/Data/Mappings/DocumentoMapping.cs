using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCoreDevIo.Data.Mappings;

public class DocumentoMapping : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        //Informando ao entity que ao salvar e trazer informações do banco deve ser alimentada a propriedade privada
        builder.Property("_cpf").HasColumnName("CPF").HasMaxLength(11);
    }
}
