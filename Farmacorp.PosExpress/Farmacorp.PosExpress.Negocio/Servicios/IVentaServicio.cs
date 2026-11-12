using Farmacorp.PosExpress.Negocio.Modelos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public interface IVentaServicio
{
    /// <summary>Transacción 4: Registro de venta (un único producto).</summary>
    Task<VentaRegistrada> RegistrarAsync(NuevaVenta nuevaVenta, CancellationToken cancelacion = default);

    Task<IReadOnlyList<ResumenVenta>> ListarRecientesAsync(CancellationToken cancelacion = default);
}
