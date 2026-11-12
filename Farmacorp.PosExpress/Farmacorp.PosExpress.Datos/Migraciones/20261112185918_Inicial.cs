using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Farmacorp.PosExpress.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    IdCategoria = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IdCategoriaPadre = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.IdCategoria);
                    table.ForeignKey(
                        name: "FK_Categorias_Categorias_IdCategoriaPadre",
                        column: x => x.IdCategoriaPadre,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TiposProducto",
                columns: table => new
                {
                    IdTipoProducto = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposProducto", x => x.IdTipoProducto);
                });

            migrationBuilder.CreateTable(
                name: "ExpProductos",
                columns: table => new
                {
                    IdProducto = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    IdTipoProducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpProductos", x => x.IdProducto);
                    table.ForeignKey(
                        name: "FK_ExpProductos_TiposProducto_IdTipoProducto",
                        column: x => x.IdTipoProducto,
                        principalTable: "TiposProducto",
                        principalColumn: "IdTipoProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodigosBarras",
                columns: table => new
                {
                    IdCodigoBarra = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueCodigo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IdProducto = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosBarras", x => x.IdCodigoBarra);
                    table.ForeignKey(
                        name: "FK_CodigosBarras_ExpProductos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "ExpProductos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ErpProductos",
                columns: table => new
                {
                    IdProducto = table.Column<long>(type: "bigint", nullable: false),
                    Costo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    UniqueCodigo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpProductos", x => x.IdProducto);
                    table.ForeignKey(
                        name: "FK_ErpProductos_ExpProductos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "ExpProductos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductosCategorias",
                columns: table => new
                {
                    IdDetalle = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    IdProducto = table.Column<long>(type: "bigint", nullable: false),
                    IdCategoria = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosCategorias", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK_ProductosCategorias_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosCategorias_ExpProductos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "ExpProductos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentaExpress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    Cliente = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    Producto = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    UniqueProducto = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IdProducto = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaExpress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaExpress_ExpProductos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "ExpProductos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "IdCategoria", "Activo", "Descripcion", "IdCategoriaPadre" },
                values: new object[,]
                {
                    { "CP", true, "Cuidado personal", null },
                    { "DESC", false, "Descontinuados", null },
                    { "MED", true, "Medicamentos", null },
                    { "SUP", true, "Suplementos", null }
                });

            migrationBuilder.InsertData(
                table: "TiposProducto",
                columns: new[] { "IdTipoProducto", "Descripcion" },
                values: new object[,]
                {
                    { 1, "Medicamento" },
                    { 2, "Cuidado personal" },
                    { 3, "Suplemento nutricional" }
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "IdCategoria", "Activo", "Descripcion", "IdCategoriaPadre" },
                values: new object[,]
                {
                    { "MED-AINE", true, "Antiinflamatorios", "MED" },
                    { "MED-ANALG", true, "Analgésicos", "MED" },
                    { "SUP-VIT", true, "Vitaminas", "SUP" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_IdCategoriaPadre",
                table: "Categorias",
                column: "IdCategoriaPadre");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosBarras_IdProducto",
                table: "CodigosBarras",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosBarras_UniqueCodigo",
                table: "CodigosBarras",
                column: "UniqueCodigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpProductos_UniqueCodigo",
                table: "ErpProductos",
                column: "UniqueCodigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpProductos_IdTipoProducto",
                table: "ExpProductos",
                column: "IdTipoProducto");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosCategorias_IdCategoria",
                table: "ProductosCategorias",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosCategorias_IdProducto_IdCategoria",
                table: "ProductosCategorias",
                columns: new[] { "IdProducto", "IdCategoria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaExpress_IdProducto",
                table: "VentaExpress",
                column: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigosBarras");

            migrationBuilder.DropTable(
                name: "ErpProductos");

            migrationBuilder.DropTable(
                name: "ProductosCategorias");

            migrationBuilder.DropTable(
                name: "VentaExpress");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "ExpProductos");

            migrationBuilder.DropTable(
                name: "TiposProducto");
        }
    }
}
