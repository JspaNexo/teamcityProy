namespace Farmacorp.PosExpress.Entidades;

public class CodigoBarra
{
    public long IdCodigoBarra { get; set; }

    public Guid UniqueCodigo { get; set; }

    public bool Activo { get; set; }

    public long IdProducto { get; set; }

    public ExpProducto? ExpProducto { get; set; }
}
