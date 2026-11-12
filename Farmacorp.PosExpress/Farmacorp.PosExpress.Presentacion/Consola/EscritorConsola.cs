using System.Globalization;
using Farmacorp.PosExpress.Presentacion.Recursos;

namespace Farmacorp.PosExpress.Presentacion.Consola;

/// <summary>
/// Utilidades de salida por consola.
/// </summary>
public static class EscritorConsola
{
    public static void EscribirTitulo(string titulo)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(Textos.Separador);
        Console.WriteLine(titulo);
        Console.WriteLine(Textos.Separador);
        Console.ResetColor();
    }

    public static void EscribirLinea(string formato, params object?[] argumentos)
    {
        Console.WriteLine(Formatear(formato, argumentos));
    }

    public static void EscribirExito(string formato, params object?[] argumentos)
    {
        EscribirConColor(ConsoleColor.Green, formato, argumentos);
    }

    public static void EscribirAdvertencia(string formato, params object?[] argumentos)
    {
        EscribirConColor(ConsoleColor.Yellow, formato, argumentos);
    }

    public static void EscribirError(string formato, params object?[] argumentos)
    {
        EscribirConColor(ConsoleColor.Red, formato, argumentos);
    }

    public static void Pausar()
    {
        // Con entrada redirigida (scripts / CI) no se espera una tecla
        if (Console.IsInputRedirected)
        {
            return;
        }

        Console.WriteLine();
        Console.Write(Textos.PresioneTecla);
        Console.ReadKey(intercept: true);
        Console.WriteLine();
    }

    private static void EscribirConColor(ConsoleColor color, string formato, object?[] argumentos)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(Formatear(formato, argumentos));
        Console.ResetColor();
    }

    private static string Formatear(string formato, object?[] argumentos)
    {
        return argumentos.Length == 0 ? formato : string.Format(CultureInfo.CurrentCulture, formato, argumentos);
    }
}
