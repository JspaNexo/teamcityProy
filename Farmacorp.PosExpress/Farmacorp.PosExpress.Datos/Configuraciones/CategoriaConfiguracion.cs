using Farmacorp.PosExpress.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacorp.PosExpress.Datos.Configuraciones;

public class CategoriaConfiguracion : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(categoria => categoria.IdCategoria);

        builder.Property(categoria => categoria.IdCategoria)
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.ID_CATEGORIA);

        builder.Property(categoria => categoria.Descripcion)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.DESCRIPCION);

        builder.Property(categoria => categoria.Activo)
            .IsRequired();

        builder.Property(categoria => categoria.IdCategoriaPadre)
            .IsUnicode(false)
            .HasMaxLength(LongitudesMaximas.ID_CATEGORIA);

        builder.HasOne(categoria => categoria.CategoriaPadre)
            .WithMany(padre => padre.Subcategorias)
            .HasForeignKey(categoria => categoria.IdCategoriaPadre)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Categoria { IdCategoria = "MED", Descripcion = "Medicamentos", Activo = true },
            new Categoria { IdCategoria = "MED-ANALG", Descripcion = "Analgésicos", Activo = true, IdCategoriaPadre = "MED" },
            new Categoria { IdCategoria = "MED-AINE", Descripcion = "Antiinflamatorios", Activo = true, IdCategoriaPadre = "MED" },
            new Categoria { IdCategoria = "SUP", Descripcion = "Suplementos", Activo = true },
            new Categoria { IdCategoria = "SUP-VIT", Descripcion = "Vitaminas", Activo = true, IdCategoriaPadre = "SUP" },
            new Categoria { IdCategoria = "CP", Descripcion = "Cuidado personal", Activo = true },
            new Categoria { IdCategoria = "DESC", Descripcion = "Descontinuados", Activo = false });
    }
}
