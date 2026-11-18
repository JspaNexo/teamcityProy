using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ApplicationDemo.Services;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApplicationDemo.Tests;

public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_DevuelveOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductos_DevuelveLista()
    {
        var productos = await _client.GetFromJsonAsync<List<Producto>>("/api/productos");

        Assert.NotNull(productos);
        Assert.NotEmpty(productos);
    }

    [Fact]
    public async Task GetProducto_Inexistente_Devuelve404()
    {
        var response = await _client.GetAsync("/api/productos/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostProducto_Invalido_Devuelve400()
    {
        var response = await _client.PostAsJsonAsync("/api/productos", new CrearProductoRequest("", 10));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Raiz_DevuelveVersionComoEtiquetaDeImagen()
    {
        var info = await _client.GetFromJsonAsync<JsonElement>("/");

        // Igual que la etiqueta de la imagen (1.0.7): sin cuarta parte ni hash del commit
        Assert.Matches(@"^\d+\.\d+\.\d+$", info.GetProperty("version").GetString());
    }
}
