using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Entidades;
using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Modelos;
using Farmacorp.PosExpress.Negocio.Recursos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public class VentaServicio : IVentaServicio
{
    private const int CANTIDAD_VENTAS_LISTADO = 20;

    private readonly IUnitOfWork _unidadTrabajo;
    private readonly ISelectorReglasNegocio _selectorReglas;
    private readonly TimeProvider _proveedorTiempo;

    public VentaServicio(IUnitOfWork unidadTrabajo, ISelectorReglasNegocio selectorReglas, TimeProvider proveedorTiempo)
    {
        _unidadTrabajo = unidadTrabajo;
        _selectorReglas = selectorReglas;
        _proveedorTiempo = proveedorTiempo;
    }

    public async Task<VentaRegistrada> RegistrarAsync(NuevaVenta nuevaVenta, CancellationToken cancelacion = default)
    {
        if (nuevaVenta.Cantidad <= 0)
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.CantidadInvalida);
        }

        ValidadorProducto.ValidarLongitud(nuevaVenta.Cliente, LongitudesMaximas.CLIENTE, Mensajes.CampoCliente);

        var producto = ValidadorProducto.ValidarExistenteYActivo(
            await _unidadTrabajo.ExpProductos.ObtenerConDetalleAsync(nuevaVenta.IdProducto, cancelacion),
            nuevaVenta.IdProducto);

        var erpProducto = producto.ErpProducto
            ?? throw new ReglaNegocioException(CodigosError.OPERACION_NO_PERMITIDA, Mensajes.ProductoSinErp, producto.IdProducto);

        var reglas = _selectorReglas.ObtenerReglasVigentes();

        // Regla de Negocio 4: solo se vende si el stock lo permite
        reglas.ValidarStock(producto.Nombre, erpProducto.Stock, nuevaVenta.Cantidad);

        var subtotal = producto.Precio * nuevaVenta.Cantidad;

        // Regla de Negocio 3: descuento si y solo si el producto tiene una única categoría
        var descuento = reglas.CalcularDescuento(subtotal, producto.ProductosCategorias.Count);

        var venta = new VentaExpress
        {
            Fecha = _proveedorTiempo.GetLocalNow().DateTime,
            Cliente = ValidadorProducto.NormalizarOpcional(nuevaVenta.Cliente),
            Producto = producto.Nombre,
            UniqueProducto = erpProducto.UniqueCodigo, // Regla de Negocio 2
            Cantidad = nuevaVenta.Cantidad,
            Precio = producto.Precio,
            Descuento = descuento,
            Total = subtotal - descuento,
            IdProducto = producto.IdProducto
        };

        // Regla de Negocio 5: actualizar stock en el módulo ERP (misma transacción que la venta)
        erpProducto.Stock -= nuevaVenta.Cantidad;

        await _unidadTrabajo.VentasExpress.AgregarAsync(venta, cancelacion);

        try
        {
            await _unidadTrabajo.GuardarCambiosAsync(cancelacion);
        }
        catch (ConflictoConcurrenciaException)
        {
            // Otra venta cambió el stock después de validarlo (RN4): se rechaza para no vender stock inexistente
            throw new ReglaNegocioException(
                CodigosError.CONFLICTO_CONCURRENCIA, Mensajes.StockModificadoConcurrencia, producto.Nombre);
        }

        return new VentaRegistrada(
            venta.Id,
            venta.Producto,
            venta.UniqueProducto,
            venta.Cantidad,
            venta.Precio,
            subtotal,
            descuento,
            venta.Total,
            erpProducto.Stock,
            reglas.Estrategia);
    }

    public async Task<IReadOnlyList<ResumenVenta>> ListarRecientesAsync(CancellationToken cancelacion = default)
    {
        var ventas = await _unidadTrabajo.VentasExpress.ListarRecientesAsync(CANTIDAD_VENTAS_LISTADO, cancelacion);

        return ventas
            .Select(venta => new ResumenVenta(
                venta.Id,
                venta.Fecha,
                venta.Cliente,
                venta.Producto,
                venta.UniqueProducto,
                venta.Cantidad,
                venta.Precio,
                venta.Descuento ?? decimal.Zero,
                venta.Total))
            .ToList();
    }
}
