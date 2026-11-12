using Farmacorp.PosExpress.Entidades;

namespace Farmacorp.PosExpress.Datos.Contratos;

public interface IExpProductoRepositorio : IRepositorio<ExpProducto>
{
    /// <summary>
    /// Obtiene el producto con su información ERP y sus categorías asignadas.
    /// </summary>
    Task<ExpProducto?> ObtenerConDetalleAsync(long idProducto, CancellationToken cancelacion = default);

    /// <summary>
    /// Lista los productos con tipo, información ERP, categorías y códigos de barras.
    /// </summary>
    Task<IReadOnlyList<ExpProducto>> ListarConDetalleAsync(CancellationToken cancelacion = default);
}
