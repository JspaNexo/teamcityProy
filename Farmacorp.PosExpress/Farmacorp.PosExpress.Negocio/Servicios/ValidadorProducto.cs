using Farmacorp.PosExpress.Entidades;
using Farmacorp.PosExpress.Negocio.Excepciones;
using Farmacorp.PosExpress.Negocio.Recursos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

/// <summary>
/// Validaciones comunes sobre productos compartidas por los servicios.
/// </summary>
internal static class ValidadorProducto
{
    public static ExpProducto ValidarExistenteYActivo(ExpProducto? producto, long idProducto)
    {
        if (producto is null)
        {
            throw new ReglaNegocioException(CodigosError.RECURSO_NO_ENCONTRADO, Mensajes.ProductoNoExiste, idProducto);
        }

        if (!producto.Activo)
        {
            throw new ReglaNegocioException(CodigosError.OPERACION_NO_PERMITIDA, Mensajes.ProductoInactivo, idProducto);
        }

        return producto;
    }

    public static void ValidarLongitud(string? valor, int longitudMaxima, string nombreCampo)
    {
        if (valor is not null && valor.Trim().Length > longitudMaxima)
        {
            throw new ReglaNegocioException(CodigosError.DATO_INVALIDO, Mensajes.LongitudExcedida, nombreCampo, longitudMaxima);
        }
    }

    public static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
