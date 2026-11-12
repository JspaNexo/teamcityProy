using Microsoft.Extensions.Options;

namespace Farmacorp.PosExpress.Negocio.Estrategias;

public class SelectorReglasNegocio : ISelectorReglasNegocio
{
    private readonly ReglasNegocioBase _reglasBase;
    private readonly ReglasNegocioGanaMax _reglasGanaMax;
    private readonly TimeProvider _proveedorTiempo;
    private readonly Lock _bloqueo = new();
    private TiposEstrategia _estrategiaSeleccionada;

    public SelectorReglasNegocio(
        ReglasNegocioBase reglasBase,
        ReglasNegocioGanaMax reglasGanaMax,
        IOptions<ConfiguracionEstrategia> opciones,
        TimeProvider proveedorTiempo)
    {
        _reglasBase = reglasBase;
        _reglasGanaMax = reglasGanaMax;
        _proveedorTiempo = proveedorTiempo;
        _estrategiaSeleccionada = opciones.Value.Tipo;
        FechaInicioGanaMax = opciones.Value.FechaInicioGanaMax;
        FechaFinGanaMax = opciones.Value.FechaFinGanaMax;
    }

    public TiposEstrategia EstrategiaSeleccionada
    {
        get
        {
            lock (_bloqueo)
            {
                return _estrategiaSeleccionada;
            }
        }
    }

    public DateOnly? FechaInicioGanaMax { get; }

    public DateOnly? FechaFinGanaMax { get; }

    public IReglasNegocio ObtenerReglasVigentes()
    {
        var esGanaMaxVigente = EstrategiaSeleccionada == TiposEstrategia.GanaMax && EsFechaDentroVigenciaGanaMax();

        return esGanaMaxVigente ? _reglasGanaMax : _reglasBase;
    }

    public void CambiarEstrategia(TiposEstrategia estrategia)
    {
        lock (_bloqueo)
        {
            _estrategiaSeleccionada = estrategia;
        }
    }

    private bool EsFechaDentroVigenciaGanaMax()
    {
        var fechaActual = DateOnly.FromDateTime(_proveedorTiempo.GetLocalNow().DateTime);
        var iniciada = FechaInicioGanaMax is null || fechaActual >= FechaInicioGanaMax;
        var noFinalizada = FechaFinGanaMax is null || fechaActual <= FechaFinGanaMax;

        return iniciada && noFinalizada;
    }
}
