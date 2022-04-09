using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCoreDevIo.Data.Mappings;

public class EstadoMapping : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder
            .HasOne(x => x.Governador)
            .WithOne(x => x.Estado)
            .HasForeignKey<Governador>(x => x.EstadoId);

        //Sempre ao buscar um estado trará seu governador sem usar include
        builder.Navigation(x => x.Governador).AutoInclude();

        //Por padrão um relacionamento 1xN tem o deletar como cascata
        //Quando colocamos o required como false podemos inserir entidades dependentes com relacionamentos nulos
        builder
            .HasMany(x => x.Cidades)
            .WithOne(x => x.Estado)
            .HasForeignKey(x => x.EstadoId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .IsRequired(false);
    }
}
