using DSE.App.Models;
using DSE.Common;
using DSE.Common.Excel;
using DSE.DataAccess;
using DSE.DataAccess.Data;
using DSE.DataAccess.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class SourceController : Controller
    {
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        UserFactory _userFactory = new UserFactory(dseCnnString);
        RoleFactory _roleFactory = new RoleFactory(dseCnnString);
        ClientFactory _clientFactory = new ClientFactory(dseCnnString);
        SourceFactory _sourceFactory = new SourceFactory(dseCnnString);
        SourceMappingFactory _sourceMappingFactory = new SourceMappingFactory(dseCnnString);
        ILog log = log4net.LogManager.GetLogger(typeof(SourceController));

        private bool CheckLogin()
        {
            if (Session[DSEConstant.LoginState] == null
                || Convert.ToBoolean(Session[DSEConstant.LoginState]) == false)
            {
                return false;
            }
            return true;
        }

        public ActionResult Index()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            try
            {
                List<Source> sourceCollection = _sourceFactory.GetSourceCollection();
                return View(sourceCollection);
            }
            catch (Exception ex)
            {
                log.Error("Get source list", ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult TargetIndex()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            try
            {
                List<Source> sourceCollection = _sourceFactory.GetSourceCollection();
                return View(sourceCollection);
            }
            catch (Exception ex)
            {
                log.Error("Get source list", ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Config(Guid id, int type)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            switch (type)
            {
                case (int)SourceType.SqlServer:
                    return RedirectToAction("SQLConfig", "Source", new { id = id });
                case (int)SourceType.Excel:
                    return RedirectToAction("ExcelConfig", "Source", new { id = id });
                default:
                    return RedirectToAction("Index", "Source");
            }
        }

        public ActionResult ExcelConfig(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            Source source = new Source();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                source = _sourceFactory.GetSource(id);
                if (source != null && !string.IsNullOrEmpty(source.CurrentTableName))
                {
                    try
                    {
                        var tablesname = source.CurrentTableName.Split(',');
                        source.CurrentData = new List<DataTable>();
                        foreach (var item in tablesname)
                        {
                            source.CurrentData.Add(_sourceFactory.GetSourceImportedData(item));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Can not get temp data of " + source.SourceName, ex);
                        source.CurrentData = null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source", ex);
                return RedirectToAction("Index", "Source");
            }
            return View(source);
        }

        public ActionResult SQLConfig(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (id == null) return RedirectToAction("Index", "Error");
            Source source = new Source();
            try
            {
                source = _sourceFactory.GetSource(id);               
            }
            catch (Exception ex)
            {
                log.Error("Get source", ex);
                return RedirectToAction("Index", "Source");
            }
            return View(source);           
        }

        [HttpPost]
        public ActionResult SQLConfig(Source source)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            ViewBag.SuccessMessage = string.Empty;
            ViewBag.ErrorMessage = string.Empty;
            try
            {
                var sourcex = _sourceFactory.GetSource(source.SourceId);

                sourcex.UpdateBy = Guid.Parse(Session[DSEConstant.UserId].ToString());
                sourcex.UpdateDate = DateTime.Now;
                sourcex.Source_ServerName = source.Source_ServerName;
                sourcex.Source_Server_DatabaseName = source.Source_Server_DatabaseName;
                sourcex.Source_Server_Password = source.Source_Server_Password;
                sourcex.Source_Server_Username = source.Source_Server_Username;
                source = _sourceFactory.Update(sourcex);
                if (source.Updated)
                {
                    ViewBag.SuccessMessage = "SQL Connection setting updated";
                }
            }
            catch (Exception ex)
            {
                log.Error("Save connection setting ", ex);
                ViewBag.ErrorMessage = "Conection setting can not save. Return message : " + ex.Message;
            }

            return View(source);

           
        }

        [HttpPost]
        public ActionResult ExcelConfig(Source source)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            Guid userId = (Guid)Session[DSEConstant.UserId];
            try
            {
                string tablenames = "";
                string messTablenames = "";
                var fileName = string.Empty;
                int res = 0;
                
                // File validate
                if (Request.Files.Count > 0 )
                {
                    string script = "" + Environment.NewLine;
                    string scriptIns = "" + Environment.NewLine;
                    var file = Request.Files[0];


                    if (file != null && file.ContentLength > 0 && (file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx")))
                    {
                        fileName = Path.GetFileName(file.FileName);
                        Directory.CreateDirectory(Server.MapPath("~/Temp/"));
                        var pathTemp = Path.Combine(Server.MapPath("~/Temp/"), fileName);
                        file.SaveAs(pathTemp);

                        ExcelHelper eh = new ExcelHelper();
                        script += eh.CreateTableScript(pathTemp);
                        scriptIns += eh.CreateInsertScript(pathTemp);
                        Directory.CreateDirectory(Server.MapPath("~/Backup/"));
                        var path = Path.Combine(Server.MapPath("~/Backup/"), DateTime.Now.ToString("ddmmyyyyhhmmss_") + fileName);
                        file.SaveAs(path);

                        var paths = eh.GetSheets(pathTemp);
                        if (paths.Count > 0)
                        {
                            foreach (var tableName in paths)
                            {
                                tablenames += "temp_" + Path.GetFileNameWithoutExtension(file.FileName) + "_" + tableName + ",";
                                messTablenames += Path.GetFileNameWithoutExtension(file.FileName) + "_" + tableName + ",";
                            }
                            int endIdx = tablenames.Length;
                            tablenames = tablenames.Substring(0, endIdx - 1);
                            int endIdxx = messTablenames.Length;
                            messTablenames = messTablenames.Substring(0, endIdxx - 1); 
                        }

                        if (System.IO.File.Exists(pathTemp)) System.IO.File.Delete(pathTemp);

                        // Run create table with transaction
                        script += Environment.NewLine + "";
                        SqlHelper.ExecuteNonQuery(dseCnnString, CommandType.Text, script);
                        res = SqlHelper.ExecuteNonQuery(dseCnnString, CommandType.Text, scriptIns);

                        ViewBag.SuccessMessage = "";
                        if (res > 0)
                        {

                           
                            source.UpdateBy = userId;
                            source.UpdateDate = DateTime.Now;
                            source.CurrentTableName = tablenames;
                            source.UploadFileName = fileName;
                            source.NumberOfRecord = res;                            
                            source = _sourceFactory.Update(source);

                            // Validation
                            DataTable resTable = _sourceFactory.GetValidationTable(source.SourceId, userId);

                            if (resTable != null && resTable.Rows.Count > 0)
                            {
                                ViewBag.ErrorMessage = fileName + " file import unsuccessfully, please check validation result. ";
                                source.Status = false;
                                source.UpdateBy = userId;
                                                              
                                source.LastRun = DateTime.Now;
                                source = _sourceFactory.Update(source);
                            }
                            else
                            {

                                bool resRun = _sourceFactory.RunData(source.SourceId, userId) == 1;
                                if (resRun)
                                {
                                    //SetText(s.SourceName + " process done." + System.Environment.NewLine);
                                    ViewBag.SuccessMessage = fileName + " file imported successfully. " + res + " records";
                                    source.Status = true;
                                    source.UpdateBy = userId;
                                                                     
                                    source.LastRun = DateTime.Now;
                                    source = _sourceFactory.Update(source);
                                }
                                else
                                {
                                    //SetText(s.SourceName + "Process to run data from imported data was failed." + System.Environment.NewLine);
                                    ViewBag.ErrorMessage = fileName + " file import unsuccessfully ";
                                    source.Status = false;
                                    source.UpdateBy = userId;                                    
                                    source.LastRun = DateTime.Now;
                                    source = _sourceFactory.Update(source);
                                }
                            }

                        }
                        else
                        {
                            ViewBag.ErrorMessage = fileName + " file is not upload successfull ";
                            source.Status = false;
                            source.UpdateBy = userId;
                            source.UpdateDate = DateTime.Now;
                            source.CurrentTableName = tablenames;
                            source.UploadFileName = fileName;
                            source.NumberOfRecord = res;
                            source.LastRun = DateTime.Now;
                            source = _sourceFactory.Update(source);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = fileName + " file is incorrect format ";                        
                    }                   
                }
                else
                {
                    ViewBag.ErrorMessage = "Please, select a file ";                    
                }

            }
            catch (Exception ex)
            {
                log.Error("Import Excel", ex);
                ViewBag.ErrorMessage = ex.Message;               
                source.Status = false;
                source.UpdateBy = userId;
                source.UpdateDate = DateTime.Now;               
                source.LastRun = DateTime.Now;
                source = _sourceFactory.Update(source);
            }

            if (source != null && !string.IsNullOrEmpty(source.CurrentTableName))
            {
                try
                {
                    var tablesname = source.CurrentTableName.Split(',');
                    source.CurrentData = new List<DataTable>();
                    foreach (var item in tablesname)
                    {
                        source.CurrentData.Add(_sourceFactory.GetSourceImportedData(item));
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Can not get temp data of " + source.SourceName, ex);
                    source.CurrentData = null;
                }
            }

            return View(source);
        }

        public ActionResult Mapping(Guid id, int type)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            if (id == null) return RedirectToAction("Index", "Error");

            switch (type)
            {
                case (int)SourceType.SqlServer:
                    return RedirectToAction("SQLMapping", "Source", new { id = id });
                case (int)SourceType.Excel:
                    return RedirectToAction("ExcelMapping", "Source", new { id = id });
                case (int)SourceType.Target:
                    return RedirectToAction("SQLMapping", "Source", new { id = id });
                default:
                    return RedirectToAction("Index", "Source");
            }
        }

        public ActionResult ExcelMapping(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            SourceMappingModel sourceMapping = new SourceMappingModel();

            if (id == null) return RedirectToAction("Index", "Error");

            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    sourceMapping.SourceId = source.SourceId;
                    sourceMapping.SourceName = source.SourceName;
                    sourceMapping.MappingData = new List<SourceMapping>();
                    try
                    {
                        //sourceMapping.MappingData = _sourceMappingFactory.GetSourceMappingCollection(id);
                        sourceMapping.MappingTable = _sourceMappingFactory.GetSourceMappingTable(id);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Can not get temp mapping data of " + source.SourceName, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Source Mapping", ex);
                return RedirectToAction("Index", "Source");
            }
            return View(sourceMapping);
        }

        public ActionResult SQLMapping(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            SourceMappingModel sourceMapping = new SourceMappingModel();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    sourceMapping.SourceId = source.SourceId;
                    sourceMapping.SourceName = source.SourceName;
                    sourceMapping.SourceType = source.SourceType;
                    sourceMapping.MappingData = new List<SourceMapping>();
                    try
                    {
                        //sourceMapping.MappingData = _sourceMappingFactory.GetSourceMappingCollection(id);
                        sourceMapping.MappingTable = _sourceMappingFactory.GetSourceMappingTable(id);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Can not get temp mapping data of " + source.SourceName, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Source Mapping", ex);
                return RedirectToAction("Index", "Source");
            }
            return View(sourceMapping);
        }

        public ActionResult Validation(Guid id, int type)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (id == null) return RedirectToAction("Index", "Error");
            switch (type)
            {
                case (int)SourceType.SqlServer:
                    return RedirectToAction("SQLValidation", "Source", new { id = id });
                case (int)SourceType.Excel:
                    return RedirectToAction("ExcelValidation", "Source", new { id = id });
                case (int)SourceType.Target:
                    return RedirectToAction("SQLValidation", "Source", new { id = id });
                default:
                    return RedirectToAction("Index", "Source");
            }
        }

        public ActionResult ExcelValidation(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            ValidateModel sourceValidate = new ValidateModel();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    sourceValidate.SourceId = source.SourceId;
                    sourceValidate.SourceName = source.SourceName;
                    sourceValidate.ValidationResult = new List<ValidateItem>();
                    try
                    {
                        Guid userId = (Guid)Session[DSEConstant.UserId];
                        //sourceValidate.ValidationResult = _sourceFactory.GetValidationResult(id, userId);
                        sourceValidate.ValidationTable = _sourceFactory.GetValidationTable(id, userId);

                        if (sourceValidate.ValidationTable != null && sourceValidate.ValidationTable.Rows.Count > 0)
                        {
                            ViewBag.ErrorMessage = "Validation failed";
                            source.Status = false;
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                            
                        }
                        else
                        {
                            ViewBag.SuccessMessage = "Your data is validated. ";
                            source.Status = true;
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Validation error", ex);
                        ViewBag.ErrorMessage = "Validation progress end with error(s)" + System.Environment.NewLine + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Validation", ex);
                ViewBag.ErrorMessage = "Validation progress end with error(s)" + System.Environment.NewLine + ex.Message;
            }
            return View(sourceValidate);

        }

        public ActionResult SQLValidation(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            ValidateModel sourceValidate = new ValidateModel();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    sourceValidate.SourceId = source.SourceId;
                    sourceValidate.SourceName = source.SourceName;
                    sourceValidate.SourceType = source.SourceType;
                    sourceValidate.ValidationResult = new List<ValidateItem>();
                    try
                    {
                        Guid userId = (Guid)Session[DSEConstant.UserId];
                        //sourceValidate.ValidationResult = _sourceFactory.GetValidationResult(id, userId);
                        sourceValidate.ValidationTable = _sourceFactory.GetValidationTable(id, userId);
                        if (sourceValidate.ValidationTable!=null && sourceValidate.ValidationTable.Rows.Count > 0)
                        {
                            ViewBag.ErrorMessage = "Validation failed";
                            source.Status = false;
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                        }
                        else
                        {
                            ViewBag.SuccessMessage = "Your data is validated. ";
                            source.Status = true;
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Validation error", ex);
                        ViewBag.ErrorMessage = "Validation progress end with error(s)" + System.Environment.NewLine + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Validation", ex);
                ViewBag.ErrorMessage = "Validation progress end with error(s)" + System.Environment.NewLine + ex.Message;
            }
            return View(sourceValidate);

        }

        public ActionResult Schedule(Guid id, int type)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (id == null) return RedirectToAction("Index", "Error");
            switch (type)
            {
                case (int)SourceType.SqlServer:
                    return RedirectToAction("SQLSchedule", "Source", new { id = id });
                case (int)SourceType.Excel:
                    return RedirectToAction("ExcelSchedule", "Source", new { id = id });
                case (int)SourceType.Target:
                    return RedirectToAction("SQLSchedule", "Source", new { id = id });
                default:
                    return RedirectToAction("Index", "Source");
            }
        }

        public ActionResult SQLSchedule(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (id == null) return RedirectToAction("Index", "Error");
            RunResultModel runResult = new RunResultModel();
            ViewBag.ErrorMessage = string.Empty;
            ViewBag.ErrorMessage = string.Empty;

            ViewBag.SErrorMessage = TempData["SErrorMessage"];
            ViewBag.SSuccessMessage = TempData["SSuccessMessage"];

            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    runResult.SourceId = source.SourceId;
                    runResult.SourceName = source.SourceName;
                    runResult.Source = source;                    
                }
                else
                {
                    return RedirectToAction("Index", "Error");
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Run Data", ex);
                return RedirectToAction("Index", "Error");
            }
            return View(runResult);

        }

        public ActionResult SQLRunData(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            RunResultModel runResult = new RunResultModel();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    runResult.SourceId = source.SourceId;
                    runResult.SourceName = source.SourceName;
                    runResult.Source = source;
                    try
                    {
                        Guid userId = (Guid)Session[DSEConstant.UserId];

                        if (source.SourceType == 3)
                        {
                            runResult.Success = _sourceFactory.RunDataTarget(id, userId) == 1;
                        }
                        else{
                            DataTable resTable = _sourceFactory.GetValidationTable(source.SourceId, userId);
                            if (resTable != null && resTable.Rows.Count > 0)
                            {
                                ViewBag.ErrorMessage = "Process unsuccessfully, please check validation result. ";
                                source.Status = false;
                                source.UpdateBy = userId;
                                source.LastRun = DateTime.Now;
                                _sourceFactory.Update(source);
                                runResult.Success = false;
                            }
                            else
                            {
                                runResult.Success = _sourceFactory.RunData(id, userId) == 1;
                            }
                        }
                        

                        if (runResult.Success)
                        {
                            ViewBag.SuccessMessage = "Process done.";
                            source.Status = true;
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Process to run data from imported data was failed, please see logs for more information";
                            source.Status = false;
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(source.SourceName + " run data error", ex);
                        ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get SQL Source in Run Data", ex);
                ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
            }
            return View("SQLSchedule", runResult);

        }
        public ActionResult ExcelSchedule(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (id == null) return RedirectToAction("Index", "Error");
            RunResultModel runResult = new RunResultModel();
            ViewBag.ErrorMessage = string.Empty;
            ViewBag.ErrorMessage = string.Empty;

            ViewBag.SErrorMessage = TempData["SErrorMessage"];
            ViewBag.SSuccessMessage = TempData["SSuccessMessage"];

            try
            {
                Source source = _sourceFactory.GetSource(id);
                if (source != null)
                {
                    runResult.SourceId = source.SourceId;
                    runResult.SourceName = source.SourceName;
                    runResult.Source = source;
                }
                else
                {
                    return RedirectToAction("Index", "Error");
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Run Data", ex);
                return RedirectToAction("Index", "Error");
            }
            return View(runResult);
        }

        public ActionResult ExcelRunData(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            RunResultModel runResult = new RunResultModel();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                Source source = _sourceFactory.GetSource(id);
                Guid userId = (Guid)Session[DSEConstant.UserId];

                if (source != null)
                {
                    runResult.SourceId = source.SourceId;
                    runResult.SourceName = source.SourceName;
                    runResult.Source = source;
                    try
                    {
                        DataTable resTable = _sourceFactory.GetValidationTable(source.SourceId, userId);

                        if (resTable != null && resTable.Rows.Count > 0)
                        {
                            ViewBag.ErrorMessage = "Process unsuccessfully, please check validation result. ";
                            source.Status = false;
                            source.UpdateBy = userId;                          
                            source.LastRun = DateTime.Now;
                            _sourceFactory.Update(source);
                            runResult.Success = false;
                        }
                        else
                        {

                            runResult.Success = _sourceFactory.RunData(id, userId) == 1;

                            if (runResult.Success)
                            {
                                ViewBag.SuccessMessage = "Process done.";
                                source.Status = true;
                                source.LastRun = DateTime.Now;
                                _sourceFactory.Update(source);
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Process to run data from imported data was failed, please see logs for more information";
                                source.Status = false;
                                source.LastRun = DateTime.Now;
                                _sourceFactory.Update(source);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Run data error", ex);
                        ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get source in Run Data", ex);
                ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
            }
            return View("ExcelSchedule", runResult);

        }

        public ActionResult ExcelManualRunData(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            RunResultModel runResult = new RunResultModel();
            if (id == null) return RedirectToAction("Index", "Error");
            try
            {
                Source source = _sourceFactory.GetSource(id);
                Guid userId = (Guid)Session[DSEConstant.UserId];


                if (source != null)
                {

                    runResult.SourceId = source.SourceId;
                    runResult.SourceName = source.SourceName;
                    runResult.Source = source;


                    try
                    {
                        runResult.Success = RunExcel(source, userId);

                        if (runResult.Success)
                        {
                            ViewBag.SuccessMessage = "Process done.";
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Process to run data from imported data was failed, please see logs for more information";
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Run data error", ex);
                        ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
                    }
                }

                //if (source != null)
                //{
                //    runResult.SourceId = source.SourceId;
                //    runResult.SourceName = source.SourceName;
                //    runResult.Source = source;
                //    try
                //    {
                //        Guid userId = (Guid)Session[DSEConstant.UserId];
                //        runResult.Success = _sourceFactory.RunData(id, userId) == 1;

                //        if (runResult.Success)
                //        {
                //            ViewBag.SuccessMessage = "Process done.";
                //            source.Status = true;
                //            source.LastRun = DateTime.Now;
                //            _sourceFactory.Update(source);
                //        }
                //        else
                //        {
                //            ViewBag.ErrorMessage = "Process to run data from imported data was failed, please see logs for more information";
                //            source.Status = false;
                //            source.LastRun = DateTime.Now;
                //            _sourceFactory.Update(source);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        log.Error("Run data error", ex);
                //        ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
                //    }
                //}
            }
            catch (Exception ex)
            {
                log.Error("Get source in Run Data", ex);
                ViewBag.ErrorMessage = "Run data progress end with error(s)" + System.Environment.NewLine + ex.Message;
            }
            return View("ExcelSchedule", runResult);

        }

        private bool RunExcel(Source s,Guid userId)
        {
            // Validate
            try
            {
                string tablenames = "";
                var fileFullName = s.Source_Server_FilePath;
                int res = 0;
                string fileName = Path.GetFileName(fileFullName);
                string scriptIns = "" + Environment.NewLine;
                // File validate
                if (System.IO.File.Exists(fileFullName))
                {
                    string script = "BEGIN TRAN T1;" + Environment.NewLine;
                    //var file = Request.Files[0];


                    if (fileFullName.EndsWith(".xls") || fileFullName.EndsWith(".xlsx"))
                    {

                        ExcelHelper eh = new ExcelHelper();
                        script += eh.CreateTableScript(fileFullName);
                        scriptIns += eh.CreateInsertScript(fileFullName);
                        
                        // backup

                        Directory.CreateDirectory(Server.MapPath("~/Backup/"));
                        var path = Path.Combine(Server.MapPath("~/Backup/"), DateTime.Now.ToString("ddmmyyyyhhmmss_") + fileName);
                        
                        System.IO.File.Copy(fileFullName, path);

                        // Import
                        var paths = eh.GetSheets(fileFullName);
                        if (paths.Count > 0)
                        {
                            foreach (var tableName in paths)
                            {
                                tablenames += "temp_" + Path.GetFileNameWithoutExtension(fileFullName) + "_" + tableName + ",";
                            }
                            int endIdx = tablenames.Length;
                            tablenames = tablenames.Substring(0, endIdx - 1);
                        }

                        //if (System.IO.File.Exists(fileFullName)) System.IO.File.Delete(pathTemp);
                    }
                    else
                    {
                        throw new Exception(s.SourceName + ": Your upload file incorrect format");
                    }

                    // Run create table with transaction
                    script += Environment.NewLine + "COMMIT TRAN T1;";
                    SqlHelper.ExecuteNonQuery(dseCnnString, CommandType.Text, script);
                    res = SqlHelper.ExecuteNonQuery(dseCnnString, CommandType.Text, scriptIns);
                    
                    if (res > 0)
                    {                       
                        s.UpdateBy = userId;
                        s.UpdateDate = DateTime.Now;
                        s.CurrentTableName = tablenames;
                        s.UploadFileName = fileFullName;
                        s.NumberOfRecord = res;
                        s = _sourceFactory.Update(s);

                        // Validation
                        DataTable resTable = _sourceFactory.GetValidationTable(s.SourceId, userId);
                        if (resTable != null && resTable.Rows.Count > 0)
                        {
                            ViewBag.ErrorMessage = fileName + " file import unsuccessfully, please check validation result. ";
                            s.Status = false;
                            s.UpdateBy = userId;
                            s.LastRun = DateTime.Now;
                            s = _sourceFactory.Update(s);
                        }
                        else
                        {
                            bool resRun = _sourceFactory.RunData(s.SourceId, userId) == 1;
                            if (resRun)
                            {
                                s.Status = true;
                                s.LastRun = DateTime.Now;
                                _sourceFactory.Update(s);
                            }
                            else
                            {
                                s.Status = false;
                                s.LastRun = DateTime.Now;
                                _sourceFactory.Update(s);
                            }
                        }

                    }                   
                    return s.Status;
                }
                else
                {
                    throw new Exception(s.SourceName + ": Scheduled file not found in " + s.Source_Server_FilePath);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw ex;
            }

        }

        [HttpPost]
        public ActionResult SaveSchedule(Source source)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            ViewBag.SSuccessMessage = string.Empty;
            ViewBag.SErrorMessage = string.Empty;
            try
            {

                source.Schedule_Start_Time = source.Schedule_Start_Time_D.Date.Add(source.Schedule_Start_Time_T.TimeOfDay);

                var sourcex = _sourceFactory.GetSource(source.SourceId);

                sourcex.UpdateBy = Guid.Parse(Session[DSEConstant.UserId].ToString());
                sourcex.UpdateDate = DateTime.Now;
                sourcex.Is_Scheduled = source.Is_Scheduled;
                sourcex.Source_Server_FilePath = source.Source_Server_FilePath;
                sourcex.Schedule_Start_Time = source.Schedule_Start_Time;
                sourcex.Schedule_Interval = source.Schedule_Interval;
                source = _sourceFactory.Update(sourcex);
                if (source.Updated)
                {
                    ViewBag.SSuccessMessage = "Source setting updated";
                }
            }
            catch (Exception ex)
            {
                log.Error("Save Schedule ", ex);
                ViewBag.SErrorMessage = "Schedule setting can not save. Return message : " + ex.Message;
            }
            TempData["SErrorMessage"] = ViewBag.SErrorMessage;
            TempData["SSuccessMessage"] = ViewBag.SSuccessMessage;
            if (source.SourceType == 1||source.SourceType==3)
                return RedirectToAction("SQLSchedule", "Source", new { id = source.SourceId });
            if (source.SourceType == 2)
                return RedirectToAction("ExcelSchedule", "Source", new { id = source.SourceId });
            else
                return RedirectToAction("Index", "Error");

        }


        public ActionResult LeftMenu()
        {
            try
            {
                List<Source> sourceCollection = _sourceFactory.GetSourceCollection();
                return PartialView(sourceCollection);
            }
            catch (Exception ex)
            {
                log.Error("Get source list", ex);
                return PartialView(new List<Source>());
            }
        }
    }
}