using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Negocio.Servicios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Farmacorp.PosExpress.Negocio;

/// <summary>
/// Registro de dependencias de la capa de negocio en el contenedor de IoC.
/// </summary>
public static class ConfiguracionServiciosNegocio
{
    public static IServiceCollection AgregarCapaNegocio(this IServiceCollection servicios, IConfiguration configuracion)
    {
        servicios.Configure<ConfiguracionEstrategia>(configuracion.GetSection(ConfiguracionEstrategia.SECCION));

        servicios.TryAddSingleton(TimeProvider.System);
        servicios.AddSingleton<IGeneradorCodigo, GeneradorCodigo>();

        // Estrategias de reglas de negocio (Base y GanaMax) y su selector
        servicios.AddSingleton<ReglasNegocioBase>();
        servicios.AddSingleton<ReglasNegocioGanaMax>();
        servicios.AddSingleton<ISelectorReglasNegocio, SelectorReglasNegocio>();

        servicios.AddScoped<IProductoServicio, ProductoServicio>();
        servicios.AddScoped<ICodigoBarraServicio, CodigoBarraServicio>();
        servicios.AddScoped<ICategoriaServicio, CategoriaServicio>();
        servicios.AddScoped<IVentaServicio, VentaServicio>();

        return servicios;
    }
}
