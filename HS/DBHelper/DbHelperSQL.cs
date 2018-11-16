using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
namespace HS
{
    public sealed class DbHelperSQL
    {
        private static string connectionString;
        public static string UseXml()
        {
            XMLHealper opXML = new XMLHealper("Data.xml");
            string conn,datasource, database, userid, password;
            datasource = opXML.PathGetNodeValue("system/datasource");
            database = opXML.PathGetNodeValue("system/database");
            userid = opXML.PathGetNodeValue("system/userid");
            password = opXML.PathGetNodeValue("system/password");
            conn = "data source=" + datasource + ";database =" + database + "; user id =" + userid + "; password =" + password;
            return conn.Trim();
        }
       public static bool ExConnect()
        {
            try
            {
                connectionString = UseXml();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    return true;
                }
            }
            catch 
            {
                return false;
            }
            return false;
        }
        public static string Execute(string SQLString)
        {
            string result = "-1";
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        if (cmd.ExecuteScalar() == null)
                        {
                            return result;
                        }
                        else
                        {
                            string rows = cmd.ExecuteScalar().ToString();
                            result = rows;
                        }
                    }
                    catch (SqlException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
            return result;
        }
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(DbHelperSQL.connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int i = 0; i < SQLStringList.Count; i++)
                    {
                        string strsql = SQLStringList[i].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (SqlException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    conn.Close();
                }
            }
        }
        public static int ExecuteSql(string SQLString)
        {
            int st = -1;
            connectionString = UseXml();
            //int result;
            try
            {
                using (SqlConnection conn = new SqlConnection(DbHelperSQL.connectionString))
                {

                    using (SqlCommand cmd = new SqlCommand(SQLString, conn))
                    {
                        conn.Open();
                        st = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                return st;
            }
            return st;
        }
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            int result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                SqlCommand cmd = new SqlCommand(strSQL, connection);
                SqlParameter myParameter = new SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    result = rows;
                }
                catch (SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return result;
        }
        public static object GetSingle(string SQLString)
        {
            object result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            result = null;
                        }
                        else
                        {
                            result = obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
            return result;
        }
        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = DbHelperSQL.GetSingle(strsql);
            int result;
            if (obj == null)
            {
                result = 1;
            }
            else
            {
                result = int.Parse(obj.ToString());
            }
            return result;
        }
        public static int GetMinID(string FieldName, string TableName)
        {
            string strsql = "select min(" + FieldName + ")+1 from " + TableName;
            object obj = DbHelperSQL.GetSingle(strsql);
            int result;
            if (obj == null)
            {
                result = 1;
            }
            else
            {
                result = int.Parse(obj.ToString());
            }
            return result;
        }
        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            object obj = DbHelperSQL.GetSingle(strSql, cmdParms);
            int cmdresult;
            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            return cmdresult != 0;
        }
        public static SqlDataReader ExecuteReader(string strSQL)
        {
            SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString);
            SqlCommand cmd = new SqlCommand(strSQL, connection);
            SqlDataReader result;
            try
            {
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader();
                result =myReader;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
            return result;
        }
        public static DataSet Query(string SQLString)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                result = ds;
            }
            return result;
        }
        public static DataTable OpenTable(string SQLString)
        {
            DataTable result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(SQLString, connection);
                DataTable dt = new DataTable();
                try
                {
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    da.Dispose();
                    connection.Close();
                }
                result = dt;
            }
            return result;
        }
        public static int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            int result=-1;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        DbHelperSQL.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        result = rows;
                    }
                    catch (SqlException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
            return result;
        }
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(DbHelperSQL.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            DbHelperSQL.PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            object result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        DbHelperSQL.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            result = null;
                        }
                        else
                        {
                            result = obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
            return result;
        }
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader result;
            try
            {
                DbHelperSQL.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                result = myReader;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
            return result;
        }
        public static DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                DbHelperSQL.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    result = ds;
                }
            }
            return result;
        }
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                for (int i = 0; i < cmdParms.Length; i++)
                {
                    SqlParameter parm = cmdParms[i];
                    cmd.Parameters.Add(parm);
                }
            }
        }
        public static SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString);
            connection.Open();
            SqlCommand command = DbHelperSQL.BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            return command.ExecuteReader();
        }
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                new SqlDataAdapter
                {
                    SelectCommand = DbHelperSQL.BuildQueryCommand(connection, storedProcName, parameters)
                }.Fill(dataSet, tableName);
                connection.Close();
                result = dataSet;
            }
            return result;
        }
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < parameters.Length; i++)
            {
                SqlParameter parameter = (SqlParameter)parameters[i];
                command.Parameters.Add(parameter);
            }
            return command;
        }
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            int result2;
            using (SqlConnection connection = new SqlConnection(DbHelperSQL.connectionString))
            {
                connection.Open();
                SqlCommand command = DbHelperSQL.BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                int result = (int)command.Parameters["ReturnValue"].Value;
                result2 = result;
            }
            return result2;
        }
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = DbHelperSQL.BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        private static Hashtable GetStrValue(string TableName, string ColumnType, string ColumnName, string Value, ArrayList GoalColumns)
        {
            Hashtable result = new Hashtable();
            string SqlStr = "select ";
            foreach (object obj in GoalColumns)
            {
                SqlStr = SqlStr + obj.ToString() + ",";
            }
            SqlStr = SqlStr.Substring(0, SqlStr.Length - 1);
            if (ColumnType.ToLower() == "int")
            {
                string text = SqlStr;
                SqlStr = string.Concat(new string[]
				{
					text,
					" from ",
					TableName,
					" where ",
					ColumnName,
					" = ",
					Value
				});
            }
            else
            {
                string text = SqlStr;
                SqlStr = string.Concat(new string[]
				{
					text,
					" from ",
					TableName,
					" where ",
					ColumnName,
					" = '",
					Value,
					"'"
				});
            }
            DataTable dt = DbHelperSQL.OpenTable(SqlStr);
            if (dt.Rows.Count == 1)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    result.Add(dt.Columns[i].Caption, dt.Rows[0][i].ToString());
                }
            }
            return result;
        }
        public static Hashtable GetUserInfo(string user_key)
        {
            return DbHelperSQL.GetStrValue("APP_USER", "int", "user_key", user_key, new ArrayList
			{
				"user_name",
				"first_name",
				"login_name",
				"category"
			});
        }
        public static string ToJson(DataTable dt)
        {
            StringBuilder Json = new StringBuilder();
            Json.Append("[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Json.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Type type = dt.Rows[i][j].GetType();
                        Json.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + StringFormat(dt.Rows[i][j].ToString(), type));
                        if (j < dt.Columns.Count - 1)
                        {
                            Json.Append(",");
                        }
                    }
                    Json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        Json.Append(",");
                    }
                }
            }
            Json.Append("]");
            return Json.ToString();
        }
        public static string ToJson(DataTable dt, int id)
        {
            StringBuilder Json = new StringBuilder();
            Json.Append("[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Json.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Type type = dt.Rows[i][j].GetType();
                        Json.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + StringFormat(dt.Rows[i][j].ToString(), type));
                        if (j < dt.Columns.Count - 1)
                        {
                            Json.Append(",");
                        }
                    }
                    Json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        Json.Append(",");
                    }
                }
            }
            Json.Append("]");
            return Json.ToString();
        }
        public static string ToJson(DataTable dt, string jsonName)
        {
            StringBuilder Json = new StringBuilder();
            if (string.IsNullOrEmpty(jsonName))
                jsonName = dt.TableName;
            Json.Append("{\"" + jsonName + "\":[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Json.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Type type = dt.Rows[i][j].GetType();
                        Json.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + StringFormat(dt.Rows[i][j].ToString(), type));
                        if (j < dt.Columns.Count - 1)
                        {
                            Json.Append(",");
                        }
                    }
                    Json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        Json.Append(",");
                    }
                }
            }
            Json.Append("]}");
            return Json.ToString();
        }
        public static string ToJson(DataTable dt, string jsonName, string total, int count)
        {
            StringBuilder Json = new StringBuilder();
            if (string.IsNullOrEmpty(jsonName))
                jsonName = dt.TableName;
            Json.Append("{\"" + total + "\":" + count + ",\"" + jsonName + "\":[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Json.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Type type = dt.Rows[i][j].GetType();
                        Json.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + StringFormat(dt.Rows[i][j].ToString(), type));
                        if (j < dt.Columns.Count - 1)
                        {
                            Json.Append(",");
                        }
                    }
                    Json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        Json.Append(",");
                    }
                }
            }
            Json.Append("]}");
            return Json.ToString();
        }
        private static string StringFormat(string str, Type type)
        {
            if (type == typeof(string))
            {
                str = String2Json(str);
                str = "\"" + str + "\"";
            }
            else if (type == typeof(DateTime))
            {
                str = "\"" + str + "\"";
            }
            else if (type == typeof(bool))
            {
                str = str.ToLower();
            }
            else if (type != typeof(string) && string.IsNullOrEmpty(str))
            {
                str = "\"" + str + "\"";
            }
            return str;
        }
        private static string String2Json(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '>':
                        sb.Append("lt;"); break;
                    case '<':
                        sb.Append("gt;"); break;
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
    }
}
