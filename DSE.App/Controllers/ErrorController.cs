using DSE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(int id)
        {
            ViewBag.Title = "Error";
            ViewBag.ErrorMessge = DSEConstant.Error_Message_Default;

            if (id == 502)
            {
                ViewBag.Title = "Access denied";
                ViewBag.ErrorMessge = "Access denied";
            }


            return View();
        }
    }
}