using System.Runtime.Serialization;

namespace RestAPI.DTO;

[DataContract]
public class DocumentDTO
{
    public IFormFile File { get; set; }
}