namespace Farmacorp.PosExpress.Negocio.Modelos;

public record ResumenProducto(
    long IdProducto,
    string Nombre,
    string TipoProducto,
    decimal Precio,
    decimal? Costo,
    int? Stock,
    Guid? UniqueCodigo,
    bool Activo,
    IReadOnlyList<string> Categorias,
    int CantidadCodigosBarras);
