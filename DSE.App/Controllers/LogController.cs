using DSE.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSE.DataAccess.Data;
using System.Web.Configuration;
using Newtonsoft.Json;
using DSE.DataAccess.Models;
using DSE.App.Helpers;
using DSE.Common;

namespace DSE.App.Controllers
{
    public class LogController : Controller
    {
        const int FUNCTION_ID = 1;
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        LogFactory _clientFactory = new LogFactory(dseCnnString);
        // GET: Log
        public ActionResult Index()
        {
            if (!CheckRole(ERole.Read))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ContentResult GetDataTable(DataTableRequest querry)
        {
            int count = 0;
            var data = _clientFactory.GetLogs(querry.start, querry.length, ref count);

            var categoriesDto = new DataTableResult<Log>(querry.draw, count, count, data);
            var result = JsonConvert.SerializeObject(categoriesDto);

            return Content(result, "application/json");
        }

        private bool CheckRole(ERole role)
        {
            if(Session[DSEConstant.UserId] == null)
            {
                return false;
            }
            Guid userId = new Guid(Session[DSEConstant.UserId].ToString());
            switch (role)
            {
                case ERole.Create:
                    return CommonFunction.IsValidRoleCreate(FUNCTION_ID, userId);
                case ERole.Read:
                    return CommonFunction.IsValidRoleRead(FUNCTION_ID, userId);
                case ERole.Update:
                    return CommonFunction.IsValidRoleUpdate(FUNCTION_ID, userId);
                case ERole.Delete:
                    return CommonFunction.IsValidRoleDelete(FUNCTION_ID, userId);
                default:
                    return false;
            }
        }
    }
}