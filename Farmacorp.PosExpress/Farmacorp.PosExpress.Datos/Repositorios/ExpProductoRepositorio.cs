using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Datos.Repositorios;

public class ExpProductoRepositorio : Repositorio<ExpProducto>, IExpProductoRepositorio
{
    public ExpProductoRepositorio(PosExpressContexto contexto)
        : base(contexto)
    {
    }

    public Task<ExpProducto?> ObtenerConDetalleAsync(long idProducto, CancellationToken cancelacion = default)
    {
        return Entidades
            .Include(producto => producto.ErpProducto)
            .Include(producto => producto.ProductosCategorias)
            .FirstOrDefaultAsync(producto => producto.IdProducto == idProducto, cancelacion);
    }

    public async Task<IReadOnlyList<ExpProducto>> ListarConDetalleAsync(CancellationToken cancelacion = default)
    {
        return await Entidades
            .AsNoTracking()
            .Include(producto => producto.TipoProducto)
            .Include(producto => producto.ErpProducto)
            .Include(producto => producto.ProductosCategorias)
            .Include(producto => producto.CodigosBarras)
            .AsSplitQuery()
            .OrderBy(producto => producto.IdProducto)
            .ToListAsync(cancelacion);
    }
}
