using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class CodigoBarraConfiguracion : IEntityTypeConfiguration<CodigoBarra>
{
    public void Configure(EntityTypeBuilder<CodigoBarra> builder)
    {
        builder.ToTable("CodigosBarras");

        builder.HasKey(codigo => codigo.IdCodigoBarra);

        builder.Property(codigo => codigo.IdCodigoBarra)
            .ValueGeneratedOnAdd();

        builder.Property(codigo => codigo.UniqueCodigo)
            .IsRequired();

        builder.HasIndex(codigo => codigo.UniqueCodigo)
            .IsUnique();

        builder.Property(codigo => codigo.Activo)
            .IsRequired();

        builder.HasOne(codigo => codigo.ExpProducto)
            .WithMany(producto => producto.CodigosBarras)
            .HasForeignKey(codigo => codigo.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
