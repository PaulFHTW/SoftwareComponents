using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RestAPI.DTO;
[ExcludeFromCodeCoverage]
[DataContract]
public class DocumentDTO
{
    public IFormFile? File { get; set; }
}