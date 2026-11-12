using Farmacorp.PosExpress.Negocio.Estrategias;
using Farmacorp.PosExpress.Pruebas.Infraestructura;
using Microsoft.Extensions.Options;

namespace Farmacorp.PosExpress.Pruebas.Estrategias;

public class SelectorReglasNegocioTests
{
    private static readonly DateTime FechaActual = new(2026, 12, 15);

    [Fact]
    public void ObtenerReglasVigentes_ConfiguracionBase_DevuelveBase()
    {
        var selector = CrearSelector(new ConfiguracionEstrategia { Tipo = TiposEstrategia.Base });

        Assert.Equal(TiposEstrategia.Base, selector.ObtenerReglasVigentes().Estrategia);
    }

    [Fact]
    public void ObtenerReglasVigentes_GanaMaxDentroDeVigencia_DevuelveGanaMax()
    {
        var selector = CrearSelector(new ConfiguracionEstrategia
        {
            Tipo = TiposEstrategia.GanaMax,
            FechaInicioGanaMax = new DateOnly(2026, 11, 1),
            FechaFinGanaMax = new DateOnly(2027, 1, 31)
        });

        Assert.Equal(TiposEstrategia.GanaMax, selector.ObtenerReglasVigentes().Estrategia);
    }

    [Fact]
    public void ObtenerReglasVigentes_GanaMaxSinFechas_DevuelveGanaMax()
    {
        var selector = CrearSelector(new ConfiguracionEstrategia { Tipo = TiposEstrategia.GanaMax });

        Assert.Equal(TiposEstrategia.GanaMax, selector.ObtenerReglasVigentes().Estrategia);
    }

    [Fact]
    public void ObtenerReglasVigentes_GanaMaxVencida_VuelveABase()
    {
        var selector = CrearSelector(new ConfiguracionEstrategia
        {
            Tipo = TiposEstrategia.GanaMax,
            FechaInicioGanaMax = new DateOnly(2026, 8, 1),
            FechaFinGanaMax = new DateOnly(2026, 10, 31)
        });

        Assert.Equal(TiposEstrategia.Base, selector.ObtenerReglasVigentes().Estrategia);
    }

    [Fact]
    public void CambiarEstrategia_PermiteVolverABaseEnCualquierMomento()
    {
        var selector = CrearSelector(new ConfiguracionEstrategia { Tipo = TiposEstrategia.GanaMax });

        selector.CambiarEstrategia(TiposEstrategia.Base);

        Assert.Equal(TiposEstrategia.Base, selector.EstrategiaSeleccionada);
        Assert.Equal(TiposEstrategia.Base, selector.ObtenerReglasVigentes().Estrategia);
    }

    private static SelectorReglasNegocio CrearSelector(ConfiguracionEstrategia configuracion)
    {
        return new SelectorReglasNegocio(
            new ReglasNegocioBase(),
            new ReglasNegocioGanaMax(),
            Options.Create(configuracion),
            new TiempoFijo(FechaActual));
    }
}
