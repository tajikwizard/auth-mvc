using System.Web.Mvc;

namespace Auth.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            // Check session
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Шумо барои дастрасӣ ба ин саҳифа иҷозат надоред. Лутфан ворид шавед.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}