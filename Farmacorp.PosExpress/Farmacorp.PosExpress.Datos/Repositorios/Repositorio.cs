using System.Linq.Expressions;
using Farmacorp.PosExpress.Datos.Contratos;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Datos.Repositorios;

public class Repositorio<TEntidad> : IRepositorio<TEntidad> where TEntidad : class
{
    public Repositorio(PosExpressContexto contexto)
    {
        Contexto = contexto;
        Entidades = contexto.Set<TEntidad>();
    }

    protected PosExpressContexto Contexto { get; }

    protected DbSet<TEntidad> Entidades { get; }

    public async Task<TEntidad?> ObtenerPorIdAsync(object id, CancellationToken cancelacion = default)
    {
        return await Entidades.FindAsync([id], cancelacion);
    }

    public async Task<IReadOnlyList<TEntidad>> ListarAsync(CancellationToken cancelacion = default)
    {
        return await Entidades.AsNoTracking().ToListAsync(cancelacion);
    }

    public Task<bool> ExisteAsync(Expression<Func<TEntidad, bool>> condicion, CancellationToken cancelacion = default)
    {
        return Entidades.AnyAsync(condicion, cancelacion);
    }

    public Task<int> ContarAsync(Expression<Func<TEntidad, bool>> condicion, CancellationToken cancelacion = default)
    {
        return Entidades.CountAsync(condicion, cancelacion);
    }

    public async Task AgregarAsync(TEntidad entidad, CancellationToken cancelacion = default)
    {
        await Entidades.AddAsync(entidad, cancelacion);
    }

    public void Actualizar(TEntidad entidad)
    {
        Entidades.Update(entidad);
    }

    public void Eliminar(TEntidad entidad)
    {
        Entidades.Remove(entidad);
    }
}
