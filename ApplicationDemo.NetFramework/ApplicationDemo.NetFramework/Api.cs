using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Web.Script.Serialization;
using ApplicationDemo.NetFramework.Services;

namespace ApplicationDemo.NetFramework
{
    /// <summary>Respuesta HTTP ya serializada: código de estado y cuerpo JSON.</summary>
    public class Respuesta
    {
        public Respuesta(int estado, string json)
        {
            Estado = estado;
            Json = json;
        }

        public int Estado { get; }
        public string Json { get; }
    }

    /// <summary>
    /// Las mismas rutas que la API .NET 10 (ApplicationDemo/Program.cs). No usa HttpContext, así las pruebas
    /// la ejecutan sin IIS; ApiHandler solo la conecta con la petición real.
    /// </summary>
    public class Api
    {
        private const string RutaProducto = "/api/productos/";

        private readonly ProductoService _servicio;

        public Api(ProductoService servicio)
        {
            _servicio = servicio;
        }

        public Respuesta Procesar(string metodo, string ruta, NameValueCollection query, string cuerpo)
        {
            // "/api/productos/" y "/api/productos" son la misma ruta
            var path = "/" + (ruta ?? "").Trim('/').ToLowerInvariant();
            var esGet = metodo == "GET";

            switch (path)
            {
                case "/":
                    return esGet ? Ok(Informacion()) : MetodoNoPermitido();
                case "/health":
                    return esGet ? Ok(new { status = "Healthy" }) : MetodoNoPermitido();
                case "/api/productos":
                    if (esGet) return Ok(_servicio.ObtenerTodos().Select(Vista).ToList());
                    if (metodo == "POST") return CrearProducto(cuerpo);
                    return MetodoNoPermitido();
                case "/api/total":
                    return esGet ? CalcularTotal(query ?? new NameValueCollection()) : MetodoNoPermitido();
            }

            if (path.StartsWith(RutaProducto, StringComparison.Ordinal)
                && int.TryParse(path.Substring(RutaProducto.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            {
                if (!esGet) return MetodoNoPermitido();

                var producto = _servicio.ObtenerPorId(id);
                return producto != null ? Ok(Vista(producto)) : NoEncontrado();
            }

            return NoEncontrado();
        }

        private static object Informacion()
        {
            var ensamblado = typeof(Api).Assembly;

            return new
            {
                app = "ApplicationDemo.NetFramework",
                // InformationalVersion es la de -p:Version tal cual (1.0.7)
                version = ensamblado.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                framework = ensamblado.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName,
                clr = Environment.Version.ToString(),
                // IIS define APP_POOL_ID en el proceso w3wp: muestra qué App Pool atendió la petición
                appPool = Environment.GetEnvironmentVariable("APP_POOL_ID"),
                instancia = Environment.MachineName
            };
        }

        private Respuesta CrearProducto(string cuerpo)
        {
            CrearProductoRequest request;

            try
            {
                request = new JavaScriptSerializer().Deserialize<CrearProductoRequest>(cuerpo ?? "");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is FormatException)
            {
                return Error("El cuerpo no es un producto JSON válido.");
            }

            if (request == null) return Error("Falta el producto en el cuerpo.");

            try
            {
                return new Respuesta(201, Serializar(Vista(_servicio.Crear(request))));
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message);
            }
        }

        private static Respuesta CalcularTotal(NameValueCollection query)
        {
            if (!decimal.TryParse(query["precio"], NumberStyles.Number, CultureInfo.InvariantCulture, out var precio))
                return Error("Parámetro inválido: precio");
            if (!int.TryParse(query["cantidad"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var cantidad))
                return Error("Parámetro inválido: cantidad");

            decimal descuento = 0;
            if (query["descuento"] != null
                && !decimal.TryParse(query["descuento"], NumberStyles.Number, CultureInfo.InvariantCulture, out descuento))
                return Error("Parámetro inválido: descuento");

            try
            {
                return Ok(new { total = ProductoService.CalcularTotal(precio, cantidad, descuento) });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Error("Parámetro inválido: " + ex.ParamName);
            }
        }

        // Mismos nombres en minúscula que el JSON de la API .NET 10
        private static object Vista(Producto producto) =>
            new { id = producto.Id, nombre = producto.Nombre, precio = producto.Precio };

        private static string Serializar(object contenido) => new JavaScriptSerializer().Serialize(contenido);

        private static Respuesta Ok(object contenido) => new Respuesta(200, Serializar(contenido));

        private static Respuesta Error(string mensaje) => new Respuesta(400, Serializar(new { error = mensaje }));

        private static Respuesta NoEncontrado() => new Respuesta(404, "");

        private static Respuesta MetodoNoPermitido() => new Respuesta(405, "");
    }
}
