using Farmacorp.PosExpress.Negocio.Estrategias;

namespace Farmacorp.PosExpress.Negocio.Modelos;

public record VentaRegistrada(
    long IdVenta,
    string Producto,
    Guid? UniqueProducto,
    int Cantidad,
    decimal Precio,
    decimal Subtotal,
    decimal Descuento,
    decimal Total,
    int StockRestante,
    TiposEstrategia EstrategiaAplicada);
