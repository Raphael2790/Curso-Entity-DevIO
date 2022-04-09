namespace DominandoEFCoreDevIo.Domain.Entidades;

public class Ator
{
    public int Id { get; set; }
    public string Nome { get; set; }

    public ICollection<Filme> Filmes { get; set; } = new List<Filme>();
    public ICollection<AtoresFilmes> AtoresFilmes { get; set; } = new List<AtoresFilmes>();
}

public class Filme
{
    public int Id { get; set; }
    public string Nome { get; set; }
    
    public ICollection<Ator> Atores { get; set; } = new List<Ator>();
    public ICollection<AtoresFilmes> AtoresFilmes { get; set; } = new List<AtoresFilmes>();
}

public class AtoresFilmes
{
    public int AtorId { get; set; }
    public int FilmeId { get; set; }

    public Ator Ator { get; set; }
    public Filme Filme { get; set; }
}