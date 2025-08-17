using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MecaFlow2025.Controllers
{
    [AllowAnonymous]
    public class AcercaNosotrosController : Controller
    {
        public IActionResult Index() => View();
    }
}
