using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;

namespace Farmacorp.PosExpress.Pruebas.Estrategias;

public class ReglasNegocioGanaMaxTests
{
    private readonly ReglasNegocioGanaMax _reglas = new();

    [Theory]
    [InlineData(100, 180)]
    [InlineData(10.50, 18.90)]
    public void CalcularPrecio_AplicaMargenDel80PorCiento(decimal costo, decimal precioEsperado)
    {
        Assert.Equal(precioEsperado, _reglas.CalcularPrecio(costo));
    }

    [Fact]
    public void CalcularDescuento_ConUnaCategoria_Aplica10PorCiento()
    {
        Assert.Equal(10m, _reglas.CalcularDescuento(100m, 1));
    }

    [Fact]
    public void CalcularDescuento_ConVariasCategorias_NoAplicaDescuento()
    {
        Assert.Equal(0m, _reglas.CalcularDescuento(100m, 2));
    }

    [Fact]
    public void ValidarStock_DejaMasDe10Unidades_NoLanzaExcepcion()
    {
        _reglas.ValidarStock("Producto", 20, 9);
    }

    [Theory]
    [InlineData(20, 10)]
    [InlineData(11, 1)]
    [InlineData(10, 1)]
    public void ValidarStock_DejaHasta10Unidades_LanzaExcepcion(int stock, int cantidad)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarStock("Producto", stock, cantidad));

        Assert.Equal(CodigosError.STOCK_MINIMO_GANAMAX, excepcion.Codigo);
    }

    [Fact]
    public void ValidarStock_SinStockDisponible_MantieneReglaBase()
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarStock("Producto", 5, 6));

        Assert.Equal(CodigosError.STOCK_INSUFICIENTE, excepcion.Codigo);
    }
}
