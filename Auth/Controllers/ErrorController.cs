using System.Web.Mvc;

namespace Auth.Controllers
{
    public class ErrorController : Controller
    {
        // General error page
        public ActionResult Index()
        {
            ViewBag.Message = "An unexpected error occurred.";
            return View("Error");
        }

        // 404 - Page not found
        public ActionResult NotFound()
        {
            Response.StatusCode = 404; // important for SEO & browser
            return View("NotFound");
        }
    }
}