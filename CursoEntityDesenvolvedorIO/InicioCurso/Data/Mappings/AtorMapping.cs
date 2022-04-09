using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCoreDevIo.Data.Mappings;

public class AtorMapping : IEntityTypeConfiguration<Ator>
{
    public void Configure(EntityTypeBuilder<Ator> builder)
    {
        builder.HasMany(x => x.Filmes)
                .WithMany(x => x.Atores)
                .UsingEntity<AtoresFilmes>("AtoresFilmes",
                x => x.HasOne(x => x.Filme)
                        .WithMany(x => x.AtoresFilmes)
                        .HasForeignKey(x => x.FilmeId),
                x => x.HasOne(x => x.Ator)
                        .WithMany(x => x.AtoresFilmes)
                        .HasForeignKey(x => x.AtorId)
                ,x =>
                {
                    //Mapping dos campos da tabela de relacionamento
                    x.Property(x => x.AtorId).HasColumnName("AtorId");
                    x.Property(x => x.FilmeId).HasColumnName("FilmeId");
                });
    }
}
