namespace Farmacorp.PosExpress.Datos.Contratos;

/// <summary>
/// Otra operación modificó los datos entre su lectura y la confirmación de los cambios.
/// Aísla a las capas superiores de las excepciones propias de EF Core.
/// </summary>
public class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException(Exception excepcionInterna)
        : base(excepcionInterna.Message, excepcionInterna)
    {
    }
}
