using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DominandoEFCoreDevIo.Domain.Entidades;

[Table("Atributos", Schema = "dbo")]
//Cria um indice composto
[Index(nameof(Descricao), nameof(Id), IsUnique = true)]
[Comment("Um comentário para uma tabela")]
public class Atributo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Descricao", TypeName = "VARCHAR(100)", Order = 2)]
    [Comment("Um comentário para um campo")]
    public string Descricao { get; set; }

    [Required]
    [MaxLength(255)]
    //O comportamento comum quando a aplicação insere e manipula os dados
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Observacao { get; set; }

    //Mesmo que qualquer ponto da aplicação faça uma alteração nesse campo somente o banco pode atualizar a mesma
    //Porém a leitura será feita de forma normal
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string CampoComputadoPeloBanco { get; set; }
}


public class Aeroporto
{
    public int Id { get; set; }
    public string Nome { get; set; }

    //[NotMapped]
    public string PropriedadeQueNaoDeveSerPersistida { get; set; }

    //Notação para resolver multiplos relacionamentos com a mesma classe
    [InverseProperty("AeroportoPartida")]
    public ICollection<Voo> VoosDePartida { get; set; }
    [InverseProperty("AeroportoChegada")]
    public ICollection<Voo> VoosDeChegada { get; set; }
}

//[NotMapped]
public class Voo 
{
    public int Id { get; set; }
    public string Descricao { get; set; }
    public int AeroportoChegadaId { get; set; }
    public int AeroportoPartidaId { get; set; }

    public Aeroporto AeroportoPartida { get; set; }
    public Aeroporto AeroportoChegada { get; set; }
}

//Notação para informar ao entity que a tabela não possui chaves
//Indicando também que seria uma tabela de apenas leitura evitando tracking e inserções
//Caso seja necessário inserir em tabelas sem chaves definidas podemos elencar a(s) chave(s)
[Keyless]
public class RelatorioFinanceiro
{
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
}


