using Microsoft.AspNetCore.Mvc; 

namespace RestAPI.Controllers;

[ApiController]
[Route("/")]
public class DefaultController : ControllerBase
{    
    [HttpGet]
    public string GetHomePage()
    {
        return "Welcome\n";
    }
}