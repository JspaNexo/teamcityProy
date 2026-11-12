using Farmacorp.PosExpress.Datos.Contratos;
using Farmacorp.PosExpress.Entidades;
using Farmacorp.PosExpress.Negocio.Modelos;

namespace Farmacorp.PosExpress.Negocio.Servicios;

public class CodigoBarraServicio : ICodigoBarraServicio
{
    private readonly IUnitOfWork _unidadTrabajo;
    private readonly IGeneradorCodigo _generadorCodigo;

    public CodigoBarraServicio(IUnitOfWork unidadTrabajo, IGeneradorCodigo generadorCodigo)
    {
        _unidadTrabajo = unidadTrabajo;
        _generadorCodigo = generadorCodigo;
    }

    public async Task<CodigoBarraAsignado> AsignarAsync(long idProducto, CancellationToken cancelacion = default)
    {
        var producto = await _unidadTrabajo.ExpProductos.ObtenerPorIdAsync(idProducto, cancelacion);
        ValidadorProducto.ValidarExistenteYActivo(producto, idProducto);

        var codigoBarra = new CodigoBarra
        {
            UniqueCodigo = _generadorCodigo.GenerarUniqueCodigo(),
            Activo = true,
            IdProducto = idProducto
        };

        await _unidadTrabajo.CodigosBarras.AgregarAsync(codigoBarra, cancelacion);
        await _unidadTrabajo.GuardarCambiosAsync(cancelacion);

        return new CodigoBarraAsignado(codigoBarra.IdCodigoBarra, idProducto, codigoBarra.UniqueCodigo);
    }
}
