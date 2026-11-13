using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Datos.Repositorios;
using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Datos;

public class UnitOfWork : IUnitOfWork
{
    private readonly PosExpressContexto _contexto;

    private IRepositorio<TipoProducto>? _tiposProducto;
    private IRepositorio<Categoria>? _categorias;
    private IExpProductoRepositorio? _expProductos;
    private IRepositorio<ErpProducto>? _erpProductos;
    private IRepositorio<CodigoBarra>? _codigosBarras;
    private IRepositorio<ProductoCategoria>? _productosCategorias;
    private IVentaExpressRepositorio? _ventasExpress;

    public UnitOfWork(PosExpressContexto contexto)
    {
        _contexto = contexto;
    }

    public IRepositorio<TipoProducto> TiposProducto =>
        _tiposProducto ??= new Repositorio<TipoProducto>(_contexto);

    public IRepositorio<Categoria> Categorias =>
        _categorias ??= new Repositorio<Categoria>(_contexto);

    public IExpProductoRepositorio ExpProductos =>
        _expProductos ??= new ExpProductoRepositorio(_contexto);

    public IRepositorio<ErpProducto> ErpProductos =>
        _erpProductos ??= new Repositorio<ErpProducto>(_contexto);

    public IRepositorio<CodigoBarra> CodigosBarras =>
        _codigosBarras ??= new Repositorio<CodigoBarra>(_contexto);

    public IRepositorio<ProductoCategoria> ProductosCategorias =>
        _productosCategorias ??= new Repositorio<ProductoCategoria>(_contexto);

    public IVentaExpressRepositorio VentasExpress =>
        _ventasExpress ??= new VentaExpressRepositorio(_contexto);

    public async Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default)
    {
        try
        {
            // SaveChanges confirma todas las operaciones pendientes en una sola transacción
            return await _contexto.SaveChangesAsync(cancelacion);
        }
        catch (DbUpdateConcurrencyException excepcion)
        {
            throw new ConflictoConcurrenciaException(excepcion);
        }
    }
}
