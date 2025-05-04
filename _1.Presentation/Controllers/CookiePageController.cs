using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _1.PresentationLayer.Controllers
{ 
        [AllowAnonymous]
        public class CookiePageController : Controller
        {
            public IActionResult Index()
            {
                return View();
            }

        }
}
