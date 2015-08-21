using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssetsViewSP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdvancedSearch()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = string.Empty; //"Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = string.Empty; //"Your contact page.";

            return View();
        }
    }
}