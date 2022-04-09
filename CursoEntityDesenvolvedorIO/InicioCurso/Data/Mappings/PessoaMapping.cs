using DominandoEFCoreDevIo.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DominandoEFCoreDevIo.Data.Mappings;

public class PessoaMapping : IEntityTypeConfiguration<Pessoa>
{
    public void Configure(EntityTypeBuilder<Pessoa> builder)
    {
        //Modelo TPH
        //Ao criar um herança entre entidades o entity irá criar um campo chamdo descriminator
        //Podemos sobrescrever o tipo dele padrão de texto e colocar valores padrões para cada objeto da herança
        builder.ToTable("Pessoas")
            .HasDiscriminator<int>("TipoPessoa")
            .HasValue<Pessoa>(99)
            .HasValue<Aluno>(6)
            .HasValue<Instrutor>(3);
    }

    public void Configure2(EntityTypeBuilder<Pessoa> builder)
    {
        //Modelo TPT
        //Ao criar um herança entre entidades o entity irá criar um campo chamdo descriminator
        //Podemos sobrescrever o tipo dele padrão de texto e colocar valores padrões para cada objeto da herança
        builder.ToTable("Pessoas");
    }
}

public class AlunoMapping : IEntityTypeConfiguration<Aluno>
{
    //Modelo TPT
    //Serão criados tabelas separadas para cada entidade
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("Alunos");
    }
}

public class InstrutorMapping : IEntityTypeConfiguration<Instrutor>
{
    //Modelo TPT
    public void Configure(EntityTypeBuilder<Instrutor> builder)
    {
        builder.ToTable("Instrutores");
    }
}
