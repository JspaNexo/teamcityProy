using Farmacorp.PosExpress.Negocio.Estrategias;

namespace Farmacorp.PosExpress.Negocio.Modelos;

public record ProductoRegistrado(
    long IdProducto,
    string Nombre,
    decimal Costo,
    decimal Precio,
    Guid UniqueCodigo,
    int Stock,
    TiposEstrategia EstrategiaAplicada);
