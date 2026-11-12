namespace Farmacorp.PosExpress.Entidades;

public class ProductoCategoria
{
    public long IdDetalle { get; set; }

    public DateTime FechaCreacion { get; set; }

    public long IdProducto { get; set; }

    public string IdCategoria { get; set; } = string.Empty;

    public ExpProducto? ExpProducto { get; set; }

    public Categoria? Categoria { get; set; }
}
