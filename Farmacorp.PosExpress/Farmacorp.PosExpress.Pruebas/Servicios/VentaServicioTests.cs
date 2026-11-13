using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Modelos;
using Farmacorp.PosExpress.Pruebas.Infraestructura;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Pruebas.Servicios;

public class VentaServicioTests
{
    [Fact]
    public async Task Registrar_ConUnaCategoria_AplicaDescuentoRegistraUniqueCodigoYActualizaStock()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync(costo: 100m, stock: 50, EscenarioPruebas.CATEGORIA_ANALGESICOS);

        var venta = await escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 2, "Juan Pérez"));

        // Precio Base = 150; subtotal = 300; descuento 30% = 90
        Assert.Equal(150m, venta.Precio);
        Assert.Equal(300m, venta.Subtotal);
        Assert.Equal(90m, venta.Descuento);
        Assert.Equal(210m, venta.Total);

        // Regla de Negocio 2
        Assert.Equal(producto.UniqueCodigo, venta.UniqueProducto);

        // Regla de Negocio 5
        Assert.Equal(48, venta.StockRestante);
        Assert.Equal(48, (await escenario.ObtenerErpAsync(producto.IdProducto)).Stock);

        var ventaGuardada = await escenario.Contexto.VentasExpress.AsNoTracking().SingleAsync();
        Assert.Equal(producto.UniqueCodigo, ventaGuardada.UniqueProducto);
        Assert.Equal(producto.Nombre, ventaGuardada.Producto);
        Assert.Equal(210m, ventaGuardada.Total);
    }

    [Fact]
    public async Task Registrar_SinCategorias_NoAplicaDescuento()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync(costo: 100m);

        var venta = await escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 1, null));

        Assert.Equal(0m, venta.Descuento);
        Assert.Equal(150m, venta.Total);
    }

    [Fact]
    public async Task Registrar_ConVariasCategorias_NoAplicaDescuento()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync(
            100m, 50, EscenarioPruebas.CATEGORIA_ANALGESICOS, EscenarioPruebas.CATEGORIA_ANTIINFLAMATORIOS);

        var venta = await escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 1, null));

        Assert.Equal(0m, venta.Descuento);
    }

    [Fact]
    public async Task Registrar_SinStockSuficiente_RechazaVentaYNoModificaStock()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync(stock: 3);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 4, null)));

        Assert.Equal(CodigosError.STOCK_INSUFICIENTE, excepcion.Codigo);
        Assert.Equal(3, (await escenario.ObtenerErpAsync(producto.IdProducto)).Stock);
        Assert.Empty(escenario.Contexto.VentasExpress);
    }

    [Fact]
    public async Task Registrar_StockModificadoPorOtraCaja_RechazaVentaSinPerderLaOtraActualizacion()
    {
        using var escenario = new EscenarioPruebas();

        // El contexto principal queda con el stock leído al registrar (20)
        var producto = await escenario.RegistrarProductoAsync(stock: 20);

        // Otra caja vende 5 unidades y deja el stock en 15
        await escenario.CrearVentaServicioOtraCaja().RegistrarAsync(new NuevaVenta(producto.IdProducto, 5, null));

        // La primera caja aún ve 20: sin control de concurrencia sobrescribiría el stock con 15 en vez de 10
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 5, null)));

        Assert.Equal(CodigosError.CONFLICTO_CONCURRENCIA, excepcion.Codigo);
        Assert.Equal(15, (await escenario.ObtenerErpAsync(producto.IdProducto)).Stock);
    }

    [Fact]
    public async Task Registrar_EstrategiaBase_PermiteVenderTodoElStock()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync(stock: 5);

        var venta = await escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 5, null));

        Assert.Equal(0, venta.StockRestante);
    }

    [Fact]
    public async Task Registrar_EstrategiaGanaMax_AplicaDescuentoDel10PorCiento()
    {
        using var escenario = new EscenarioPruebas(TiposEstrategia.GanaMax);
        var producto = await escenario.RegistrarProductoAsync(costo: 100m, stock: 50, EscenarioPruebas.CATEGORIA_ANALGESICOS);

        var venta = await escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 1, null));

        // Precio GanaMax = 180; descuento 10% = 18
        Assert.Equal(180m, venta.Precio);
        Assert.Equal(18m, venta.Descuento);
        Assert.Equal(162m, venta.Total);
        Assert.Equal(TiposEstrategia.GanaMax, venta.EstrategiaAplicada);
    }

    [Fact]
    public async Task Registrar_EstrategiaGanaMax_RechazaVentaQueDejaStockDe10OMenos()
    {
        using var escenario = new EscenarioPruebas(TiposEstrategia.GanaMax);
        var producto = await escenario.RegistrarProductoAsync(stock: 15);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 5, null)));

        Assert.Equal(CodigosError.STOCK_MINIMO_GANAMAX, excepcion.Codigo);
        Assert.Equal(15, (await escenario.ObtenerErpAsync(producto.IdProducto)).Stock);
    }

    [Fact]
    public async Task Registrar_VolviendoABase_AplicaNuevamenteReglasBase()
    {
        using var escenario = new EscenarioPruebas(TiposEstrategia.GanaMax);
        var producto = await escenario.RegistrarProductoAsync(costo: 100m, stock: 15, EscenarioPruebas.CATEGORIA_ANALGESICOS);

        escenario.Selector.CambiarEstrategia(TiposEstrategia.Base);
        var venta = await escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 5, null));

        // El precio quedó fijado al registrar (GanaMax = 180); el descuento y el stock usan reglas Base
        Assert.Equal(TiposEstrategia.Base, venta.EstrategiaAplicada);
        Assert.Equal(270m, venta.Descuento);
        Assert.Equal(10, venta.StockRestante);
    }

    [Fact]
    public async Task Registrar_CantidadInvalida_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.VentaServicio.RegistrarAsync(new NuevaVenta(producto.IdProducto, 0, null)));

        Assert.Equal(CodigosError.DATO_INVALIDO, excepcion.Codigo);
    }

    [Fact]
    public async Task Registrar_ProductoInexistente_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.VentaServicio.RegistrarAsync(new NuevaVenta(999, 1, null)));

        Assert.Equal(CodigosError.RECURSO_NO_ENCONTRADO, excepcion.Codigo);
    }
}
