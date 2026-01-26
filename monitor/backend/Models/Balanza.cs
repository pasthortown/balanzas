using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BalanzasMonitor.Models;

public class Balanza
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("ip")]
    public string Ip { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("ultimaConexion")]
    public DateTime? UltimaConexion { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = "error";
}

public class BalanzaCreateDto
{
    public string Ip { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
