namespace Farmacorp.PosExpress.Entidades;

/// <summary>
/// Información de catálogo e inventario del producto (módulo ERP).
/// Relación 1 a 1 con <see cref="ExpProducto"/>: IdProducto es PK y FK.
/// </summary>
public class ErpProducto
{
    public long IdProducto { get; set; }

    public decimal Costo { get; set; }

    public Guid UniqueCodigo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int Stock { get; set; }

    public ExpProducto? ExpProducto { get; set; }
}
