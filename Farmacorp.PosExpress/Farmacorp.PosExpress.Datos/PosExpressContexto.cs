using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Farmacorp.PosExpress.Datos;

public class PosExpressContexto : DbContext
{
    public PosExpressContexto(DbContextOptions<PosExpressContexto> opciones)
        : base(opciones)
    {
    }

    public DbSet<TipoProducto> TiposProducto => Set<TipoProducto>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<ExpProducto> ExpProductos => Set<ExpProducto>();

    public DbSet<ProductoCategoria> ProductosCategorias => Set<ProductoCategoria>();

    public DbSet<VentaExpress> VentasExpress => Set<VentaExpress>();

    public DbSet<ErpProducto> ErpProductos => Set<ErpProducto>();

    public DbSet<CodigoBarra> CodigosBarras => Set<CodigoBarra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosExpressContexto).Assembly);
    }
}
