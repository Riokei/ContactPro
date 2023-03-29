using ContactPro.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ContactPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Route("/Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code) 
        {
            var customError = new CustomError();
            customError.code = code;
            switch (code)
            {
                case 401:
                    customError.message = "Not authorized! Are you logged in?";
                    break;
                case 403:
                    customError.message = "Not authorized! Do you have permission to access this page?";
                    break;
                case 404:
                    customError.message = "The page you're looking for might have been removed, had it's name changed, or is temporarily unavailable.";
                    break;
                case 500:
                    customError.message = "We're sorry something broke on our end, you could try refreshing the page!";
                    break;

                default:
                    customError.message = "Sorry! Something went wrong!";
                    break;
                    
                
            }
            return View("~/Views/Shared/CustomError.cshtml",customError);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}