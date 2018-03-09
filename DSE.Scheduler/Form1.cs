using DSE.Common.Excel;
using DSE.DataAccess;
using DSE.DataAccess.Data;
using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Timers;
using System.Windows.Forms;

namespace DSE.Scheduler
{
    public partial class frmMain : Form
    {

        System.Timers.Timer aTimer = new System.Timers.Timer();
        static string dseCnnString = ConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        Guid appUserId = Guid.Parse(ConfigurationManager.AppSettings["appUserId"]);
        SourceFactory _sourceFactory = new SourceFactory(dseCnnString);
        SourceMappingFactory _sourceMappingFactory = new SourceMappingFactory(dseCnnString);



        public frmMain()
        {
            InitializeComponent();
            InnitTimer();
        }

        //private void btnLog_Click(object sender, EventArgs e)
        //{
        //    frmLog fl = new frmLog();
        //    fl.ShowDialog(this);
        //}

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (aTimer.Enabled == false)
            {
                aTimer.Enabled = true;
                txtLog.Text += "Schedule service started at " + DateTime.Now.ToString() + System.Environment.NewLine;
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                txtStatus.Text = "Schedule was started";
            }
            else
            {
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                txtLog.Text += "Schedule service still running " + System.Environment.NewLine;
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {

            if (aTimer.Enabled == false)
            {
                txtLog.Text += "Schedule has stopped" + System.Environment.NewLine;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            else
            {
                aTimer.Enabled = false;
                btnStop.Enabled = false;
                btnStart.Enabled = true;
                txtLog.Text += "Schedule service stoped at " + DateTime.Now.ToString() + System.Environment.NewLine;
                txtStatus.Text = "Schedule was stopped";
            }
        }

        public void InnitTimer()
        {

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 10 * 60000; // 10mins
            aTimer.Enabled = true;
            txtLog.Text = "Schedule service started at " + DateTime.Now + System.Environment.NewLine;
            txtStatus.Text = "Schedule was started";


            aTimer.Enabled = true;
            btnStop.Enabled = true;
            btnStart.Enabled = false;


        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            SetText("Schedule event running at " + DateTime.Now.ToString() + System.Environment.NewLine);

            CheckShedule();

            SetText("Schedule proccess done at " + DateTime.Now.ToString() + System.Environment.NewLine);
        }

        private void CheckShedule()
        {
            DateTime actionTime = DateTime.Now;
            aTimer.Stop();
            // Get schedules item

            try
            {
                List<Source> sources = _sourceFactory.GetSourceCollection();

                foreach (Source s in sources)
                {

                    try
                    {
                        if (s.Is_Scheduled.HasValue && s.Is_Scheduled == 1 && s.Schedule_Start_Time.HasValue && s.Schedule_Interval > 0)
                        {

                            DateTime checkTime = s.Schedule_Start_Time.Value;

                            var runDay = s.Schedule_Start_Time.Value.Date;

                            while (runDay< actionTime.Date)
                            {
                                runDay = runDay.AddDays((double) s.Schedule_Interval.Value / 24);
                            }

                            if (runDay == actionTime.Date && checkTime.Hour == actionTime.Hour)
                            {
                                if (s.Schedule_Start_Time.Value <= actionTime && (s.Schedule_Start_Time.Value.Minute  >= (actionTime.Minute-5) && (s.Schedule_Start_Time.Value.Minute <= (actionTime.Minute +5))))
                                {
                                    RunShedule(s);                                   
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SetText(ex.Message + " " + DateTime.Now.ToString() + System.Environment.NewLine);
                    }
                }
            }
            catch (Exception exx)
            {
                // Log
                SetText(exx.Message + " " + DateTime.Now.ToString() + System.Environment.NewLine);
            }
            finally
            {
                aTimer.Start();
            }
        }

        private void RunShedule(Source s)
        {
            SetText(s.SourceName + " start process " + DateTime.Now.ToString() + System.Environment.NewLine);
            switch (s.SourceType)
            {
                case 1: // SQL
                    RunSQL(s);
                    break;
                case 2: // EXCEL
                    RunExcel(s);
                    break;
                case 3: // DATA TARGET
                    RunDataTarget(s);
                    break;
                default:
                    break;
            }
           
        }

        private void RunSQL(Source s)
        {
            try
            {
                bool res = _sourceFactory.RunData(s.SourceId, appUserId) == 1;
                if (res)
                {
                    SetText(s.SourceName + " process done." + System.Environment.NewLine);
                    s.Status = true;
                    s.LastRun = DateTime.Now;
                    _sourceFactory.Update(s);
                }
                else
                {
                    SetText(s.SourceName + "Process to run data from imported data was failed." + System.Environment.NewLine);
                    s.Status = false;
                    s.LastRun = DateTime.Now;
                    _sourceFactory.Update(s);
                }
            }
            catch (Exception ex)
            {
                SetText(s.SourceName + " run data error: " + ex.Message + System.Environment.NewLine);
            }
        }

        private void RunExcel(Source s)
        {
            // Validate
            try
            {
                string tablenames = "";
                var fileFullName = s.Source_Server_FilePath;
                int res = 0;
                string fileName = Path.GetFileName(fileFullName);

                // File validate
                if (File.Exists(fileFullName))
                {
                    string script = "BEGIN TRAN T1;" + Environment.NewLine;
                    string scriptIns =  Environment.NewLine;
                    //var file = Request.Files[0];


                    if (fileFullName.EndsWith(".xls") || fileFullName.EndsWith(".xlsx"))
                    {

                        ExcelHelper eh = new ExcelHelper();
                        script += eh.CreateTableScript(fileFullName);
                        scriptIns += eh.CreateInsertScript(fileFullName);
                        // backup
                        Directory.CreateDirectory("C:\\interpub\\wwwroot\\dse\\Backup\\");
                        var path = Path.Combine("C:\\interpub\\wwwroot\\dse\\Backup\\", DateTime.Now.ToString("ddmmyyyyhhmmss_") + fileName);
                        File.Copy(fileFullName, path);

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
                    SqlHelper.ExecuteNonQuery(dseCnnString, System.Data.CommandType.Text, script);                   
                    res = SqlHelper.ExecuteNonQuery(dseCnnString, CommandType.Text, scriptIns);

                    if (res > 0)
                    {
                        

                        SetText("Table(s) " + tablenames + "  was create successful. Total " + res + " records " + System.Environment.NewLine);
                        
                        s.UpdateBy = appUserId;
                        s.UpdateDate = DateTime.Now;
                        s.CurrentTableName = tablenames;
                        s.UploadFileName = fileFullName;
                        s.NumberOfRecord = res;
                        s = _sourceFactory.Update(s);

                        DataTable resTable = _sourceFactory.GetValidationTable(s.SourceId, appUserId);

                        if (resTable != null && resTable.Rows.Count > 0)
                        {
                            SetText("Data validation failed, please check log for more information. " + System.Environment.NewLine);
                            s.Status = false;
                            s.UpdateBy = appUserId;
                            s.LastRun = DateTime.Now;
                            _sourceFactory.Update(s);

                        }
                        else
                        {
                            SetText("Data validation successfully. " + System.Environment.NewLine);

                            bool resRun = _sourceFactory.RunData(s.SourceId, appUserId) == 1;
                            if (resRun)
                            {
                                SetText(s.SourceName + " process done." + System.Environment.NewLine);
                                s.Status = true;
                                s.LastRun = DateTime.Now;
                                _sourceFactory.Update(s);
                            }
                            else
                            {
                                SetText(s.SourceName + "Process to run data from imported data was failed." + System.Environment.NewLine);
                                s.Status = false;
                                s.LastRun = DateTime.Now;
                                _sourceFactory.Update(s);
                            }
                        }
                    }                    
                }
                else
                {
                    throw new Exception(s.SourceName + ": Scheduled file not found in " + s.Source_Server_FilePath);
                }

            }
            catch (Exception ex)
            {
                SetText(ex.Message + System.Environment.NewLine);
            }

        }

        private void RunDataTarget(Source s)
        {
            try
            {
                bool res = _sourceFactory.RunDataTarget(s.SourceId, appUserId) == 1;
                if (res)
                {
                    SetText(s.SourceName + " process done." + System.Environment.NewLine);
                    s.Status = true;
                    s.LastRun = DateTime.Now;
                    _sourceFactory.Update(s);
                }
                else
                {
                    SetText(s.SourceName + "Process to run data from imported data was failed." + System.Environment.NewLine);
                    s.Status = false;
                    s.LastRun = DateTime.Now;
                    _sourceFactory.Update(s);
                }
            }
            catch (Exception ex)
            {
                SetText(s.SourceName + " run data error: " + ex.Message + System.Environment.NewLine);
            }
        }

        delegate void StringArgReturningVoidDelegate(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.txtLog.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txtLog.Text += text;
            }
        }

        private void runNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetText("Force start checking " + DateTime.Now.ToString() + System.Environment.NewLine);

            CheckShedule();

            SetText("Schedule proccess done at " + DateTime.Now.ToString() + System.Environment.NewLine);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStop_Click(sender, e);
            Application.Exit();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            else if (this.WindowState != FormWindowState.Minimized)
                this.WindowState = FormWindowState.Minimized;
        }
    }
}
