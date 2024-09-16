using Microsoft.AspNetCore.Mvc; 

namespace RestAPI.Controllers;

[ApiController]
[Route("uploads")]
public class UploadController : ControllerBase
{    
    [HttpPost]
    public string Upload(string fileName)
    {
        return "upload successful\n";
    }
}