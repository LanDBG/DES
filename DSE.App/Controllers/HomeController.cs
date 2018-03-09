using DSE.App.Models;
using DSE.Common;
using DSE.DataAccess.Data;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class HomeController : Controller
    {
        const int FUNCTION_ID = 1;
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        KPIFactory _kpiFactory = new KPIFactory(dseCnnString);
        KPIDateFactory _kpiDateFactory = new KPIDateFactory(dseCnnString);
        KpiDataTypeFactory _kpiDataTypesFactory = new KpiDataTypeFactory(dseCnnString);
        ILog log = log4net.LogManager.GetLogger(typeof(HomeController));

        private bool CheckLogin()
        {
            if (Session[DSEConstant.LoginState] == null
                || Convert.ToBoolean(Session[DSEConstant.LoginState]) == false)
            {
                return false;
            }
            return true;
        }

        public ActionResult Index(string archiveName)
        {
            if (CheckLogin())
            {
                ViewBag.UserName = Session[DSEConstant.UserName].ToString();
                ViewBag.hideSidebar = true;
                ViewBag.showFields = _kpiFactory.GetKPIShowFields()?.config_value;
                ViewBag.archiveName = archiveName?.Trim();
                var fieldsName = _kpiFactory.GetFieldsName();
                var fieldsType = _kpiDataTypesFactory.GetKpiDataTypes();
                ViewBag.fieldsName = fieldsName;
                ViewBag.fieldsType = fieldsType;
                var dates = _kpiDateFactory.GetKPIDate(archiveName);
                if(dates != null)
                {
                    ViewBag.startDate = dates.Item1;
                    ViewBag.endDate = dates.Item2;
                }

                return View();
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public ContentResult GetDataTable(DataTableRequest querry, string id)
        {
            int count = 0;
            var data = _kpiFactory.GetKPIs(id, querry.start, querry.length, ref count);

            var categoriesDto = new DataTableResult<object>(querry.draw, count, count, data);
            var result = JsonConvert.SerializeObject(categoriesDto);

            return Content(result, "application/json");
        }

        public ActionResult Detail(string id, string archiveName)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            try
            {
                var kpi = _kpiFactory.GetKPIById(id);
                ViewBag.kpi = kpi;
                ViewBag.archiveName = archiveName;
            }
            catch (Exception ex)
            {
                log.Error("Load detail KPI " + archiveName, ex);
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Update(object kpi, string archiveName)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            var _param = HttpContext.Request.Params;
            var allTypes = new Dictionary<string, Type>();
            var allKeys = _kpiFactory.GetFieldsName(allTypes);
            string sqlCommand = "UPDATE [vw_target_kpi] SET {0} WHERE [o_target_kpi_id] = @id";

            StringBuilder sqlParams = new StringBuilder("");
            foreach (var field in allKeys)
            {
                if (_param.AllKeys.Contains(field) && FieldEditable(field))
                {
                    sqlParams.Append($"{field} = @{field},");
                }

            }

            // remove last ',' character
            sqlParams.Remove(sqlParams.Length - 1, 1);

            sqlCommand = string.Format(sqlCommand, sqlParams);

            try
            {
                using (SqlConnection con = new SqlConnection(dseCnnString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlCommand, con))
                    {

                        foreach (var field in allKeys)
                        {
                            if (_param.AllKeys.Contains(field) && FieldEditable(field))
                            {
                                if (allTypes[field] == typeof(Decimal))
                                {
                                    if (string.IsNullOrEmpty(_param[field]))
                                    {
                                        cmd.Parameters.Add($"@{field}", SqlDbType.Decimal).Value = DBNull.Value;
                                    }
                                    else
                                    {
                                        Decimal value = 0;
                                        if (Decimal.TryParse(_param[field], out value))
                                        {
                                            cmd.Parameters.Add($"@{field}", SqlDbType.Decimal).Value = value;
                                        }
                                        continue;
                                    }
                                }

                                if (allTypes[field] == typeof(String))
                                {
                                    cmd.Parameters.Add($"@{field}", SqlDbType.NChar).Value = _param[field];
                                    continue;
                                }
                                if (allTypes[field] == typeof(Int32))
                                {
                                    cmd.Parameters.Add($"@{field}", SqlDbType.Int).Value = _param[field];
                                    continue;
                                }
                                if (allTypes[field] == typeof(DateTime))
                                {
                                    cmd.Parameters.Add($"@{field}", SqlDbType.DateTime).Value = _param[field];
                                }
                            }
                        }

                        cmd.Parameters.Add("@id", SqlDbType.NChar).Value = _param["o_target_kpi_id"];

                        var result = cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (System.Exception ex)
            {
                log.Error("Update KPI " + archiveName, ex);
            }
            return RedirectToAction("Index", new { archiveName = archiveName });
        }

        public bool FieldEditable(string fieldName)
        {
            bool editable = fieldName.Contains("_tab");
            return editable;
        }

        public ActionResult UpdateKPIShowFields(string fields)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            try
            {
                _kpiFactory.UpdateKPIShowFields(fields);

            }
            catch (Exception ex)
            {
                log.Error("Update home KPI column ", ex);

            }
            return RedirectToAction("Index");
        }
    }
}