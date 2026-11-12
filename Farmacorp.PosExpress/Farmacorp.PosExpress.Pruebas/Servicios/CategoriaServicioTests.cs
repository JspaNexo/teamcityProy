using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Pruebas.Infraestructura;

namespace Farmacorp.PosExpress.Pruebas.Servicios;

public class CategoriaServicioTests
{
    [Fact]
    public async Task Asignar_RegistraDetalleYCuentaCategorias()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync();

        var primera = await escenario.CategoriaServicio.AsignarAsync(producto.IdProducto, EscenarioPruebas.CATEGORIA_ANALGESICOS);
        var segunda = await escenario.CategoriaServicio.AsignarAsync(producto.IdProducto, EscenarioPruebas.CATEGORIA_ANTIINFLAMATORIOS);

        Assert.Equal(1, primera.TotalCategoriasProducto);
        Assert.Equal(2, segunda.TotalCategoriasProducto);
    }

    [Fact]
    public async Task Asignar_NormalizaElIdDeCategoria()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync();

        var asignada = await escenario.CategoriaServicio.AsignarAsync(producto.IdProducto, "  med-analg ");

        Assert.Equal(EscenarioPruebas.CATEGORIA_ANALGESICOS, asignada.IdCategoria);
    }

    [Fact]
    public async Task Asignar_CategoriaDuplicada_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync(categorias: EscenarioPruebas.CATEGORIA_ANALGESICOS);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.CategoriaServicio.AsignarAsync(producto.IdProducto, EscenarioPruebas.CATEGORIA_ANALGESICOS));

        Assert.Equal(CodigosError.REGISTRO_DUPLICADO, excepcion.Codigo);
    }

    [Fact]
    public async Task Asignar_CategoriaInactiva_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.CategoriaServicio.AsignarAsync(producto.IdProducto, EscenarioPruebas.CATEGORIA_INACTIVA));

        Assert.Equal(CodigosError.OPERACION_NO_PERMITIDA, excepcion.Codigo);
    }

    [Fact]
    public async Task Asignar_CategoriaInexistente_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.CategoriaServicio.AsignarAsync(producto.IdProducto, "NO-EXISTE"));

        Assert.Equal(CodigosError.RECURSO_NO_ENCONTRADO, excepcion.Codigo);
    }
}
