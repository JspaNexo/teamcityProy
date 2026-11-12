using System.Linq.Expressions;

namespace Farmacorp.PosExpress.Datos.Contratos;

/// <summary>
/// Repositorio genérico con las operaciones comunes a todas las entidades.
/// </summary>
public interface IRepositorio<TEntidad> where TEntidad : class
{
    Task<TEntidad?> ObtenerPorIdAsync(object id, CancellationToken cancelacion = default);

    Task<IReadOnlyList<TEntidad>> ListarAsync(CancellationToken cancelacion = default);

    Task<bool> ExisteAsync(Expression<Func<TEntidad, bool>> condicion, CancellationToken cancelacion = default);

    Task<int> ContarAsync(Expression<Func<TEntidad, bool>> condicion, CancellationToken cancelacion = default);

    Task AgregarAsync(TEntidad entidad, CancellationToken cancelacion = default);

    void Actualizar(TEntidad entidad);

    void Eliminar(TEntidad entidad);
}
