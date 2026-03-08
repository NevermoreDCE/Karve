using Microsoft.AspNetCore.Mvc;

namespace Karve.Invoicing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("placeholder");
}