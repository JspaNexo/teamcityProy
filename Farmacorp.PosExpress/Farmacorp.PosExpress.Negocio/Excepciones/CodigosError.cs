namespace Farmacorp.PosExpress.Negocio.Excepciones;

/// <summary>
/// Códigos internos de error devueltos junto a cada <see cref="ReglaNegocioException"/>.
/// </summary>
public static class CodigosError
{
    public const string DATO_INVALIDO = "FC-400";
    public const string RECURSO_NO_ENCONTRADO = "FC-404";
    public const string REGISTRO_DUPLICADO = "FC-409";
    public const string CONFLICTO_CONCURRENCIA = "FC-412";
    public const string OPERACION_NO_PERMITIDA = "FC-422";
    public const string STOCK_INSUFICIENTE = "RN4-BASE";
    public const string STOCK_MINIMO_GANAMAX = "RN4-GANAMAX";
}
