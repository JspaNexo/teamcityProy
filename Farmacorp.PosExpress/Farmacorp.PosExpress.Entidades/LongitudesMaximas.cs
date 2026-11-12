namespace Farmacorp.PosExpress.Entidades;

/// <summary>
/// Longitudes máximas de los campos de texto según el diseño físico.
/// Compartidas por la configuración de EF (Datos) y las validaciones (Negocio).
/// </summary>
public static class LongitudesMaximas
{
    public const int DESCRIPCION = 300;
    public const int NOMBRE_PRODUCTO = 300;
    public const int OBSERVACIONES = 10;
    public const int ID_CATEGORIA = 30;
    public const int CLIENTE = 300;
}
