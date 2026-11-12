using Farmacorp.PosExpress.Negocio.Modelos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public interface ICodigoBarraServicio
{
    /// <summary>Transacción 2: Registro de asignación de código de barras.</summary>
    Task<CodigoBarraAsignado> AsignarAsync(long idProducto, CancellationToken cancelacion = default);
}
