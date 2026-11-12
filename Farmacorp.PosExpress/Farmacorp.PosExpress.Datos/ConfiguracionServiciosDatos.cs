using Farmacorp.PosExpress.Datos.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacorp.PosExpress.Datos;

/// <summary>
/// Registro de dependencias de la capa de datos en el contenedor de IoC.
/// </summary>
public static class ConfiguracionServiciosDatos
{
    public static IServiceCollection AgregarCapaDatos(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddDbContext<PosExpressContexto>(opciones => opciones.UseSqlServer(cadenaConexion));
        servicios.AddScoped<IUnitOfWork, UnitOfWork>();

        return servicios;
    }

    /// <summary>
    /// Crea la base de datos (si no existe) y aplica las migraciones pendientes (Code First).
    /// </summary>
    public static async Task AplicarMigracionesAsync(this IServiceProvider proveedor, CancellationToken cancelacion = default)
    {
        using var ambito = proveedor.CreateScope();
        var contexto = ambito.ServiceProvider.GetRequiredService<PosExpressContexto>();

        await contexto.Database.MigrateAsync(cancelacion);
    }
}
