namespace Farmacorp.PosExpress.Negocio.Estrategias;

/// <summary>
/// Configuración de la estrategia comercial (sección "Estrategia" del appsettings.json).
/// </summary>
public class ConfiguracionEstrategia
{
    public const string SECCION = "Estrategia";

    public TiposEstrategia Tipo { get; set; } = TiposEstrategia.Base;

    /// <summary>Inicio de la vigencia de GanaMax (inclusive). Sin valor = sin límite.</summary>
    public DateOnly? FechaInicioGanaMax { get; set; }

    /// <summary>Fin de la vigencia de GanaMax (inclusive). Sin valor = sin límite.</summary>
    public DateOnly? FechaFinGanaMax { get; set; }
}
