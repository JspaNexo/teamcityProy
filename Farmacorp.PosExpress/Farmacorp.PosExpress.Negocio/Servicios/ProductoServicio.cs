using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Entidades;
using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Modelos;
using Farmacorp.PosExpress.Negocio.Recursos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public class ProductoServicio : IProductoServicio
{
    private readonly IUnitOfWork _unidadTrabajo;
    private readonly ISelectorReglasNegocio _selectorReglas;
    private readonly IGeneradorCodigo _generadorCodigo;
    private readonly TimeProvider _proveedorTiempo;

    public ProductoServicio(
        IUnitOfWork unidadTrabajo,
        ISelectorReglasNegocio selectorReglas,
        IGeneradorCodigo generadorCodigo,
        TimeProvider proveedorTiempo)
    {
        _unidadTrabajo = unidadTrabajo;
        _selectorReglas = selectorReglas;
        _generadorCodigo = generadorCodigo;
        _proveedorTiempo = proveedorTiempo;
    }

    public async Task<ProductoRegistrado> RegistrarAsync(NuevoProductoErp nuevoProducto, CancellationToken cancelacion = default)
    {
        var fechaActual = _proveedorTiempo.GetLocalNow().DateTime;

        ValidarNuevoProducto(nuevoProducto, fechaActual);

        var existeTipo = await _unidadTrabajo.TiposProducto.ExisteAsync(
            tipo => tipo.IdTipoProducto == nuevoProducto.IdTipoProducto, cancelacion);

        if (!existeTipo)
        {
            throw new ReglaNegocioException(
                CodigosError.RECURSO_NO_ENCONTRADO, Mensajes.TipoProductoNoExiste, nuevoProducto.IdTipoProducto);
        }

        var reglas = _selectorReglas.ObtenerReglasVigentes();

        var producto = new ExpProducto
        {
            Nombre = nuevoProducto.Nombre.Trim(),
            Precio = reglas.CalcularPrecio(nuevoProducto.Costo), // Regla de Negocio 1
            Activo = true,
            FechaVencimiento = nuevoProducto.FechaVencimiento,
            Observaciones = ValidadorProducto.NormalizarOpcional(nuevoProducto.Observaciones),
            IdTipoProducto = nuevoProducto.IdTipoProducto,
            ErpProducto = new ErpProducto
            {
                Costo = nuevoProducto.Costo,
                UniqueCodigo = _generadorCodigo.GenerarUniqueCodigo(),
                FechaRegistro = fechaActual,
                Stock = nuevoProducto.Stock
            }
        };

        // ExpProducto y ErpProducto se insertan en la misma transacción
        await _unidadTrabajo.ExpProductos.AgregarAsync(producto, cancelacion);
        await _unidadTrabajo.GuardarCambiosAsync(cancelacion);

        return new ProductoRegistrado(
            producto.IdProducto,
            producto.Nombre,
            producto.ErpProducto.Costo,
            producto.Precio,
            producto.ErpProducto.UniqueCodigo,
            producto.ErpProducto.Stock,
            reglas.Estrategia);
    }

    public async Task<IReadOnlyList<ResumenProducto>> ListarAsync(CancellationToken cancelacion = default)
    {
        var productos = await _unidadTrabajo.ExpProductos.ListarConDetalleAsync(cancelacion);

        return productos
            .Select(producto => new ResumenProducto(
                producto.IdProducto,
                producto.Nombre,
                producto.TipoProducto?.Descripcion ?? string.Empty,
                producto.Precio,
                producto.ErpProducto?.Costo,
                producto.ErpProducto?.Stock,
                producto.ErpProducto?.UniqueCodigo,
                producto.Activo,
                producto.ProductosCategorias.Select(detalle => detalle.IdCategoria).Order().ToList(),
                producto.CodigosBarras.Count))
            .ToList();
    }

    public async Task<IReadOnlyList<ResumenTipoProducto>> ListarTiposAsync(CancellationToken cancelacion = default)
    {
        var tipos = await _unidadTrabajo.TiposProducto.ListarAsync(cancelacion);

        return tipos
            .OrderBy(tipo => tipo.IdTipoProducto)
            .Select(tipo => new ResumenTipoProducto(tipo.IdTipoProducto, tipo.Descripcion))
            .ToList();
    }

    private static void ValidarNuevoProducto(NuevoProductoErp nuevoProducto, DateTime fechaActual)
    {
        if (string.IsNullOrWhiteSpace(nuevoProducto.Nombre))
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.NombreObligatorio);
        }

        ValidadorProducto.ValidarLongitud(nuevoProducto.Nombre, LongitudesMaximas.NOMBRE_PRODUCTO, Mensajes.CampoNombre);
        ValidadorProducto.ValidarLongitud(nuevoProducto.Observaciones, LongitudesMaximas.OBSERVACIONES, Mensajes.CampoObservaciones);

        if (nuevoProducto.Costo <= decimal.Zero)
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.CostoInvalido);
        }

        if (nuevoProducto.Stock < 0)
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.StockInicialInvalido);
        }

        if (nuevoProducto.FechaVencimiento.Date <= fechaActual.Date)
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.FechaVencimientoInvalida);
        }
    }
}
