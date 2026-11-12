using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class ProductoCategoriaConfiguracion : IEntityTypeConfiguration<ProductoCategoria>
{
    public void Configure(EntityTypeBuilder<ProductoCategoria> builder)
    {
        builder.ToTable("ProductosCategorias");

        builder.HasKey(detalle => detalle.IdDetalle);

        builder.Property(detalle => detalle.IdDetalle)
            .ValueGeneratedOnAdd();

        builder.Property(detalle => detalle.FechaCreacion)
            .IsRequired()
            .HasColumnType(TiposColumna.FECHA_HORA);

        builder.Property(detalle => detalle.IdCategoria)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.ID_CATEGORIA);

        // Un producto no puede tener la misma categoría asignada dos veces
        builder.HasIndex(detalle => new { detalle.IdProducto, detalle.IdCategoria })
            .IsUnique();

        builder.HasOne(detalle => detalle.ExpProducto)
            .WithMany(producto => producto.ProductosCategorias)
            .HasForeignKey(detalle => detalle.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(detalle => detalle.Categoria)
            .WithMany(categoria => categoria.ProductosCategorias)
            .HasForeignKey(detalle => detalle.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
