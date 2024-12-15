using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RestAPI.DTO;
[ExcludeFromCodeCoverage]
[DataContract]
public class DocumentUpdateDTO
{
    public string? Title { get; set; }
}