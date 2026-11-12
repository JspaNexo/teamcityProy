namespace Farmacorp.PosExpress.Negocio.Servicios;

public class GeneradorCodigo : IGeneradorCodigo
{
    public Guid GenerarUniqueCodigo()
    {
        // GUID v7: único y ordenado por tiempo (mejor para índices que un GUID aleatorio)
        return Guid.CreateVersion7();
    }
}
