using System.Runtime.Serialization;

namespace RestAPI.DTO;

[DataContract]
public class DocumentUpdateDTO
{
    public string? Title { get; set; }
}