using Dapper;
using System.Data.SqlClient;

namespace JWTdemoProject
{
    public static class DBHandler
    {
        public static bool Login(string connectString,string username,string password)
        {
            try
            {
                using (var cn = new SqlConnection(connectString))
                {
                    int count = cn.QueryFirst<int>($"select count(*) from TEST_CMS..UserAccount where UserName = @username and PassWord = @password", new { username = username,password = password });
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("登入有誤: " + ex);
                throw;
            }
        }
    }
}
