using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Entidades;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Modelos;
using Farmacorp.PosExpress.Negocio.Recursos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public class CategoriaServicio : ICategoriaServicio
{
    private readonly IUnitOfWork _unidadTrabajo;
    private readonly TimeProvider _proveedorTiempo;

    public CategoriaServicio(IUnitOfWork unidadTrabajo, TimeProvider proveedorTiempo)
    {
        _unidadTrabajo = unidadTrabajo;
        _proveedorTiempo = proveedorTiempo;
    }

    public async Task<CategoriaAsignada> AsignarAsync(long idProducto, string idCategoria, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(idCategoria))
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.CategoriaObligatoria);
        }

        var idCategoriaNormalizado = idCategoria.Trim().ToUpperInvariant();

        var producto = await _unidadTrabajo.ExpProductos.ObtenerPorIdAsync(idProducto, cancelacion);
        ValidadorProducto.ValidarExistenteYActivo(producto, idProducto);

        var categoria = await _unidadTrabajo.Categorias.ObtenerPorIdAsync(idCategoriaNormalizado, cancelacion);

        if (categoria is null)
        {
            throw new ReglaNegocioException(CodigosError.RECURSO_NO_ENCONTRADO, Mensajes.CategoriaNoExiste, idCategoriaNormalizado);
        }

        if (!categoria.Activo)
        {
            throw new ReglaNegocioException(CodigosError.OPERACION_NO_PERMITIDA, Mensajes.CategoriaInactiva, idCategoriaNormalizado);
        }

        var yaAsignada = await _unidadTrabajo.ProductosCategorias.ExisteAsync(
            detalle => detalle.IdProducto == idProducto && detalle.IdCategoria == idCategoriaNormalizado, cancelacion);

        if (yaAsignada)
        {
            throw new ReglaNegocioException(
                CodigosError.REGISTRO_DUPLICADO, Mensajes.CategoriaYaAsignada, idProducto, idCategoriaNormalizado);
        }

        var detalle = new ProductoCategoria
        {
            FechaCreacion = _proveedorTiempo.GetLocalNow().DateTime,
            IdProducto = idProducto,
            IdCategoria = idCategoriaNormalizado
        };

        await _unidadTrabajo.ProductosCategorias.AgregarAsync(detalle, cancelacion);
        await _unidadTrabajo.GuardarCambiosAsync(cancelacion);

        var totalCategorias = await _unidadTrabajo.ProductosCategorias.ContarAsync(
            asignacion => asignacion.IdProducto == idProducto, cancelacion);

        return new CategoriaAsignada(detalle.IdDetalle, idProducto, idCategoriaNormalizado, totalCategorias);
    }

    public async Task<IReadOnlyList<ResumenCategoria>> ListarAsync(CancellationToken cancelacion = default)
    {
        var categorias = await _unidadTrabajo.Categorias.ListarAsync(cancelacion);

        return categorias
            .OrderBy(categoria => categoria.IdCategoria)
            .Select(categoria => new ResumenCategoria(
                categoria.IdCategoria, categoria.Descripcion, categoria.Activo, categoria.IdCategoriaPadre))
            .ToList();
    }
}
