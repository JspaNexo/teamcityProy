using ApplicationDemo.Services;

namespace ApplicationDemo.Tests;

public class ProductoServiceTests
{
    [Fact]
    public void ObtenerTodos_DevuelveProductosIniciales()
    {
        var service = new ProductoService();

        Assert.Equal(3, service.ObtenerTodos().Count);
    }

    [Fact]
    public void Crear_AsignaIdIncremental()
    {
        var service = new ProductoService();

        var producto = service.Crear(new CrearProductoRequest("Aspirina", 9.99m));

        Assert.Equal(4, producto.Id);
        Assert.Equal(producto, service.ObtenerPorId(4));
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("Jarabe", 0)]
    [InlineData("Jarabe", -5)]
    public void Crear_ConDatosInvalidos_LanzaExcepcion(string nombre, decimal precio)
    {
        var service = new ProductoService();

        Assert.Throws<ArgumentException>(() => service.Crear(new CrearProductoRequest(nombre, precio)));
    }

    [Theory]
    [InlineData(10, 3, 0, 30)]
    [InlineData(10, 3, 10, 27)]
    [InlineData(12.5, 2, 50, 12.5)]
    public void CalcularTotal_AplicaDescuento(decimal precio, int cantidad, decimal descuento, decimal esperado)
    {
        Assert.Equal(esperado, ProductoService.CalcularTotal(precio, cantidad, descuento));
    }

    [Fact]
    public void CalcularTotal_ConCantidadCero_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ProductoService.CalcularTotal(10, 0));
    }
}
