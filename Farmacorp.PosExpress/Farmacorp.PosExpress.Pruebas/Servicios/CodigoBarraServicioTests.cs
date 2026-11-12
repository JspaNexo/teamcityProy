using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Pruebas.Infraestructura;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Pruebas.Servicios;

public class CodigoBarraServicioTests
{
    [Fact]
    public async Task Asignar_GeneraUniqueCodigoActivoParaElProducto()
    {
        using var escenario = new EscenarioPruebas();
        var producto = await escenario.RegistrarProductoAsync();

        var primero = await escenario.CodigoBarraServicio.AsignarAsync(producto.IdProducto);
        var segundo = await escenario.CodigoBarraServicio.AsignarAsync(producto.IdProducto);

        Assert.NotEqual(Guid.Empty, primero.UniqueCodigo);
        Assert.NotEqual(primero.UniqueCodigo, segundo.UniqueCodigo);
        Assert.NotEqual(producto.UniqueCodigo, primero.UniqueCodigo);

        var codigos = await escenario.Contexto.CodigosBarras
            .Where(codigo => codigo.IdProducto == producto.IdProducto)
            .ToListAsync();

        Assert.Equal(2, codigos.Count);
        Assert.All(codigos, codigo => Assert.True(codigo.Activo));
    }

    [Fact]
    public async Task Asignar_ProductoInexistente_LanzaExcepcion()
    {
        using var escenario = new EscenarioPruebas();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => escenario.CodigoBarraServicio.AsignarAsync(999));

        Assert.Equal(CodigosError.RECURSO_NO_ENCONTRADO, excepcion.Codigo);
    }
}
