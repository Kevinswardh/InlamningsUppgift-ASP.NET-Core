using _1.PresentationLayer.ViewModels.ProjectsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _1.PresentationLayer.Controllers
{
    public class CreateTestController : Controller
    {
        [HttpPost]
        public IActionResult Index([FromForm] CreateProjectViewModel model)
        {
            Console.WriteLine("✅ POST kördes!");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState invalid:");
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($" - {entry.Key}: {error.ErrorMessage}");
                    }
                }

                // Här kan du returnera detaljer om felen
                return Content("ModelState invalid", "text/plain");
            }

            // Om modelbinding lyckades
            return Content("✅ Modellbindning lyckades!", "text/plain");
        }

    }
}