using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class ErpProductoConfiguracion : IEntityTypeConfiguration<ErpProducto>
{
    public void Configure(EntityTypeBuilder<ErpProducto> builder)
    {
        builder.ToTable("ErpProductos");

        // PK y FK hacia ExpProductos (relación 1 a 1)
        builder.HasKey(erp => erp.IdProducto);

        builder.Property(erp => erp.IdProducto)
            .ValueGeneratedNever();

        builder.Property(erp => erp.Costo)
            .IsRequired()
            .HasPrecision(ConfiguracionDecimales.PRECISION, ConfiguracionDecimales.ESCALA);

        builder.Property(erp => erp.UniqueCodigo)
            .IsRequired();

        builder.HasIndex(erp => erp.UniqueCodigo)
            .IsUnique();

        builder.Property(erp => erp.FechaRegistro)
            .IsRequired()
            .HasColumnType(TiposColumna.FECHA_HORA);

        // Concurrencia optimista: el UPDATE incluye el stock leído en el WHERE, así dos ventas
        // simultáneas del mismo producto no pueden pisarse ni vender stock inexistente
        builder.Property(erp => erp.Stock)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasOne(erp => erp.ExpProducto)
            .WithOne(producto => producto.ErpProducto)
            .HasForeignKey<ErpProducto>(erp => erp.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
