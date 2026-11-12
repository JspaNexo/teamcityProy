namespace Farmacorp.PosExpress.Pruebas.Infraestructura;

/// <summary>
/// Proveedor de tiempo con fecha fija para pruebas deterministas.
/// </summary>
public class TiempoFijo : TimeProvider
{
    private readonly DateTimeOffset _fechaActual;

    public TiempoFijo(DateTime fechaActual)
    {
        _fechaActual = new DateTimeOffset(fechaActual, TimeSpan.Zero);
    }

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow()
    {
        return _fechaActual;
    }
}
