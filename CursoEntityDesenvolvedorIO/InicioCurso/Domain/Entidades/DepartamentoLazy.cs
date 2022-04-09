using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DominandoEFCoreDevIo.Domain.Entidades;

public class DepartamentoLazy
{
    public int Id { get; set; }
    public string Descricao { get; set; }
    public bool Ativo { get; set; }

    public DepartamentoLazy()
    {

    }

    // private DepartamentoLazy(ILazyLoader lazyLoader)
    // {
    //     _lazyLoader = lazyLoader;
    // }

    // private ILazyLoader _lazyLoader { get; set; }
    // private ICollection<Funcionario> _funcionarios;
    // public ICollection<Funcionario> Funcionarios 
    // { 
    //     get => _lazyLoader.Load(this, ref _funcionarios); 
    //     set => _funcionarios = value;
    // }

    private DepartamentoLazy(Action<object,string> lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }

    private Action<object,string> _lazyLoader { get; set; }
    private ICollection<Funcionario> _funcionarios;
    public ICollection<Funcionario> Funcionarios 
    { 
        get
        {
            _lazyLoader?.Invoke(this, nameof(Funcionarios));
            return _funcionarios;
        }
        set => _funcionarios = value;
    }
}
