using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;

namespace DES.DataService
{
    public partial class frmMain : Form
    {

        System.Timers.Timer aTimer = new System.Timers.Timer();
        int IntervalInHour = Int32.Parse(ConfigurationManager.AppSettings["IntervalInHour"]);
        int MaxRows = Int32.Parse(ConfigurationManager.AppSettings["MaxRows"]);
        Dictionary<string, int> LastIdInTable = new Dictionary<string, int>();
        public frmMain()
        {
            InitializeComponent();
            InnitTimer();
        }
        public void InnitTimer()
        {

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent); 
            aTimer.Interval = 60 * 60000; // 60 mins
            aTimer.Enabled = true;
            WriteLog("Schedule service started");
            txtStatus.Text = "Schedule was started";


            aTimer.Enabled = true;
            btnStop.Enabled = true;
            btnStart.Enabled = false;


        }

       
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            WriteLog("Schedule event running");

            CheckShedule();

            WriteLog("Schedule proccess done");
        }
        private void CheckShedule(bool bRunNow = false)
        {

            aTimer.Stop();
            try
            {
                //If it is time to run, just run RunSchedule
                DateTime dNextRun = ReadLocalData(out LastIdInTable);
                if(DateTime.Now >= dNextRun || bRunNow) //Time to run
                {
                    RunSchedule();
                    //Write localdata back
                    dNextRun = dNextRun.AddHours(IntervalInHour);
                    WriteLocalData(dNextRun, LastIdInTable);
                }
            }
            catch (Exception ex)
            {
                // Log
                WriteLog("CheckShedule failed:" + ex.Message);
            }
            finally
            {
                aTimer.Start();
            }
        }
        #region Configuration
        private List<Table> ReadConfig()
        {
            List<Table> tables = new List<Table>();
            try
            {
                string xmlFile = File.ReadAllText("Config\\Config.xml");
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlFile);
                XmlNodeList nodeList = xmldoc.GetElementsByTagName("Table");
                string Short_Fall = string.Empty;

                foreach (XmlNode node in nodeList)
                {
                    Table obj = new Table();
                    obj.Name = node.Attributes["name"].Value;

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.Name == "QueryString") obj.QueryString = child.InnerText;
                        if (child.Name == "API") obj.API = child.InnerText;
                        if (child.Name == "Truncate")
                        {
                            bool truncate = false;
                            bool.TryParse(child.InnerText, out truncate);
                            obj.Truncate = truncate;
                        }

                    }
                    tables.Add(obj);
                }
            }
            catch
            {
                WriteLog("Failed to reading config file");

            }
            return tables;
        }
        private DateTime ReadLocalData(out Dictionary<string, int> dic)
        {
            DateTime result = DateTime.Now;
            dic = new Dictionary<string, int>();
            var lines = File.ReadLines(@"Config\LocalData.txt");
            foreach (var line in lines)
            {
                if(line.StartsWith("NextRun"))
                {
                    var token = line.Split('=');
                    result = DateTime.ParseExact(token[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture); ;
                }
                else
                {
                    var token = line.Split('=');
                    dic.Add(token[0], int.Parse(token[1]));
                }
            }
            return result;
        }
        private void WriteLocalData(DateTime nextrun, Dictionary<string, int> items) {
            List<string> lines = new List<string>();
            lines.Add("NextRun=" + nextrun.ToString("yyyy-MM-dd HH:mm"));
            foreach(var item in items)
            {
                lines.Add(item.Key + "=" + item.Value.ToString());
            }
            File.WriteAllLines(@"Config\LocalData.txt", lines);
        }
        #endregion

        #region Helper


        private void RunSchedule() {
            List<Table> tables = ReadConfig();
            foreach (Table table in tables)
            {
                ProcessTable(table);
            }
        }
        private void ProcessTable(Table table)
        {
            DataSet ds = new DataSet();
            string sError = "";
            WriteLog("Started processing table " + table.Name);
            //Master/Lookup table
            if (table.Truncate)
            {
                if (SqlHelper.ExecuteSQLDataSet(ds, table.QueryString, out sError) > -1)
                {
                    DataTable dt = ds.Tables[0];
                    string data = string.Empty;
                    //Get header
                    foreach (DataColumn col in dt.Columns)
                    {
                        data += col.ColumnName + ",";
                    }
                    data = data.Substring(0, data.Length - 1) + System.Environment.NewLine;
                    //Get content of data
                    foreach (DataRow row in dt.Rows)
                    {
                        string content = "";
                        for (int index = 0; index < dt.Columns.Count; index++)
                        {
                            content += "'" + row[index].ToString().Replace("'","''") + "',";
                        }
                        content = content.Length > 0 ? content.Substring(0, content.Length - 1) : content;
                        data += content + "\n";
                    }
                    Task.Run(() => DataAsync(table.API, data, table.Truncate, table.Name));
                }
                else
                    WriteLog(table.Name + "Failed : " + sError);
            }
            //Event table - should check last id 
            //else
            //{
                
            //}
            WriteLog("End of processing table " + table.Name);
        }
        public async Task DataAsync(string webAPI, string data, bool isTruncate, string tablename)
        {
            Uri uri = new Uri(webAPI);
            string host = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
            string query = uri.PathAndQuery;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(host);
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("Truncate", isTruncate ? "True" : "False"),
                        new KeyValuePair<string, string>("Data", data)
                    });
                    var result = await client.PostAsync(query, content);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    WriteLog("Post data to API ( " + tablename + "): " + resultContent);
                    
                }
            }
            catch(Exception ex) { WriteLog(ex.Message); }
        }

        #endregion

        #region Logger
        delegate void StringArgReturningVoidDelegate(string text);
        private void WriteLog(string text)
        {

            if (this.txtLog.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(WriteLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txtLog.Text += text + " at " + DateTime.Now.ToString() + System.Environment.NewLine;
            }

        }
        #endregion

        #region Event
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (aTimer.Enabled == false)
            {
                aTimer.Enabled = true;
                WriteLog("Schedule service started");
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                txtStatus.Text = "Schedule was started";
            }
            else
            {
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                WriteLog("Schedule service still running ");

            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {

            if (aTimer.Enabled == false)
            {
                WriteLog("Schedule has stopped");
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            else
            {
                aTimer.Enabled = false;
                btnStop.Enabled = false;
                btnStart.Enabled = true;
                WriteLog("Schedule service stoped");
                txtStatus.Text = "Schedule was stopped";
            }
        }
        private void runNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteLog("Force start checking ");

            CheckShedule(true);

            WriteLog("Schedule proccess done");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStop_Click(sender, e);
            Application.Exit();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStart_Click(sender, e);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStop_Click(sender, e);
        }
        #endregion


    }
}
