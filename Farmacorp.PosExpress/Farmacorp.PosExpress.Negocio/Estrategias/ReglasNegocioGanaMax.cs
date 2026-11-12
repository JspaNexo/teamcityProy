using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Recursos;

namespace Farmacorp.PosExpress.Negocio.Estrategias;

/// <summary>
/// Reglas de la estrategia GanaMax. Hereda del negocio Base:
/// las reglas no sobrescritas se mantienen como en Base.
/// </summary>
public class ReglasNegocioGanaMax : ReglasNegocioBase
{
    private const decimal MARGEN_GANANCIA = 0.80m;
    private const decimal PORCENTAJE_DESCUENTO = 0.10m;
    private const int STOCK_MINIMO_RESTANTE = 10;

    public override TiposEstrategia Estrategia => TiposEstrategia.GanaMax;

    protected override decimal MargenGanancia => MARGEN_GANANCIA;

    protected override decimal PorcentajeDescuento => PORCENTAJE_DESCUENTO;

    public override void ValidarStock(string nombreProducto, int stockDisponible, int cantidadSolicitada)
    {
        base.ValidarStock(nombreProducto, stockDisponible, cantidadSolicitada);

        var stockRestante = stockDisponible - cantidadSolicitada;

        if (stockRestante <= STOCK_MINIMO_RESTANTE)
        {
            throw new ReglaNegocioException(
                CodigosError.STOCK_MINIMO_GANAMAX,
                Mensajes.StockMinimoGanaMax,
                nombreProducto,
                stockRestante,
                STOCK_MINIMO_RESTANTE);
        }
    }
}
