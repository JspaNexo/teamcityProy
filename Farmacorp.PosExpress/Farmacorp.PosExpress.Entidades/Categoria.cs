namespace Farmacorp.PosExpress.Entidades;

public class Categoria
{
    public string IdCategoria { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public string? IdCategoriaPadre { get; set; }

    public Categoria? CategoriaPadre { get; set; }

    public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();

    public ICollection<ProductoCategoria> ProductosCategorias { get; set; } = new List<ProductoCategoria>();
}
