namespace Farmacorp.PosExpress.Negocio.Estrategias;

/// <summary>
/// Determina qué reglas de negocio aplicar en cada transacción.
/// </summary>
public interface ISelectorReglasNegocio
{
    TiposEstrategia EstrategiaSeleccionada { get; }

    DateOnly? FechaInicioGanaMax { get; }

    DateOnly? FechaFinGanaMax { get; }

    /// <summary>
    /// Devuelve las reglas GanaMax si están seleccionadas y dentro de su vigencia; caso contrario, las reglas Base.
    /// </summary>
    IReglasNegocio ObtenerReglasVigentes();

    /// <summary>
    /// Cambia la estrategia en tiempo de ejecución (permite volver a Base en cualquier momento).
    /// </summary>
    void CambiarEstrategia(TiposEstrategia estrategia);
}
