using Postgrest.Models;
using Postgrest.Attributes;

public class intentos : BaseModel
{
    [Column("id"), PrimaryKey]
    public int id { get; set; }

    [Column("id_usuario")]
    public int id_usuario { get; set; }

    [Column("puntaje")]
    public int puntaje { get; set; }

    [Column("categoria")]
    public int categoria { get; set; }
}

