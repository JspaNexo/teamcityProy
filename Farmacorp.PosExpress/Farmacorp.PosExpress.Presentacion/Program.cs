using System.Globalization;
using System.Text;
using Farmacorp.PosExpress.Datos;
using Farmacorp.PosExpress.Negocio;
using Farmacorp.PosExpress.Presentacion.Recursos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Farmacorp.PosExpress.Presentacion;

public static class Program
{
    private const string NOMBRE_CADENA_CONEXION = "PosExpress";

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var constructor = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        var cadenaConexion = constructor.Configuration.GetConnectionString(NOMBRE_CADENA_CONEXION)
            ?? throw new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture, Textos.CadenaConexionNoConfigurada, NOMBRE_CADENA_CONEXION));

        // Composición de dependencias (IoC): cada capa registra sus propios servicios
        constructor.Services
            .AgregarCapaDatos(cadenaConexion)
            .AgregarCapaNegocio(constructor.Configuration);

        constructor.Services.AddSingleton<MenuPrincipal>();

        using var host = constructor.Build();

        Console.WriteLine(Textos.InicializandoBaseDatos);
        await host.Services.AplicarMigracionesAsync();

        await host.Services.GetRequiredService<MenuPrincipal>().EjecutarAsync();
    }
}
