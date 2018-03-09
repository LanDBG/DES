using DSE.Common.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.Common.Excel
{
    public class ExcelHelper
    {
        /// <summary>
        /// Get columns name and data type
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="sheetName">name of sheet</param>
        /// <returns></returns>
        public List<ExcelColumnItem> GetColumns(string filePath, string sheetName)
        {
            List<ExcelColumnItem> res = new List<ExcelColumnItem>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    // 2. Use the AsDataSet extension method
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {

                        // Gets or sets a value indicating whether to set the DataColumn.DataType 
                        // property in a second pass.
                        UseColumnDataType = true,

                        // Gets or sets a callback to obtain configuration options for a DataTable. 
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {

                            // Gets or sets a value indicating the prefix of generated column names.
                            EmptyColumnNamePrefix = "",

                            // Gets or sets a value indicating whether to use a row from the 
                            // data as column names.
                            UseHeaderRow = true,

                            // Gets or sets a callback to determine which row is the header row. 
                            // Only called when UseHeaderRow = true.
                            ReadHeaderRow = (rowReader) =>
                            {
                                // F.ex skip the first row and use the 2nd row as column headers:
                                rowReader.Read();
                            },

                            // Gets or sets a callback to determine whether to include the 
                            // current row in the DataTable.
                            FilterRow = (rowReader) =>
                            {
                                return true;
                            },
                        }
                    });

                    if (result.Tables[sheetName] != null)
                    {
                        foreach (DataColumn item in result.Tables[sheetName].Columns)
                        {
                            res.Add(new ExcelColumnItem()
                            {
                                ColumnName = item.ColumnName,
                                ColumnType = item.DataType.FullName
                            });
                        }
                    }

                    // The result of each spreadsheet is in result.Tables
                }
            }

            return res;
        }

        /// <summary>
        /// Get name of sheets
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public List<string> GetSheets(string filePath)
        {
            List<string> res = new List<string>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    // 2. Use the AsDataSet extension method
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {

                        // Gets or sets a value indicating whether to set the DataColumn.DataType 
                        // property in a second pass.
                        UseColumnDataType = true,

                        // Gets or sets a callback to obtain configuration options for a DataTable. 
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {

                            // Gets or sets a value indicating the prefix of generated column names.
                            EmptyColumnNamePrefix = "",

                            // Gets or sets a value indicating whether to use a row from the 
                            // data as column names.
                            UseHeaderRow = true,

                            // Gets or sets a callback to determine which row is the header row. 
                            // Only called when UseHeaderRow = true.
                            ReadHeaderRow = (rowReader) =>
                            {
                                // F.ex skip the first row and use the 2nd row as column headers:
                                rowReader.Read();
                            },

                            // Gets or sets a callback to determine whether to include the 
                            // current row in the DataTable.
                            FilterRow = (rowReader) =>
                            {
                                return true;
                            },
                        }
                    });

                    foreach (DataTable item in result.Tables)
                    {
                        res.Add(item.TableName);
                    }
                }
            }

            return res;
        }

        public string CreateTableScript(string filePath)
        {
            string filename = Path.GetFileNameWithoutExtension(filePath);
            string script = "SET ANSI_NULLS ON" + Environment.NewLine +
                                    //"GO " + Environment.NewLine +
                                    "SET QUOTED_IDENTIFIER ON " + Environment.NewLine;
                                    //"GO " + Environment.NewLine;

            List<ExcelColumnItem> res = new List<ExcelColumnItem>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    // 2. Use the AsDataSet extension method
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {

                        // Gets or sets a value indicating whether to set the DataColumn.DataType 
                        // property in a second pass.
                        UseColumnDataType = true,

                        // Gets or sets a callback to obtain configuration options for a DataTable. 
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {

                            // Gets or sets a value indicating the prefix of generated column names.
                            EmptyColumnNamePrefix = "",

                            // Gets or sets a value indicating whether to use a row from the 
                            // data as column names.
                            UseHeaderRow = true,

                            // Gets or sets a callback to determine whether to include the 
                            // current row in the DataTable.
                            FilterRow = (rowReader) =>
                            {
                                return true;
                            },
                        }
                    });

                    foreach (DataTable table in result.Tables)
                    {
                        // Create table
                        script += Environment.NewLine;
                        script += "IF OBJECT_ID('dbo.[temp_" + filename + "_" + table.TableName + "]', 'U') IS NOT NULL" + Environment.NewLine +
                                    "   DROP TABLE dbo.[temp_" + filename + "_" + table.TableName + "]; " + Environment.NewLine +
                                   "CREATE TABLE [dbo].[temp_" + filename + "_" + table.TableName + "](" + Environment.NewLine +
                                    "[id] [int] IDENTITY(1,1) NOT NULL," + Environment.NewLine;

                        foreach (DataColumn item in table.Columns)
                        {
                            script += string.Format("[{0}][nvarchar](500) NULL,", item.ColumnName) + Environment.NewLine;
                        }

                        script += "PRIMARY KEY CLUSTERED(" + Environment.NewLine +
                           "[id] ASC" + Environment.NewLine +
                           ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" + Environment.NewLine +
                           ") ON[PRIMARY]" + Environment.NewLine;

                        // Insert data

                        //string insertq = "INSERT INTO [dbo].[temp_" + filename + "_" + table.TableName + "](";

                        //for (int i = 0; i < table.Columns.Count; i++)
                        //{
                        //    insertq += "["+ table.Columns[i].ColumnName+"]";
                        //    if (i < table.Columns.Count - 1) insertq += ",";
                        //}
                        //insertq += ")VALUES({0})" + Environment.NewLine;

                        //foreach (DataRow row in table.Rows)
                        //{
                        //    string dataScript = "";

                        //    for (int i = 0; i < table.Columns.Count; i++)
                        //    {
                        //        string val = row[i].ToString();
                        //        dataScript += "N'" + val.Replace("'","`") + "'";
                        //        if (i < table.Columns.Count - 1) dataScript += ",";
                        //    }

                        //    script += string.Format(insertq, dataScript);
                        //}
                    }



                    // The result of each spreadsheet is in result.Tables
                }
            }



            return script;
        }

        public string CreateInsertScript(string filePath)
        {
            string filename = Path.GetFileNameWithoutExtension(filePath);
            string script = "SET ANSI_NULLS ON" + Environment.NewLine +
                                    //"GO " + Environment.NewLine +
                                    "SET QUOTED_IDENTIFIER ON " + Environment.NewLine;
            //"GO " + Environment.NewLine;

            List<ExcelColumnItem> res = new List<ExcelColumnItem>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    // 2. Use the AsDataSet extension method
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {

                        // Gets or sets a value indicating whether to set the DataColumn.DataType 
                        // property in a second pass.
                        UseColumnDataType = true,

                        // Gets or sets a callback to obtain configuration options for a DataTable. 
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {

                            // Gets or sets a value indicating the prefix of generated column names.
                            EmptyColumnNamePrefix = "",

                            // Gets or sets a value indicating whether to use a row from the 
                            // data as column names.
                            UseHeaderRow = true,

                            // Gets or sets a callback to determine whether to include the 
                            // current row in the DataTable.
                            FilterRow = (rowReader) =>
                            {
                                return true;
                            },
                        }
                    });

                    foreach (DataTable table in result.Tables)
                    {
                        // Create table
                        script += Environment.NewLine;
                        //script += "IF OBJECT_ID('dbo.[temp_" + filename + "_" + table.TableName + "]', 'U') IS NOT NULL" + Environment.NewLine +
                        //            "   DROP TABLE dbo.[temp_" + filename + "_" + table.TableName + "]; " + Environment.NewLine +
                        //           "CREATE TABLE [dbo].[temp_" + filename + "_" + table.TableName + "](" + Environment.NewLine +
                        //            "[id] [int] IDENTITY(1,1) NOT NULL," + Environment.NewLine;

                        //foreach (DataColumn item in table.Columns)
                        //{
                        //    script += string.Format("[{0}][nvarchar](500) NULL,", item.ColumnName) + Environment.NewLine;
                        //}

                        //script += "PRIMARY KEY CLUSTERED(" + Environment.NewLine +
                        //   "[id] ASC" + Environment.NewLine +
                        //   ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]" + Environment.NewLine +
                        //   ") ON[PRIMARY]" +Environment.NewLine;

                        //// Insert data

                        string insertq = "INSERT INTO [dbo].[temp_" + filename + "_" + table.TableName + "](";

                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            insertq += "[" + table.Columns[i].ColumnName + "]";
                            if (i < table.Columns.Count - 1) insertq += ",";
                        }
                        insertq += ")VALUES({0})" + Environment.NewLine;

                        foreach (DataRow row in table.Rows)
                        {
                            string dataScript = "";

                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                string val = row[i].ToString();
                                dataScript += "N'" + val.Replace("'", "`") + "'";
                                if (i < table.Columns.Count - 1) dataScript += ",";
                            }

                            script += string.Format(insertq, dataScript);
                        }
                    }



                    // The result of each spreadsheet is in result.Tables
                }
            }



            return script;
        }
    }
}
