using Farmacorp.PosExpress.Entidades;

namespace Farmacorp.PosExpress.Datos.Contratos;

public interface IVentaExpressRepositorio : IRepositorio<VentaExpress>
{
    Task<IReadOnlyList<VentaExpress>> ListarRecientesAsync(int cantidadMaxima, CancellationToken cancelacion = default);
}
