using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Farmacorp.PosExpress.Datos;

/// <summary>
/// Fábrica usada solo en tiempo de diseño por las herramientas de EF (dotnet ef migrations).
/// </summary>
public class PosExpressContextoFactory : IDesignTimeDbContextFactory<PosExpressContexto>
{
    private const string CADENA_CONEXION_DISENO =
        @"Server=(localdb)\MSSQLLocalDB;Database=FarmacorpPosExpress;Trusted_Connection=True;TrustServerCertificate=True";

    public PosExpressContexto CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<PosExpressContexto>()
            .UseSqlServer(CADENA_CONEXION_DISENO)
            .Options;

        return new PosExpressContexto(opciones);
    }
}
