using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Recursos;

namespace Farmacorp.PosExpress.Negocio.Estrategias;

/// <summary>
/// Reglas del negocio Base.
/// </summary>
public class ReglasNegocioBase : IReglasNegocio
{
    private const decimal MARGEN_GANANCIA = 0.50m;
    private const decimal PORCENTAJE_DESCUENTO = 0.30m;
    private const int CATEGORIAS_PARA_DESCUENTO = 1;
    private const int DECIMALES_MONTO = 2;

    public virtual TiposEstrategia Estrategia => TiposEstrategia.Base;

    protected virtual decimal MargenGanancia => MARGEN_GANANCIA;

    protected virtual decimal PorcentajeDescuento => PORCENTAJE_DESCUENTO;

    public decimal CalcularPrecio(decimal costo)
    {
        return Redondear(costo * (decimal.One + MargenGanancia));
    }

    public decimal CalcularDescuento(decimal subtotal, int cantidadCategorias)
    {
        var tieneCategoriaUnica = cantidadCategorias == CATEGORIAS_PARA_DESCUENTO;

        return tieneCategoriaUnica ? Redondear(subtotal * PorcentajeDescuento) : decimal.Zero;
    }

    public virtual void ValidarStock(string nombreProducto, int stockDisponible, int cantidadSolicitada)
    {
        if (cantidadSolicitada > stockDisponible)
        {
            throw new ReglaNegocioException(
                CodigosError.STOCK_INSUFICIENTE,
                Mensajes.StockInsuficiente,
                nombreProducto,
                stockDisponible,
                cantidadSolicitada);
        }
    }

    protected static decimal Redondear(decimal monto)
    {
        return Math.Round(monto, DECIMALES_MONTO, MidpointRounding.AwayFromZero);
    }
}
