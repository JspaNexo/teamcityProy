using Farmacorp.PosExpress.Negocio.Modelos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public interface IProductoServicio
{
    /// <summary>Transacción 1: Registro de nuevo producto ERP.</summary>
    Task<ProductoRegistrado> RegistrarAsync(NuevoProductoErp nuevoProducto, CancellationToken cancelacion = default);

    Task<IReadOnlyList<ResumenProducto>> ListarAsync(CancellationToken cancelacion = default);

    Task<IReadOnlyList<ResumenTipoProducto>> ListarTiposAsync(CancellationToken cancelacion = default);
}
