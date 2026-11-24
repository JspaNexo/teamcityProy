using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationDemo.NetFramework.Services
{
    public class Producto
    {
        public Producto(int id, string nombre, decimal precio)
        {
            Id = id;
            Nombre = nombre;
            Precio = precio;
        }

        public int Id { get; }
        public string Nombre { get; }
        public decimal Precio { get; }
    }

    public class CrearProductoRequest
    {
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
    }

    /// <summary>
    /// La misma lógica que ApplicationDemo/Services/ProductoService.cs (.NET 10), en C# 7.3: el de .NET Framework 4.8.
    /// </summary>
    public class ProductoService
    {
        private readonly List<Producto> _productos = new List<Producto>
        {
            new Producto(1, "Paracetamol 500mg", 12.50m),
            new Producto(2, "Ibuprofeno 400mg", 18.00m),
            new Producto(3, "Vitamina C 1g", 25.90m)
        };

        private readonly object _lock = new object();

        public IReadOnlyList<Producto> ObtenerTodos()
        {
            lock (_lock) return _productos.ToList();
        }

        public Producto ObtenerPorId(int id)
        {
            lock (_lock) return _productos.FirstOrDefault(p => p.Id == id);
        }

        public Producto Crear(CrearProductoRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Nombre)) throw new ArgumentException("El nombre es obligatorio.");
            if (request.Precio <= 0) throw new ArgumentException("El precio debe ser mayor a cero.");

            lock (_lock)
            {
                var id = _productos.Count == 0 ? 1 : _productos.Max(p => p.Id) + 1;
                var producto = new Producto(id, request.Nombre.Trim(), request.Precio);
                _productos.Add(producto);
                return producto;
            }
        }

        public static decimal CalcularTotal(decimal precio, int cantidad, decimal descuentoPorcentaje = 0)
        {
            if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
            if (descuentoPorcentaje < 0 || descuentoPorcentaje > 100) throw new ArgumentOutOfRangeException(nameof(descuentoPorcentaje));

            var subtotal = precio * cantidad;
            return Math.Round(subtotal * (1 - descuentoPorcentaje / 100m), 2);
        }
    }
}
