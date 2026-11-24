using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using ApplicationDemo.NetFramework.Services;
using Xunit;

namespace ApplicationDemo.NetFramework.Tests
{
    /// <summary>Las rutas de la API, sin IIS: Api recibe método, ruta, query y cuerpo como los pasa ApiHandler.</summary>
    public class ApiTests
    {
        private readonly Api _api = new Api(new ProductoService());

        private static T Leer<T>(Respuesta respuesta) => new JavaScriptSerializer().Deserialize<T>(respuesta.Json);

        private static Dictionary<string, object> LeerObjeto(Respuesta respuesta) => Leer<Dictionary<string, object>>(respuesta);

        private static NameValueCollection Query(string precio, string cantidad, string descuento = null) =>
            new NameValueCollection { { "precio", precio }, { "cantidad", cantidad }, { "descuento", descuento } };

        [Fact]
        public void Raiz_IndicaQueCorreEnNetFramework()
        {
            var respuesta = _api.Procesar("GET", "/", null, "");

            Assert.Equal(200, respuesta.Estado);
            Assert.Equal(".NET Framework 4.8", LeerObjeto(respuesta)["framework"]);
        }

        [Fact]
        public void Health_DevuelveHealthy()
        {
            var respuesta = _api.Procesar("GET", "/health", null, "");

            Assert.Equal(200, respuesta.Estado);
            Assert.Equal("Healthy", LeerObjeto(respuesta)["status"]);
        }

        [Fact]
        public void GetProductos_DevuelveLosTres()
        {
            var respuesta = _api.Procesar("GET", "/api/productos/", null, "");

            Assert.Equal(200, respuesta.Estado);
            Assert.Equal(3, Leer<List<Dictionary<string, object>>>(respuesta).Count);
        }

        [Fact]
        public void GetProducto_PorId()
        {
            var respuesta = _api.Procesar("GET", "/api/productos/1", null, "");

            Assert.Equal(200, respuesta.Estado);
            Assert.Equal("Paracetamol 500mg", LeerObjeto(respuesta)["nombre"]);
        }

        [Fact]
        public void GetProducto_Inexistente_Devuelve404()
        {
            Assert.Equal(404, _api.Procesar("GET", "/api/productos/99", null, "").Estado);
        }

        [Fact]
        public void PostProducto_Valido_Devuelve201ConId()
        {
            var respuesta = _api.Procesar("POST", "/api/productos", null, "{ \"nombre\": \"Aspirina\", \"precio\": 9.99 }");

            Assert.Equal(201, respuesta.Estado);
            Assert.Equal(4, Convert.ToInt32(LeerObjeto(respuesta)["id"]));
            Assert.Equal(200, _api.Procesar("GET", "/api/productos/4", null, "").Estado);
        }

        [Theory]
        [InlineData("{ \"nombre\": \"\", \"precio\": 10 }")]
        [InlineData("{ \"nombre\": \"Jarabe\", \"precio\": 0 }")]
        [InlineData("no es json")]
        [InlineData("")]
        public void PostProducto_Invalido_Devuelve400(string cuerpo)
        {
            var respuesta = _api.Procesar("POST", "/api/productos", null, cuerpo);

            Assert.Equal(400, respuesta.Estado);
            Assert.True(LeerObjeto(respuesta).ContainsKey("error"));
        }

        [Fact]
        public void Total_AplicaDescuento()
        {
            var respuesta = _api.Procesar("GET", "/api/total", Query("10", "3", "10"), "");

            Assert.Equal(200, respuesta.Estado);
            Assert.Equal(27m, Convert.ToDecimal(LeerObjeto(respuesta)["total"]));
        }

        [Theory]
        [InlineData("10", "0")]
        [InlineData("abc", "3")]
        [InlineData(null, "3")]
        public void Total_ConParametrosInvalidos_Devuelve400(string precio, string cantidad)
        {
            Assert.Equal(400, _api.Procesar("GET", "/api/total", Query(precio, cantidad), "").Estado);
        }

        [Fact]
        public void RutaDesconocida_Devuelve404()
        {
            Assert.Equal(404, _api.Procesar("GET", "/no-existe", null, "").Estado);
        }

        [Fact]
        public void MetodoNoSoportado_Devuelve405()
        {
            Assert.Equal(405, _api.Procesar("DELETE", "/api/productos", null, "").Estado);
        }
    }
}
