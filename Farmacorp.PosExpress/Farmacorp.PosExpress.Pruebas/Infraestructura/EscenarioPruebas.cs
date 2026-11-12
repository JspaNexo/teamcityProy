using Farmacorp.PosExpress.Datos;
using Farmacorp.PosExpress.Entidades;
using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Modelos;
using Farmacorp.PosExpress.Negocio.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farmacorp.PosExpress.Pruebas.Infraestructura;

/// <summary>
/// Arma los servicios de negocio sobre una base de datos en memoria con los datos semilla.
/// </summary>
public sealed class EscenarioPruebas : IDisposable
{
    public const int ID_TIPO_MEDICAMENTO = 1;
    public const string CATEGORIA_ANALGESICOS = "MED-ANALG";
    public const string CATEGORIA_ANTIINFLAMATORIOS = "MED-AINE";
    public const string CATEGORIA_INACTIVA = "DESC";

    public static readonly DateTime FechaActual = new(2026, 12, 15, 10, 0, 0);

    public EscenarioPruebas(TiposEstrategia estrategia = TiposEstrategia.Base)
    {
        var opciones = new DbContextOptionsBuilder<PosExpressContexto>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Contexto = new PosExpressContexto(opciones);
        Contexto.Database.EnsureCreated();

        var tiempo = new TiempoFijo(FechaActual);
        var unidadTrabajo = new UnitOfWork(Contexto);
        var generadorCodigo = new GeneradorCodigo();

        Selector = new SelectorReglasNegocio(
            new ReglasNegocioBase(),
            new ReglasNegocioGanaMax(),
            Options.Create(new ConfiguracionEstrategia { Tipo = estrategia }),
            tiempo);

        ProductoServicio = new ProductoServicio(unidadTrabajo, Selector, generadorCodigo, tiempo);
        CodigoBarraServicio = new CodigoBarraServicio(unidadTrabajo, generadorCodigo);
        CategoriaServicio = new CategoriaServicio(unidadTrabajo, tiempo);
        VentaServicio = new VentaServicio(unidadTrabajo, Selector, tiempo);
    }

    public PosExpressContexto Contexto { get; }

    public SelectorReglasNegocio Selector { get; }

    public ProductoServicio ProductoServicio { get; }

    public CodigoBarraServicio CodigoBarraServicio { get; }

    public CategoriaServicio CategoriaServicio { get; }

    public VentaServicio VentaServicio { get; }

    public static NuevoProductoErp CrearNuevoProducto(decimal costo = 100m, int stock = 50)
    {
        return new NuevoProductoErp("Paracetamol 500mg", costo, stock, FechaActual.AddYears(1), ID_TIPO_MEDICAMENTO, null);
    }

    public async Task<ProductoRegistrado> RegistrarProductoAsync(decimal costo = 100m, int stock = 50, params string[] categorias)
    {
        var producto = await ProductoServicio.RegistrarAsync(CrearNuevoProducto(costo, stock));

        foreach (var categoria in categorias)
        {
            await CategoriaServicio.AsignarAsync(producto.IdProducto, categoria);
        }

        return producto;
    }

    public async Task<ErpProducto> ObtenerErpAsync(long idProducto)
    {
        return await Contexto.ErpProductos.AsNoTracking().SingleAsync(erp => erp.IdProducto == idProducto);
    }

    public void Dispose()
    {
        Contexto.Dispose();
    }
}
