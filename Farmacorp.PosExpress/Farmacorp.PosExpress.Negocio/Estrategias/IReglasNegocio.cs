namespace Farmacorp.PosExpress.Negocio.Estrategias;

/// <summary>
/// Reglas de negocio que varían según la estrategia comercial vigente (patrón Strategy).
/// </summary>
public interface IReglasNegocio
{
    TiposEstrategia Estrategia { get; }

    /// <summary>Regla de Negocio 1: precio del producto como margen sobre el costo.</summary>
    decimal CalcularPrecio(decimal costo);

    /// <summary>Regla de Negocio 3: descuento si y solo si el producto tiene una única categoría.</summary>
    decimal CalcularDescuento(decimal subtotal, int cantidadCategorias);

    /// <summary>Regla de Negocio 4: valida que el stock permita realizar la venta.</summary>
    void ValidarStock(string nombreProducto, int stockDisponible, int cantidadSolicitada);
}
