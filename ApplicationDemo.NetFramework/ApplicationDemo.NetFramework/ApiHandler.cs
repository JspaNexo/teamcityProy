using System.IO;
using System.Text;
using System.Web;
using ApplicationDemo.NetFramework.Services;

namespace ApplicationDemo.NetFramework
{
    /// <summary>
    /// Punto de entrada de IIS (registrado en Web.config): pasa cada petición a Api y escribe su respuesta.
    /// </summary>
    public class ApiHandler : IHttpHandler
    {
        // Una sola instancia por App Pool: los productos viven en memoria, como en la API .NET 10
        private static readonly Api ApiDemo = new Api(new ProductoService());

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            string cuerpo;
            using (var lector = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                cuerpo = lector.ReadToEnd();
            }

            // "~/api/productos" -> "/api/productos", también si el sitio está en un directorio virtual
            var ruta = context.Request.AppRelativeCurrentExecutionFilePath.Substring(1);
            var respuesta = ApiDemo.Procesar(context.Request.HttpMethod, ruta, context.Request.QueryString, cuerpo);

            context.Response.StatusCode = respuesta.Estado;
            // Sin esto IIS reemplaza las respuestas 400/404 por su página de error
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(respuesta.Json);
        }
    }
}
