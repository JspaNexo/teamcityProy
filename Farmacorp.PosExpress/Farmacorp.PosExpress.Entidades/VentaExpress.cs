namespace Farmacorp.PosExpress.Entidades;

/// <summary>
/// Venta por artículo (un único producto por venta).
/// </summary>
public class VentaExpress
{
    public long Id { get; set; }

    public DateTime Fecha { get; set; }

    public string? Cliente { get; set; }

    /// <summary>Nombre del producto al momento de la venta.</summary>
    public string Producto { get; set; } = string.Empty;

    /// <summary>UniqueCodigo del producto en el módulo ERP (Regla de Negocio 2).</summary>
    public Guid? UniqueProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }

    public decimal? Descuento { get; set; }

    public decimal Total { get; set; }

    public long IdProducto { get; set; }

    public ExpProducto? ExpProducto { get; set; }
}
