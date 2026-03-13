using DevExpress.XtraReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auth.Report;

namespace Auth.Controllers
{
    public class ReportController : Controller
    {

        public ActionResult ShowReport()
        {
            return View();
        }

        // GET: Report
        [HttpGet]
        public ActionResult DocumentViewerPartial()
        {
            UsersReport report = new UsersReport(); 
            return PartialView("_ReportViewerPartial", report);
        }
    }
}