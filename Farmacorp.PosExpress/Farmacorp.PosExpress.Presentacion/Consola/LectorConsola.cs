using System.Globalization;
using Farmacorp.PosExpress.Presentacion.Recursos;

namespace Farmacorp.PosExpress.Presentacion.Consola;

/// <summary>
/// Utilidades de lectura y validación de datos ingresados por consola.
/// </summary>
public static class LectorConsola
{
    public const string FORMATO_FECHA = "dd/MM/yyyy";

    private const char SEPARADOR_DECIMAL_COMA = ',';
    private const char SEPARADOR_DECIMAL_PUNTO = '.';

    public static string LeerTexto(string mensaje)
    {
        while (true)
        {
            var texto = LeerLinea(mensaje);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                return texto.Trim();
            }

            EscritorConsola.EscribirAdvertencia(Textos.ValorObligatorio);
        }
    }

    public static string? LeerTextoOpcional(string mensaje)
    {
        var texto = LeerLinea(mensaje);

        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    public static int LeerEntero(string mensaje)
    {
        return LeerValor(mensaje, texto => (int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor), valor));
    }

    public static long LeerEnteroLargo(string mensaje)
    {
        return LeerValor(mensaje, texto => (long.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor), valor));
    }

    public static decimal LeerDecimal(string mensaje)
    {
        // Se acepta coma o punto como separador decimal
        return LeerValor(mensaje, texto => (decimal.TryParse(
            texto.Replace(SEPARADOR_DECIMAL_COMA, SEPARADOR_DECIMAL_PUNTO),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var valor), valor));
    }

    public static DateTime LeerFecha(string mensaje)
    {
        return LeerValor(mensaje, texto => (DateTime.TryParseExact(
            texto,
            FORMATO_FECHA,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var valor), valor));
    }

    private static T LeerValor<T>(string mensaje, Func<string, (bool EsValido, T Valor)> convertir)
    {
        while (true)
        {
            var (esValido, valor) = convertir(LeerTexto(mensaje));

            if (esValido)
            {
                return valor;
            }

            EscritorConsola.EscribirAdvertencia(Textos.ValorInvalido);
        }
    }

    private static string LeerLinea(string mensaje)
    {
        Console.Write(mensaje);

        // Fin de la entrada estándar (por ejemplo, entrada redirigida desde un archivo)
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }
}
