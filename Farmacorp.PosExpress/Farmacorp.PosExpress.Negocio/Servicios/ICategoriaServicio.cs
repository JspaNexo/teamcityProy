using Farmacorp.PosExpress.Negocio.Modelos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public interface ICategoriaServicio
{
    /// <summary>Transacción 3: Registro de asignación de categoría a un producto.</summary>
    Task<CategoriaAsignada> AsignarAsync(long idProducto, string idCategoria, CancellationToken cancelacion = default);

    Task<IReadOnlyList<ResumenCategoria>> ListarAsync(CancellationToken cancelacion = default);
}
