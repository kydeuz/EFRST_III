using Microsoft.AspNetCore.Mvc;

namespace SGEB.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}