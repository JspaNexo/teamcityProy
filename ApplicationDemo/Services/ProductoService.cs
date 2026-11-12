namespace ApplicationDemo.Services;

public record Producto(int Id, string Nombre, decimal Precio);

public record CrearProductoRequest(string Nombre, decimal Precio);

public class ProductoService
{
    private readonly List<Producto> _productos =
    [
        new(1, "Paracetamol 500mg", 12.50m),
        new(2, "Ibuprofeno 400mg", 18.00m),
        new(3, "Vitamina C 1g", 25.90m)
    ];

    private readonly Lock _lock = new();

    public IReadOnlyList<Producto> ObtenerTodos()
    {
        lock (_lock) return _productos.ToList();
    }

    public Producto? ObtenerPorId(int id)
    {
        lock (_lock) return _productos.FirstOrDefault(p => p.Id == id);
    }

    public Producto Crear(CrearProductoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(request));
        if (request.Precio <= 0)
            throw new ArgumentException("El precio debe ser mayor a cero.", nameof(request));

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
        if (descuentoPorcentaje is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(descuentoPorcentaje));

        var subtotal = precio * cantidad;
        return Math.Round(subtotal * (1 - descuentoPorcentaje / 100m), 2);
    }
}
