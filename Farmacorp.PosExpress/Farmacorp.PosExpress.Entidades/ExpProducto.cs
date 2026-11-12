namespace Farmacorp.PosExpress.Entidades;

/// <summary>
/// Producto del módulo Express.
/// </summary>
public class ExpProducto
{
    public long IdProducto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public string? Observaciones { get; set; }

    public int IdTipoProducto { get; set; }

    public TipoProducto? TipoProducto { get; set; }

    public ErpProducto? ErpProducto { get; set; }

    public ICollection<CodigoBarra> CodigosBarras { get; set; } = new List<CodigoBarra>();

    public ICollection<ProductoCategoria> ProductosCategorias { get; set; } = new List<ProductoCategoria>();

    public ICollection<VentaExpress> Ventas { get; set; } = new List<VentaExpress>();
}
