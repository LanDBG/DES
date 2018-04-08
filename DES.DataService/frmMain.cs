using Newtonsoft.Json;
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
using System.Web;
using System.Web.Script.Serialization;
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
                if (DateTime.Now >= dNextRun || bRunNow) //Time to run
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
                        if (child.Name == "API") obj.API = child.InnerText;
                        if (child.Name == "GetSetting") obj.GetSetting = child.InnerText;
                        if (child.Name == "UpdateSetting") obj.UpdateSetting = child.InnerText;
                    }
                    tables.Add(obj);
                }
            }
            catch (Exception)
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

        private TableSetting GetSetting(Table table)
        {
            string data = string.Format("ConfigName={0}", table.Name);
            string result = APIHelper(table.GetSetting, data, table.Name);
            JsonSerializer serializer = new JsonSerializer();
            TableSetting obj = JsonConvert.DeserializeObject<TableSetting>(result);
            return obj;
        }
        private string UpdateSetting(TableSetting setting, Table table)
        {
            string data = string.Format("ConfigName={0}&StartValue={1}&EndValue={2}&LastValue={3}",
                                            setting.ConfigName, setting.StartValue, setting.EndValue, setting.LastValue);
            return APIHelper(table.UpdateSetting, data, table.Name);
            
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

        #region Main processing

        private void RunSchedule() {
            List<Table> tables = ReadConfig();
            foreach (Table table in tables)
            {
                ProcessTable(table);
            }
        }
        private void ProcessTable(Table table)
        {
            
            WriteLog("Started processing table " + table.Name);
            TableSetting setting = GetSetting(table);
            if (setting.Active)
            {
                #region Master/Truncate table
                if (setting.Truncate)
                {
                    int round = 0;
                    string minvalue = "";
                    while (true)
                    {
                        string query = string.Format("SELECT TOP {0} {1} FROM {2} WHERE 1 = 1 {3} ORDER BY {4} ASC",
                                                                setting.RowPerRound,      //0
                                                                setting.Columns,          //1
                                                                setting.SourceTable,      //2
                                                                round == 0 ? "" : " AND " + setting.PrimaryKey + " > '" + minvalue + "'", //3
                                                                setting.PrimaryKey        //4
                                                                );
                        string currentmaxvalue = "";
                        string data = GetData(query, setting, out currentmaxvalue);
                        if (!string.IsNullOrEmpty(data))
                        {
                            PostDataToAPI(table.API, data, round == 0, table.Name, round);
                            round++;

                            minvalue = currentmaxvalue;
                        }
                        else break;
                    }
                }
                #endregion
                #region Event table - get config from server
                else
                {

                    int round = 0;
                    string minvalue = "";
                    while (true)
                    {
                        string query = string.Format("SELECT TOP {0} {1} FROM {2} WHERE 1 = 1 {3} {4} ORDER BY {5} ASC",
                                                                setting.RowPerRound,      //0
                                                                setting.Columns,          //1
                                                                setting.SourceTable,      //2
                                                                round == 0 ?            //3
                                                                        (string.IsNullOrEmpty(setting.LastValue) ? "" :
                                                                        " AND " + setting.PrimaryKey + " > '" + setting.LastValue + "'") :
                                                                        " AND " + setting.PrimaryKey + " > '" + minvalue + "'",
                                                                " AND (" + setting.WhereColumn + " BETWEEN '" + setting.StartValue + "' AND '" + setting.EndValue + "'" + ")",   //4
                                                                setting.PrimaryKey); //5
                        string currentmaxvalue = "";
                        string data = GetData(query, setting, out currentmaxvalue);
                        if (!string.IsNullOrEmpty(data))
                        {
                           
                            PostDataToAPI(table.API, data, false, table.Name, round);
                            round++;

                            minvalue = currentmaxvalue;
                        }
                        else break;
                    }
                    //Update setting back to server
                    setting.StartValue = setting.EndValue;
                    setting.LastValue = FormatData(minvalue);
                    int number = 0;
                    if (int.TryParse(setting.EndValue, out number)) //if number
                    {
                        setting.EndValue = (number + setting.Sequence).ToString();
                    }
                    else //if a date
                    {
                        DateTime dt;
                        if (DateTime.TryParse(setting.EndValue, out dt))
                        {
                            setting.EndValue = dt.AddHours(setting.Sequence).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                    string response = UpdateSetting(setting, table);
                    WriteLog("Update setting : " + response);
                }
            }
            #endregion
            WriteLog("End of processing table " + table.Name);
        }
        #endregion

        #region Helper
        private string FormatData(string s) {
            int num;
            if(int.TryParse(s, out num))
            {
                return num.ToString();
            }
            else
            {
                DateTime dt;
                if (DateTime.TryParse(s, out dt))
                    return dt.ToString("yyyy-MM-dd HH:mm:ss");
                else return "";
            }
        }
        private string GetData(string query, TableSetting setting, out string minvalue)
        {
            string data = string.Empty;
            minvalue = "";
            DataSet ds = new DataSet();
            string sError = "";
            if (SqlHelper.ExecuteSQLDataSet(ds, query, out sError) > -1)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count == 0) return "";
                else
                    //get max value
                    minvalue = dt.Rows[dt.Rows.Count - 1][setting.PrimaryKey].ToString();
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
                        content += "'" + row[index].ToString().Replace("'", "''") + "',";
                    }
                    content = content.Length > 0 ? content.Substring(0, content.Length - 1) : content;
                    data += content + "\n";
                }
                
            }
            else
                WriteLog("Read data from " + setting.SourceTable + " Failed : " + sError);

            return data;
        }
        private bool PostDataToAPI(string webAPI, string data, bool isTruncate, string tablename, int round)
        {
            string postData = string.Format("Truncate={0}&Data={1}", isTruncate ? "True" : "False", HttpUtility.UrlEncode(data));
            string result = APIHelper(webAPI, postData, tablename);
            WriteLog(tablename + " post data to API (round " + (round + 1) + "):" + result);
            return result == "Success";

        }
        private string APIHelper(string webAPI, string data, string tablename)
        {
            try
            {
                var cookies = new CookieContainer();
                var request = (HttpWebRequest)WebRequest.Create(webAPI);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = cookies;
                byte[] postBytes = Encoding.Default.GetBytes(data);
                request.ContentLength = postBytes.Length;
                using (Stream body = request.GetRequestStream())
                {
                    body.Write(postBytes, 0, postBytes.Length);
                }

                WebResponse response = request.GetResponse();
                var requestStream = response.GetResponseStream();
                var streamReader = new StreamReader(requestStream);
                string result = streamReader.ReadToEnd();
                
                return result;
            }
            catch (Exception ex)
            {
                WriteLog("Post data to API ( " + tablename + ") Exception: " + ex.Message);
                return ex.Message;
            }

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
