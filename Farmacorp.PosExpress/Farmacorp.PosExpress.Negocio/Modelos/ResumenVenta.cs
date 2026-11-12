namespace Farmacorp.PosExpress.Negocio.Modelos;

public record ResumenVenta(
    long IdVenta,
    DateTime Fecha,
    string? Cliente,
    string Producto,
    Guid? UniqueProducto,
    int Cantidad,
    decimal Precio,
    decimal Descuento,
    decimal Total);
