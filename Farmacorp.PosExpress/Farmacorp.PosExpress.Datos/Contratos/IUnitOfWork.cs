using Farmacorp.PosExpress.Entidades;

namespace Farmacorp.PosExpress.Datos.Contratos;

/// <summary>
/// Unidad de trabajo: agrupa los repositorios que comparten el mismo contexto
/// y confirma todos los cambios en una única transacción.
/// </summary>
public interface IUnitOfWork
{
    IRepositorio<TipoProducto> TiposProducto { get; }

    IRepositorio<Categoria> Categorias { get; }

    IExpProductoRepositorio ExpProductos { get; }

    IRepositorio<ErpProducto> ErpProductos { get; }

    IRepositorio<CodigoBarra> CodigosBarras { get; }

    IRepositorio<ProductoCategoria> ProductosCategorias { get; }

    IVentaExpressRepositorio VentasExpress { get; }

    /// <exception cref="ConflictoConcurrenciaException">
    /// Otra operación modificó un dato con control de concurrencia (por ejemplo, el stock) después de leerlo.
    /// </exception>
    Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default);
}
