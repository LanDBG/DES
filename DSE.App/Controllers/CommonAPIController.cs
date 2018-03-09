using DSE.Common;
using DSE.DataAccess.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;

namespace DSE.App.Controllers
{
    public class CommonAPIController : ApiController
    {
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        SourceFactory _sourceFactory = new SourceFactory(dseCnnString);
        ILog log = log4net.LogManager.GetLogger(typeof(CommonAPIController));

        [AcceptVerbs("GET", "HEAD")]
        public bool CheckConnection(string connection) {

            //if (Session[DSEConstant.LoginState] == null
            //    || Convert.ToBoolean(Session[DSEConstant.LoginState]) == false)
            //{
            //    return false;
            //}

            bool res = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open(); // throws if invalid
                    res = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Check connection "+connection+" false", ex);
                return false;
            }
            return res;
        }
    }
}
