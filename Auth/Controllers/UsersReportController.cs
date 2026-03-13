using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auth.Report;
using System.IO;

namespace Auth.Controllers
{
    public class UsersReportController : Controller
    {
        // GET: UsersReport - returns a view that embeds the PDF preview
        public ActionResult Index()
        {
            return View();
        }


        public FileResult Pdf()
        {
            UserReport report = new UserReport();

            using (var ms = new MemoryStream())
            {
                report.ExportToPdf(ms);
                var bytes = ms.ToArray();
                return File(bytes, "application/pdf");
            }
        }
    }
}