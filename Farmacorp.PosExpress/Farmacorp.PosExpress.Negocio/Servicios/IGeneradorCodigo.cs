namespace Farmacorp.PosExpress.Negocio.Servicios;

/// <summary>
/// Genera los valores únicos (UniqueCodigo) de productos y códigos de barras.
/// </summary>
public interface IGeneradorCodigo
{
    Guid GenerarUniqueCodigo();
}
