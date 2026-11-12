using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Modelos;
using Farmacorp.PosExpress.Negocio.Servicios;
using Farmacorp.PosExpress.Presentacion.Consola;
using Farmacorp.PosExpress.Presentacion.Recursos;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacorp.PosExpress.Presentacion;

/// <summary>
/// Menú de la aplicación de consola. Cada operación se ejecuta en su propio ámbito de DI
/// (un DbContext / UnitOfWork por transacción).
/// </summary>
public class MenuPrincipal
{
    private const int LONGITUD_MAXIMA_COLUMNA = 25;
    private const string SEPARADOR_ENCABEZADO = "|";
    private const string SEPARADOR_CATEGORIAS = ", ";
    private const string FORMATO_FECHA_HORA = "dd/MM/yyyy HH:mm";

    private readonly IServiceScopeFactory _fabricaAmbitos;
    private readonly ISelectorReglasNegocio _selectorReglas;

    public MenuPrincipal(IServiceScopeFactory fabricaAmbitos, ISelectorReglasNegocio selectorReglas)
    {
        _fabricaAmbitos = fabricaAmbitos;
        _selectorReglas = selectorReglas;
    }

    public async Task EjecutarAsync()
    {
        try
        {
            var continuar = true;

            while (continuar)
            {
                MostrarMenu();
                continuar = await ProcesarOpcionAsync(LectorConsola.LeerEntero(Textos.SolicitarOpcion));
            }
        }
        catch (EndOfStreamException)
        {
            // Entrada finalizada: se cierra la aplicación
        }
    }

    private void MostrarMenu()
    {
        EscritorConsola.EscribirTitulo(Textos.TituloAplicacion);
        MostrarEstrategiaVigente();
        Console.WriteLine();
        Console.WriteLine(Textos.MenuRegistrarProducto);
        Console.WriteLine(Textos.MenuAsignarCodigoBarra);
        Console.WriteLine(Textos.MenuAsignarCategoria);
        Console.WriteLine(Textos.MenuRegistrarVenta);
        Console.WriteLine(Textos.MenuListarProductos);
        Console.WriteLine(Textos.MenuListarVentas);
        Console.WriteLine(Textos.MenuCambiarEstrategia);
        Console.WriteLine(Textos.MenuSalir);
        Console.WriteLine();
    }

    private void MostrarEstrategiaVigente()
    {
        var reglasVigentes = _selectorReglas.ObtenerReglasVigentes();
        EscritorConsola.EscribirLinea(Textos.EstrategiaVigente, reglasVigentes.Estrategia);

        var ganaMaxFueraVigencia = _selectorReglas.EstrategiaSeleccionada == TiposEstrategia.GanaMax
            && reglasVigentes.Estrategia != TiposEstrategia.GanaMax;

        if (ganaMaxFueraVigencia)
        {
            EscritorConsola.EscribirAdvertencia(
                Textos.EstrategiaFueraVigencia,
                FormatearFechaVigencia(_selectorReglas.FechaInicioGanaMax),
                FormatearFechaVigencia(_selectorReglas.FechaFinGanaMax));
        }
    }

    private async Task<bool> ProcesarOpcionAsync(int opcion)
    {
        switch ((OpcionesMenu)opcion)
        {
            case OpcionesMenu.Salir:
                return false;
            case OpcionesMenu.RegistrarProducto:
                await EjecutarOperacionAsync(RegistrarProductoAsync);
                break;
            case OpcionesMenu.AsignarCodigoBarra:
                await EjecutarOperacionAsync(AsignarCodigoBarraAsync);
                break;
            case OpcionesMenu.AsignarCategoria:
                await EjecutarOperacionAsync(AsignarCategoriaAsync);
                break;
            case OpcionesMenu.RegistrarVenta:
                await EjecutarOperacionAsync(RegistrarVentaAsync);
                break;
            case OpcionesMenu.ListarProductos:
                await EjecutarOperacionAsync(ListarProductosAsync);
                break;
            case OpcionesMenu.ListarVentas:
                await EjecutarOperacionAsync(ListarVentasAsync);
                break;
            case OpcionesMenu.CambiarEstrategia:
                CambiarEstrategia();
                break;
            default:
                EscritorConsola.EscribirAdvertencia(Textos.OpcionInvalida);
                break;
        }

        return true;
    }

    private async Task EjecutarOperacionAsync(Func<IServiceProvider, Task> operacion)
    {
        try
        {
            using var ambito = _fabricaAmbitos.CreateScope();
            await operacion(ambito.ServiceProvider);
        }
        catch (ReglaNegocioException excepcion)
        {
            EscritorConsola.EscribirError(Textos.ErrorReglaNegocio, excepcion.Codigo, excepcion.Message);
        }
        catch (EndOfStreamException)
        {
            throw;
        }
        catch (Exception excepcion)
        {
            EscritorConsola.EscribirError(Textos.ErrorInesperado, excepcion.GetBaseException().Message);
        }

        EscritorConsola.Pausar();
    }

    private static async Task RegistrarProductoAsync(IServiceProvider proveedor)
    {
        var productoServicio = proveedor.GetRequiredService<IProductoServicio>();

        EscritorConsola.EscribirTitulo(Textos.TituloRegistrarProducto);
        Console.WriteLine(Textos.TiposProductoDisponibles);

        foreach (var tipo in await productoServicio.ListarTiposAsync())
        {
            EscritorConsola.EscribirLinea(Textos.FormatoTipoProducto, tipo.IdTipoProducto, tipo.Descripcion);
        }

        var nuevoProducto = new NuevoProductoErp(
            LectorConsola.LeerTexto(Textos.SolicitarNombre),
            LectorConsola.LeerDecimal(Textos.SolicitarCosto),
            LectorConsola.LeerEntero(Textos.SolicitarStock),
            LectorConsola.LeerFecha(Textos.SolicitarFechaVencimiento),
            LectorConsola.LeerEntero(Textos.SolicitarTipoProducto),
            LectorConsola.LeerTextoOpcional(Textos.SolicitarObservaciones));

        var registrado = await productoServicio.RegistrarAsync(nuevoProducto);

        EscritorConsola.EscribirExito(
            Textos.ProductoRegistrado,
            registrado.EstrategiaAplicada,
            registrado.IdProducto,
            registrado.Costo,
            registrado.Precio,
            registrado.UniqueCodigo,
            registrado.Stock);
    }

    private static async Task AsignarCodigoBarraAsync(IServiceProvider proveedor)
    {
        var codigoBarraServicio = proveedor.GetRequiredService<ICodigoBarraServicio>();

        EscritorConsola.EscribirTitulo(Textos.TituloAsignarCodigoBarra);

        var asignado = await codigoBarraServicio.AsignarAsync(LectorConsola.LeerEnteroLargo(Textos.SolicitarIdProducto));

        EscritorConsola.EscribirExito(Textos.CodigoBarraAsignado, asignado.IdCodigoBarra, asignado.IdProducto, asignado.UniqueCodigo);
    }

    private static async Task AsignarCategoriaAsync(IServiceProvider proveedor)
    {
        var categoriaServicio = proveedor.GetRequiredService<ICategoriaServicio>();

        EscritorConsola.EscribirTitulo(Textos.TituloAsignarCategoria);
        Console.WriteLine(Textos.CategoriasDisponibles);

        foreach (var categoria in await categoriaServicio.ListarAsync())
        {
            EscritorConsola.EscribirLinea(
                Textos.FormatoCategoria,
                categoria.IdCategoria,
                categoria.Descripcion,
                categoria.Activo ? Textos.EstadoActiva : Textos.EstadoInactiva,
                categoria.IdCategoriaPadre ?? string.Empty);
        }

        var idProducto = LectorConsola.LeerEnteroLargo(Textos.SolicitarIdProducto);
        var idCategoria = LectorConsola.LeerTexto(Textos.SolicitarIdCategoria);

        var asignada = await categoriaServicio.AsignarAsync(idProducto, idCategoria);

        EscritorConsola.EscribirExito(
            Textos.CategoriaAsignada,
            asignada.IdDetalle,
            asignada.IdProducto,
            asignada.IdCategoria,
            asignada.TotalCategoriasProducto);
    }

    private static async Task RegistrarVentaAsync(IServiceProvider proveedor)
    {
        var ventaServicio = proveedor.GetRequiredService<IVentaServicio>();

        EscritorConsola.EscribirTitulo(Textos.TituloRegistrarVenta);

        var nuevaVenta = new NuevaVenta(
            LectorConsola.LeerEnteroLargo(Textos.SolicitarIdProducto),
            LectorConsola.LeerEntero(Textos.SolicitarCantidad),
            LectorConsola.LeerTextoOpcional(Textos.SolicitarCliente));

        var venta = await ventaServicio.RegistrarAsync(nuevaVenta);

        EscritorConsola.EscribirExito(
            Textos.VentaRegistrada,
            venta.EstrategiaAplicada,
            venta.IdVenta,
            venta.Producto,
            venta.UniqueProducto,
            venta.Cantidad,
            venta.Precio,
            venta.Subtotal,
            venta.Descuento,
            venta.Total,
            venta.StockRestante);
    }

    private static async Task ListarProductosAsync(IServiceProvider proveedor)
    {
        var productos = await proveedor.GetRequiredService<IProductoServicio>().ListarAsync();

        EscritorConsola.EscribirTitulo(Textos.TituloListarProductos);

        if (productos.Count == 0)
        {
            Console.WriteLine(Textos.SinRegistros);
            return;
        }

        EscritorConsola.EscribirLinea(Textos.FormatoProducto, ObtenerEncabezado(Textos.EncabezadoProductos));

        foreach (var producto in productos)
        {
            EscritorConsola.EscribirLinea(
                Textos.FormatoProducto,
                producto.IdProducto,
                Recortar(producto.Nombre),
                producto.Precio,
                producto.Costo,
                producto.Stock,
                producto.CantidadCodigosBarras,
                producto.UniqueCodigo?.ToString() ?? Textos.ValorNoDisponible,
                producto.Categorias.Count == 0 ? Textos.ValorNoDisponible : string.Join(SEPARADOR_CATEGORIAS, producto.Categorias));
        }
    }

    private static async Task ListarVentasAsync(IServiceProvider proveedor)
    {
        var ventas = await proveedor.GetRequiredService<IVentaServicio>().ListarRecientesAsync();

        EscritorConsola.EscribirTitulo(Textos.TituloListarVentas);

        if (ventas.Count == 0)
        {
            Console.WriteLine(Textos.SinRegistros);
            return;
        }

        EscritorConsola.EscribirLinea(Textos.FormatoVenta, ObtenerEncabezado(Textos.EncabezadoVentas));

        foreach (var venta in ventas)
        {
            EscritorConsola.EscribirLinea(
                Textos.FormatoVenta,
                venta.IdVenta,
                venta.Fecha.ToString(FORMATO_FECHA_HORA),
                venta.Cliente ?? Textos.ValorNoDisponible,
                Recortar(venta.Producto),
                venta.Cantidad,
                venta.Precio,
                venta.Descuento,
                venta.Total,
                venta.UniqueProducto?.ToString() ?? Textos.ValorNoDisponible);
        }
    }

    private void CambiarEstrategia()
    {
        EscritorConsola.EscribirTitulo(Textos.TituloCambiarEstrategia);

        var estrategias = Enum.GetValues<TiposEstrategia>();

        for (var indice = 0; indice < estrategias.Length; indice++)
        {
            EscritorConsola.EscribirLinea(Textos.FormatoOpcionEstrategia, indice + 1, estrategias[indice]);
        }

        var seleccion = LectorConsola.LeerEntero(Textos.SolicitarEstrategia) - 1;

        if (seleccion < 0 || seleccion >= estrategias.Length)
        {
            EscritorConsola.EscribirAdvertencia(Textos.OpcionInvalida);
            return;
        }

        _selectorReglas.CambiarEstrategia(estrategias[seleccion]);
        EscritorConsola.EscribirExito(Textos.EstrategiaCambiada, estrategias[seleccion]);
    }

    private static object[] ObtenerEncabezado(string encabezado)
    {
        return encabezado.Split(SEPARADOR_ENCABEZADO).Cast<object>().ToArray();
    }

    private static string Recortar(string texto)
    {
        return texto.Length <= LONGITUD_MAXIMA_COLUMNA ? texto : texto[..LONGITUD_MAXIMA_COLUMNA];
    }

    private static string FormatearFechaVigencia(DateOnly? fecha)
    {
        return fecha?.ToString(LectorConsola.FORMATO_FECHA) ?? Textos.SinLimite;
    }
}
