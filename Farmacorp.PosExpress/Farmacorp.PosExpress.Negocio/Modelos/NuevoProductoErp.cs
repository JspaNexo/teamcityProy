namespace Farmacorp.PosExpress.Negocio.Modelos;

public record NuevoProductoErp(
    string Nombre,
    decimal Costo,
    int Stock,
    DateTime FechaVencimiento,
    int IdTipoProducto,
    string? Observaciones);
