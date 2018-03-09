using DSE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class AnalyticController : Controller
    {
        // GET: Tableau
        public ActionResult Index()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            string tableauServer = WebConfigurationManager.AppSettings["TableauServer"];
            string xMergeIP = WebConfigurationManager.AppSettings["XMergeIP"];
            string tableauUser = WebConfigurationManager.AppSettings["TableauUser"];

            string url = $"{tableauServer}/trusted";
            WebClient wc = new WebClient();

            wc.QueryString.Add("username", tableauUser);
            wc.QueryString.Add("client_ip", xMergeIP);

            var data = wc.UploadValues(url, "POST", wc.QueryString);

            var responseString = UnicodeEncoding.UTF8.GetString(data);

            ViewBag.ticket = responseString;

            return View();
        }

        private bool CheckLogin()
        {
            if (Session[DSEConstant.LoginState] == null
                || Convert.ToBoolean(Session[DSEConstant.LoginState]) == false)
            {
                return false;
            }
            return true;
        }
    }
}