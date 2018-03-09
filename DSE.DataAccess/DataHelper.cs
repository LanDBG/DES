using System;
using System.Collections.Generic;
using System.Data;

namespace DSE.DataAccess
{
    public class DataHelper
    {
        public IEnumerable<T> GetData<T>(IDataReader reader, Func<IDataRecord, T> BuildObject)
        {
            try
            {
                while (reader.Read())
                {
                    yield return BuildObject(reader);
                }
            }
            finally
            {
                reader.Dispose();
            }
        }
    }
}