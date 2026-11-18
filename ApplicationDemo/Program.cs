using System.Reflection;
using ApplicationDemo.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ProductoService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    app = "ApplicationDemo",
    // InformationalVersion es la de -p:Version tal cual (1.0.7); GetName().Version la completaría a 4 partes (1.0.7.0)
    version = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
    entorno = app.Environment.EnvironmentName,
    // En un contenedor es el id del contenedor: permite ver qué réplica del Swarm respondió
    instancia = Environment.MachineName
}));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

var productos = app.MapGroup("/api/productos");

productos.MapGet("/", (ProductoService service) => service.ObtenerTodos());

productos.MapGet("/{id:int}", (int id, ProductoService service) =>
    service.ObtenerPorId(id) is { } producto ? Results.Ok(producto) : Results.NotFound());

productos.MapPost("/", (CrearProductoRequest request, ProductoService service) =>
{
    try
    {
        var producto = service.Crear(request);
        return Results.Created($"/api/productos/{producto.Id}", producto);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/total", (decimal precio, int cantidad, decimal? descuento) =>
{
    try
    {
        return Results.Ok(new { total = ProductoService.CalcularTotal(precio, cantidad, descuento ?? 0) });
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = $"Parámetro inválido: {ex.ParamName}" });
    }
});

app.Run();

// Necesario para WebApplicationFactory en las pruebas de integración
public partial class Program;
