using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;

namespace Farmacorp.PosExpress.Pruebas.Estrategias;

public class ReglasNegocioBaseTests
{
    private readonly ReglasNegocioBase _reglas = new();

    [Theory]
    [InlineData(100, 150)]
    [InlineData(10.50, 15.75)]
    [InlineData(0.99, 1.49)]
    public void CalcularPrecio_AplicaMargenDel50PorCiento(decimal costo, decimal precioEsperado)
    {
        Assert.Equal(precioEsperado, _reglas.CalcularPrecio(costo));
    }

    [Fact]
    public void CalcularDescuento_ConUnaCategoria_Aplica30PorCiento()
    {
        Assert.Equal(30m, _reglas.CalcularDescuento(100m, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalcularDescuento_SinCategoriaUnica_NoAplicaDescuento(int cantidadCategorias)
    {
        Assert.Equal(0m, _reglas.CalcularDescuento(100m, cantidadCategorias));
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(5, 1)]
    public void ValidarStock_ConStockSuficiente_NoLanzaExcepcion(int stock, int cantidad)
    {
        _reglas.ValidarStock("Producto", stock, cantidad);
    }

    [Theory]
    [InlineData(5, 6)]
    [InlineData(0, 1)]
    public void ValidarStock_ConStockInsuficiente_LanzaExcepcion(int stock, int cantidad)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarStock("Producto", stock, cantidad));

        Assert.Equal(CodigosError.STOCK_INSUFICIENTE, excepcion.Codigo);
    }
}
