# Farmacorp POS Express (Práctica 06)

Solución .NET 10 / C# con arquitectura N-Capas que implementa las transacciones del sistema POS Express
(módulos Express y ERP) con las reglas de negocio **Base** y la estrategia **GanaMax**.

## Arquitectura

```
Presentacion (Consola)  ──►  Negocio  ──►  Datos  ──►  Entidades
      │                                      ▲
      └──────── composición IoC / DI ────────┘
```

| Proyecto | Responsabilidad |
|---|---|
| `Farmacorp.PosExpress.Entidades` | Entidades del modelo (ExpProducto, ErpProducto, CodigoBarra, Categoria, ProductoCategoria, TipoProducto, VentaExpress). |
| `Farmacorp.PosExpress.Datos` | EF Core Code First, configuraciones Fluent API (`Configuraciones/`), repositorios (`Repositorios/`), `UnitOfWork`, contratos (`Contratos/`) y migraciones (`Migraciones/`). |
| `Farmacorp.PosExpress.Negocio` | Servicios por transacción (`Servicios/`), reglas de negocio como estrategias (`Estrategias/`), excepciones y mensajes (`Recursos/Mensajes.resx`). |
| `Farmacorp.PosExpress.Presentacion` | Aplicación de consola: menú, lectura de datos, composición de dependencias (`Program.cs`) y textos (`Recursos/Textos.resx`). |
| `Farmacorp.PosExpress.Pruebas` | Pruebas xUnit de reglas y servicios (EF InMemory). |

Conceptos aplicados:

- **Repositories**: `IRepositorio<T>` genérico y repositorios específicos (`IExpProductoRepositorio`, `IVentaExpressRepositorio`).
- **UnitOfWork**: `IUnitOfWork` agrupa los repositorios sobre un mismo `DbContext` y confirma todo en una transacción (`GuardarCambiosAsync`).
- **IoC / DI**: cada capa registra sus servicios (`AgregarCapaDatos`, `AgregarCapaNegocio`); la consola compone el contenedor con `Microsoft.Extensions.Hosting`. Los servicios dependen de interfaces, no de implementaciones.
- **EF Code First + Fluent API + SQL Server Express LocalDB**: el esquema se define con `IEntityTypeConfiguration<T>` según el diseño físico (varchar, decimal(12,2), datetime, bigint) y se crea con migraciones.

## Transacciones y reglas

| Transacción | Regla | Base | GanaMax |
|---|---|---|---|
| 1. Registro nuevo producto ERP | UniqueCodigo autogenerado | GUID v7 | igual |
| | RN1: precio = costo + margen (costo con máximo 2 decimales) | 50% | **80%** |
| 2. Asignación de código de barras | UniqueCodigo autogenerado | GUID v7 | igual |
| 3. Asignación de categorías | Sin duplicados, solo categorías activas | ✓ | igual |
| 4. Registro de venta (un producto) | RN2: guarda UniqueCodigo del producto en `UniqueProducto` | ✓ | igual |
| | RN3: descuento si y solo si tiene una única categoría | 30% | **10%** |
| | RN4: stock disponible | stock ≥ cantidad | **y stock restante > 10** |
| | RN5: descuenta el stock en ErpProductos (misma transacción, con concurrencia optimista sobre `Stock`) | ✓ | igual |

`ReglasNegocioGanaMax` hereda de `ReglasNegocioBase` y solo sobrescribe lo que cambia, así que las reglas
no especificadas se mantienen como en Base.

### Activar / desactivar GanaMax

En `Farmacorp.PosExpress.Presentacion/appsettings.json`:

```json
"Estrategia": {
  "Tipo": "GanaMax",
  "FechaInicioGanaMax": "2026-11-01",
  "FechaFinGanaMax": "2027-01-31"
}
```

- `Tipo`: `Base` o `GanaMax`.
- GanaMax solo se aplica dentro de la vigencia (3 meses). Fuera de ella se usan las reglas Base automáticamente.
- Desde el menú (opción 7) se puede volver a Base, o cambiar a GanaMax, en cualquier momento sin reiniciar.

## Ejecución

Requisitos: .NET SDK 10 y SQL Server Express LocalDB (`(localdb)\MSSQLLocalDB`).

```bash
dotnet run --project Farmacorp.PosExpress.Presentacion   # crea la BD FarmacorpPosExpress y aplica migraciones
dotnet test Farmacorp.PosExpress.slnx                    # pruebas
```

Al iniciar se aplican las migraciones y se cargan datos semilla de tipos de producto y categorías.

Nueva migración tras cambiar el modelo:

```bash
dotnet ef migrations add NombreMigracion --project Farmacorp.PosExpress.Datos --output-dir Migraciones --namespace Farmacorp.PosExpress.Datos.Migraciones
```

## Estándar de codificación aplicado

- Identificadores en español, PascalCase para tipos, métodos y propiedades, camelCase para parámetros y variables, `_camelCase` para campos privados.
- Constantes en MAYÚSCULAS (`MARGEN_GANANCIA`, `STOCK_MINIMO_RESTANTE`), sin números mágicos.
- Interfaces con prefijo `I`, excepciones con sufijo `Exception`, enumeradores sin sufijo (`TiposEstrategia`).
- Una clase por archivo, modificadores de acceso explícitos.
- Textos de usuario y mensajes de error en archivos de recursos (`.resx`).
