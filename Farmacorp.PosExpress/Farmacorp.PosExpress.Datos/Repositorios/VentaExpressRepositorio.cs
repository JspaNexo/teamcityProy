using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Datos.Repositorios;

public class VentaExpressRepositorio : Repositorio<VentaExpress>, IVentaExpressRepositorio
{
    public VentaExpressRepositorio(PosExpressContexto contexto)
        : base(contexto)
    {
    }

    public async Task<IReadOnlyList<VentaExpress>> ListarRecientesAsync(int cantidadMaxima, CancellationToken cancelacion = default)
    {
        return await Entidades
            .AsNoTracking()
            .OrderByDescending(venta => venta.Id)
            .Take(cantidadMaxima)
            .ToListAsync(cancelacion);
    }
}
