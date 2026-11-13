namespace Farmacorp.PosExpress.Entidades;

/// <summary>
/// Precisión de los montos según el diseño físico: decimal(12,2).
/// Compartida por la configuración de EF (Datos) y las validaciones (Negocio).
/// </summary>
public static class ConfiguracionDecimales
{
    public const int PRECISION = 12;
    public const int ESCALA = 2;
}
