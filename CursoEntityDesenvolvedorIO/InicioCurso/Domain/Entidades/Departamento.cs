namespace DominandoEFCoreDevIo.Domain.Entidades;

public class Departamento
{
    public int Id { get; set; }
    public string Descricao { get; set; }
    public bool Ativo { get; set; }
    public bool Excluido { get; set; }
    public DateTime Datacadastro { get; set; }

    public ICollection<Funcionario> Funcionarios { get; set;}
}
