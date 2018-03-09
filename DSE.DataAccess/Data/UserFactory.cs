using DSE.Common;
using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Data
{
    public class UserFactory
    {
        public string _connectionString { get; set; }

        public UserFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public User GetUser(Guid id)
        {
            User item = new User();

            //string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("select * FROM [tb_user]  WHERE o_user_id = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    DataHelper dh = new DataHelper();
                    item = dh.GetData(cmd.ExecuteReader(), User.Builder).FirstOrDefault();
                }
            }

            return item;
        }

        public User AuthentiationLogin(UserLogin userLogin, ref int valid)
        {
            User user = new User();
            int is_valid = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_user_check_login", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@user_name", SqlDbType.VarChar).Value = userLogin.UserName;
                        cmd.Parameters.Add("@password", SqlDbType.VarChar).Value = userLogin.Password;
                        cmd.Parameters.Add("@is_valid", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        is_valid = Convert.ToInt32(cmd.Parameters["@is_valid"].Value);
                        valid = is_valid;

                    }
                    if (is_valid > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_user WHERE user_name = @user_name", con))
                        {
                            cmd.Parameters.AddWithValue("@user_name", userLogin.UserName);
                            DataHelper dh = new DataHelper();
                            user = dh.GetData(cmd.ExecuteReader(), User.Builder).FirstOrDefault();
                        }

                    }
                }

                if (is_valid == 0) {
                    user.IsActive = false;                    
                }
                return user;
            }
            catch (Exception ex)
            {
                is_valid = 3;
                user.ErrorMessage = ex.Message;
                return user;
            }
        }

        public User GetUser(string userName)
        {
            User user = new User();


            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_user WHERE user_name = @user_name", con))
                {
                    cmd.Parameters.AddWithValue("@user_name", userName);

                    con.Open();
                    DataHelper dh = new DataHelper();

                    user = dh.GetData(cmd.ExecuteReader(), User.Builder).FirstOrDefault();
                }
            }
            return user;
        }


        public User Update(User user)
        {
            user.Updated = false;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                bool updated = false;
                using (SqlCommand cmd = new SqlCommand(@"  
                    UPDATE tb_user 
                    SET o_client_id   = @client_id,
                        address = @address, email       = @email,
                        phone   = @phone,   is_active   = @is_active,
                        first_name = @first_name, last_name = @last_name,
                        modification_date = @modification_date
                    WHERE user_name = @user_name", con))
                {
                    cmd.Parameters.AddWithValue("@first_name", user.FirstName);
                    cmd.Parameters.AddWithValue("@last_name", user.LastName);
                    cmd.Parameters.AddWithValue("@user_name", user.UserName);
                    cmd.Parameters.AddWithValue("@client_id", (user.ClientId as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@address", user.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@email", user.Email ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", user.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@is_active", user.IsActive == true ? 1 : 0);
                    cmd.Parameters.AddWithValue("@modification_date", user.ModificationDate);

                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            updated = true;
                        }
                    }
                    catch (Exception e)
                    {
                        user.ErrorMessage = e.Message;
                        updated = false;
                    }
                }

                if (updated)
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_user WHERE o_user_id = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", user.UserId);

                        DataHelper dh = new DataHelper();

                        user = dh.GetData(cmd.ExecuteReader(), User.Builder).FirstOrDefault();
                    }
                    user.Updated = true;
                }
            }
            return user;
        }

        public List<User> GetUserCollection()
        {
            List<User> list = new List<User>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("select * FROM [tb_user] ORDER BY  user_name DESC", con))
                {
                    DataHelper dh = new DataHelper();
                    list = dh.GetData(cmd.ExecuteReader(), User.Builder).ToList();
                }
            }

            return list;
        }

        public User Create(User user)
        {
            user.Updated = false;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_user_create", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@first_name", SqlDbType.VarChar).Value = user.FirstName;
                        cmd.Parameters.Add("@last_name", SqlDbType.VarChar).Value = user.LastName;
                        cmd.Parameters.Add("@user_name", SqlDbType.VarChar).Value = user.UserName;
                        cmd.Parameters.Add("@password", SqlDbType.VarChar).Value = user.Password;
                        cmd.Parameters.Add("@address", SqlDbType.VarChar).Value = user.Address;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = user.Email;
                        cmd.Parameters.Add("@phone", SqlDbType.VarChar).Value = user.Phone;
                        cmd.Parameters.Add("@is_valid", SqlDbType.Int).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        user.ErrorCode = Convert.ToInt32(cmd.Parameters["@is_valid"].Value);
                        user.Updated = Convert.ToInt32(cmd.Parameters["@is_valid"].Value) == 1 ? true : false;
                    }
                    if (user.Updated)
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_user WHERE user_name = @user_name", con))
                        {

                            cmd.Parameters.AddWithValue("@user_name", user.UserName);
                            DataHelper dh = new DataHelper();
                            user = dh.GetData(cmd.ExecuteReader(), User.Builder).FirstOrDefault();
                        }
                        user.Updated = true;
                    }
                }
                return user;
            }
            catch (Exception ex)
            {
                user.ErrorMessage = ex.Message;
                user.Updated = false;
                return user;
            }
        }

        public bool Delete(User user)
        {
            return Delete(user.UserId);
        }

        public bool Delete(Guid? userId)
        {
            if (userId != null)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM [tb_user] WHERE [o_user_id] = @userId", con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        DataHelper dh = new DataHelper();
                        try
                        {
                            int effect = cmd.ExecuteNonQuery();
                            if (effect > 0)
                            {
                                return true;
                            }
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        public UserChangePassword ChangePassword(UserChangePassword user)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_user_change_password", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@user_name", SqlDbType.VarChar).Value = user.UserName;
                        cmd.Parameters.Add("@password_old", SqlDbType.VarChar).Value = user.OldPassword;
                        cmd.Parameters.Add("@password_new", SqlDbType.VarChar).Value = user.NewPassword;
                        cmd.Parameters.Add("@password_confirmnew", SqlDbType.VarChar).Value = user.ConfirmNewPassword;
                        cmd.Parameters.Add("@is_valid", SqlDbType.Int).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        user.Updated = Convert.ToInt32(cmd.Parameters["@is_valid"].Value);
                    }
                }
                return user;
            }
            catch (Exception ex)
            {
                user.ErrorMessage = ex.Message;
                user.Updated = 0;
                return user;
            }
        }

        public bool UpdateFunctionRole(Guid userId, int functionId, string roles)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_role_user_update]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@user_id", SqlDbType.UniqueIdentifier).Value = userId;
                        cmd.Parameters.Add("@role_id", SqlDbType.VarChar).Value = roles;
                        cmd.Parameters.Add("@function_id", SqlDbType.VarChar).Value = functionId;

                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<int> GetListRole(Guid userId, int functionId)
        {
            List<int> roles = new List<int>();
            List<UserFuncRole> selectFunctions = new List<UserFuncRole>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [tb_role_user] WHERE [o_user_id] = @o_user_id AND [o_function_id] = @o_function_id", con))
                    {
                        con.Open();
                        DataHelper dh = new DataHelper();

                        cmd.Parameters.Add("@o_user_id", SqlDbType.UniqueIdentifier).Value = userId;
                        cmd.Parameters.Add("@o_function_id", SqlDbType.Int).Value = functionId;

                        selectFunctions = dh.GetData(cmd.ExecuteReader(), UserFuncRole.Builder).ToList();
                        foreach (var item in selectFunctions)
                        {
                            roles.Add(item.o_role_id);
                        }
                    }
                }
                return roles;
            }
            catch (Exception ex)
            {
                // TODO: Add log
                return roles;
            }
        }
    }
}
