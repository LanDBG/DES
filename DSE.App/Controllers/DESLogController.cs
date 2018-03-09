using DSE.App.Models;
using DSE.DataAccess.Data;
using DSE.DataAccess.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class DESLogController : Controller
    {
        const int FUNCTION_ID = 1;
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        LogViewFactory _clientFactory = new LogViewFactory(dseCnnString);
        SourceFactory _sourceFactory = new SourceFactory(dseCnnString);

        // GET: DESLog
        public ActionResult Index(Guid id, int type)
        {
            ViewBag.source = _sourceFactory.GetSource(id);
            ViewBag.id = id.ToString();
            return View();
        }

        public ContentResult GetDataTable(DataTableRequest querry, Guid id)
        {
            int count = 0;
            var data = _clientFactory.GetLogs(id, querry.start, querry.length, ref count);

            var categoriesDto = new DataTableResult<LogViewModel>(querry.draw, count, count, data);
            var result = JsonConvert.SerializeObject(categoriesDto);

            return Content(result, "application/json");
        }

        public ActionResult Detail(Guid id, Guid source)
        {
            ViewBag.source = _sourceFactory.GetSource(source);
            ViewBag.id = id.ToString();
            var model = _clientFactory.LogDetailByJobId(id);
            return View("DetailList", model);
        }
    }
}