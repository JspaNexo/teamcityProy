using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class TipoProductoConfiguracion : IEntityTypeConfiguration<TipoProducto>
{
    public void Configure(EntityTypeBuilder<TipoProducto> builder)
    {
        builder.ToTable("TiposProducto");

        builder.HasKey(tipo => tipo.IdTipoProducto);

        builder.Property(tipo => tipo.IdTipoProducto)
            .ValueGeneratedNever();

        builder.Property(tipo => tipo.Descripcion)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.DESCRIPCION);

        builder.HasData(
            new TipoProducto { IdTipoProducto = 1, Descripcion = "Medicamento" },
            new TipoProducto { IdTipoProducto = 2, Descripcion = "Cuidado personal" },
            new TipoProducto { IdTipoProducto = 3, Descripcion = "Suplemento nutricional" });
    }
}
