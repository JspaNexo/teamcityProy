using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Pruebas.Infraestructura;

namespace Farmacorp.PosExpress.Pruebas.Servicios;

public class ProductoServicioTests
{
    [Fact]
    public async Task Registrar_EstrategiaBase_CreaProductoErpConPrecioYUniqueCodigo()
    {
        using var escenario = new EscenarioPruebas();

        var registrado = await escenario.ProductoServicio.RegistrarAsync(EscenarioPruebas.CrearNuevoProducto(costo: 100m, stock: 40));

        Assert.Equal(150m, registrado.Precio);
        Assert.NotEqual(Guid.Empty, registrado.UniqueCodigo);
        Assert.Equal(TiposEstrategia.Base, registrado.EstrategiaAplicada);

        var erp = await escenario.ObtenerErpAsync(registrado.IdProducto);
        Assert.Equal(100m, erp.Costo);
        Assert.Equal(40, erp.Stock);
        Assert.Equal(registrado.UniqueCodigo, erp.UniqueCodigo);
        Assert.Equal(EscenarioPruebas.FechaActual, erp.FechaRegistro);
    }

    [Fact]
    public async Task Registrar_EstrategiaGanaMax_AplicaMargenDel80PorCiento()
    {
        using var escenario = new EscenarioPruebas(TiposEstrategia.GanaMax);

        var registrado = await escenario.ProductoServicio.RegistrarAsync(EscenarioPruebas.CrearNuevoProducto(costo: 100m));

        Assert.Equal(180m, registrado.Precio);
        Assert.Equal(TiposEstrategia.GanaMax, registrado.EstrategiaAplicada);
    }

    [Fact]
    public async Task Registrar_GeneraUniqueCodigoDistintoPorProducto()
    {
        using var escenario = new EscenarioPruebas();

        var primero = await escenario.RegistrarProductoAsync();
        var segundo = await escenario.RegistrarProductoAsync();

        Assert.NotEqual(primero.UniqueCodigo, segundo.UniqueCodigo);
    }

    [Fact]
    public async Task Registrar_TipoProductoInexistente_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var nuevoProducto = EscenarioPruebas.CrearNuevoProducto() with { IdTipoProducto = 999 };

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.ProductoServicio.RegistrarAsync(nuevoProducto));

        Assert.Equal(CodigosError.RECURSO_NO_ENCONTRADO, excepcion.Codigo);
    }

    [Theory]
    [InlineData(10.555)]
    [InlineData(0.001)]
    public async Task Registrar_CostoConMasDeDosDecimales_LanzaExcepcion(decimal costo)
    {
        using var escenario = new EscenarioPruebas();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.ProductoServicio.RegistrarAsync(EscenarioPruebas.CrearNuevoProducto(costo: costo)));

        Assert.Equal(CodigosError.DATO_INVALIDO, excepcion.Codigo);
    }

    [Fact]
    public async Task Registrar_CostoConCerosDecimalesExtra_EsValido()
    {
        using var escenario = new EscenarioPruebas();

        var registrado = await escenario.ProductoServicio.RegistrarAsync(EscenarioPruebas.CrearNuevoProducto(costo: 10.500m));

        Assert.Equal(15.75m, registrado.Precio);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Registrar_CostoInvalido_LanzaExcepcion(decimal costo)
    {
        using var escenario = new EscenarioPruebas();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.ProductoServicio.RegistrarAsync(EscenarioPruebas.CrearNuevoProducto(costo: costo)));

        Assert.Equal(CodigosError.DATO_INVALIDO, excepcion.Codigo);
    }

    [Fact]
    public async Task Registrar_ProductoVencido_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var nuevoProducto = EscenarioPruebas.CrearNuevoProducto() with { FechaVencimiento = EscenarioPruebas.FechaActual };

        await Assert.ThrowsAsync<ReglaNegocioException>(() => escenario.ProductoServicio.RegistrarAsync(nuevoProducto));
    }

    [Fact]
    public async Task Registrar_ObservacionesExcedenLongitud_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var nuevoProducto = EscenarioPruebas.CrearNuevoProducto() with { Observaciones = "Texto demasiado largo" };

        await Assert.ThrowsAsync<ReglaNegocioException>(() => escenario.ProductoServicio.RegistrarAsync(nuevoProducto));
    }
}
