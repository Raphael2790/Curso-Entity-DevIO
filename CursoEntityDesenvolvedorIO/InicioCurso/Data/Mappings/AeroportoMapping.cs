using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCoreDevIo.Data.Mappings;

public class AeroportoMapping : IEntityTypeConfiguration<Aeroporto>
{
    public void Configure(EntityTypeBuilder<Aeroporto> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
                .UseIdentityColumn<int>(1,1);

        builder.HasMany(x => x.VoosDeChegada)
                .WithOne(x => x.AeroportoChegada)
                .HasForeignKey(x => x.AeroportoChegadaId);

        builder.HasMany(x => x.VoosDePartida)
                .WithOne(x => x.AeroportoPartida)
                .HasForeignKey(x => x.AeroportoPartidaId);

        builder.Ignore(x => x.PropriedadeQueNaoDeveSerPersistida);

        //Cria um index composto
        builder.HasIndex(x => new { x.Id, x.Nome }).IsUnique();

        builder.Property(x => x.Nome).HasComment("Comentário para o campo");

        builder.HasComment("Comentário para uma tabela");
    }
}
