using System.Globalization;

namespace Farmacorp.PosExpress.Negocio.Excepciones;

/// <summary>
/// Error controlado producido al incumplir una validación o regla de negocio.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string codigo, string formatoMensaje, params object[] argumentos)
        : base(string.Format(CultureInfo.CurrentCulture, formatoMensaje, argumentos))
    {
        Codigo = codigo;
    }

    public string Codigo { get; }
}
