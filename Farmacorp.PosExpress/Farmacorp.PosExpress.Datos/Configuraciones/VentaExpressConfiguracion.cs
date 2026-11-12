using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class VentaExpressConfiguracion : IEntityTypeConfiguration<VentaExpress>
{
    public void Configure(EntityTypeBuilder<VentaExpress> builder)
    {
        builder.ToTable("VentaExpress");

        builder.HasKey(venta => venta.Id);

        builder.Property(venta => venta.Id)
            .ValueGeneratedOnAdd();

        builder.Property(venta => venta.Fecha)
            .IsRequired()
            .HasColumnType(TiposColumna.FECHA_HORA);

        builder.Property(venta => venta.Cliente)
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.CLIENTE);

        builder.Property(venta => venta.Producto)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.NOMBRE_PRODUCTO);

        builder.Property(venta => venta.UniqueProducto);

        builder.Property(venta => venta.Cantidad)
            .IsRequired();

        builder.Property(venta => venta.Precio)
            .IsRequired()
            .HasPrecision(ConfiguracionDecimales.PRECISION, ConfiguracionDecimales.ESCALA);

        builder.Property(venta => venta.Descuento)
            .HasPrecision(ConfiguracionDecimales.PRECISION, ConfiguracionDecimales.ESCALA);

        builder.Property(venta => venta.Total)
            .IsRequired()
            .HasPrecision(ConfiguracionDecimales.PRECISION, ConfiguracionDecimales.ESCALA);

        builder.HasOne(venta => venta.ExpProducto)
            .WithMany(producto => producto.Ventas)
            .HasForeignKey(venta => venta.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
