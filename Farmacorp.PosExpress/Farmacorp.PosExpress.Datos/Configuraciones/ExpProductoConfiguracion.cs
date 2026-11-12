using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class ExpProductoConfiguracion : IEntityTypeConfiguration<ExpProducto>
{
    public void Configure(EntityTypeBuilder<ExpProducto> builder)
    {
        builder.ToTable("ExpProductos");

        builder.HasKey(producto => producto.IdProducto);

        builder.Property(producto => producto.IdProducto)
            .ValueGeneratedOnAdd();

        builder.Property(producto => producto.Nombre)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.NOMBRE_PRODUCTO);

        builder.Property(producto => producto.Precio)
            .IsRequired()
            .HasPrecision(ConfiguracionDecimales.PRECISION, ConfiguracionDecimales.ESCALA);

        builder.Property(producto => producto.Activo)
            .IsRequired();

        builder.Property(producto => producto.FechaVencimiento)
            .IsRequired()
            .HasColumnType(TiposColumna.FECHA_HORA);

        builder.Property(producto => producto.Observaciones)
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.OBSERVACIONES);

        builder.HasOne(producto => producto.TipoProducto)
            .WithMany(tipo => tipo.Productos)
            .HasForeignKey(producto => producto.IdTipoProducto)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
