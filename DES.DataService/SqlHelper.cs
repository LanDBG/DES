using System;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace DES.DataService
{
    public class SqlHelper
    {
        static string strGlobalConnectionString = ConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        public static int ExecuteSQLDataSet(DataSet dataSet, string sSQL, out string sErrorMessage)
        {
            int returnValue = 0;
            sErrorMessage = "";

            SqlCommand command = new SqlCommand();
            command.CommandText = sSQL;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 3000;
            command.Connection = new SqlConnection(strGlobalConnectionString);

            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

            try
            {
                dataAdapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                returnValue = -1;
                sErrorMessage = e.Message;
            }
            command.Connection.Close();
            return returnValue;
        }
    }
}
