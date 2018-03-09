using DSE.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class ArchiveController : Controller
    {
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        ArchiveFactory _archiveFactory = new ArchiveFactory(dseCnnString);

        // GET: Archive
        public ActionResult Index()
        {
            ViewBag.hideSidebar = true;
            var archives = _archiveFactory.GetArchives();
            return View(archives);
        }
    }
}