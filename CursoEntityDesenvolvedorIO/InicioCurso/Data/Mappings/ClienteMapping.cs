using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCoreDevIo.Data.Mappings;

public class ClienteMapping : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder
            .OwnsOne(x => x.Endereco, e =>
            {
                e.Property(y => y.Bairro)
                    .HasColumnName("Bairro");

                e.ToTable("Enderecos");
            });
    }
}
