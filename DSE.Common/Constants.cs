namespace DSE.Common
{
    public enum SourceType
    {
        SqlServer = 1,
        Excel = 2,
        Target = 3
    }

    public class DSEConstant
    {
        public static string DefaultPasswords = "a";

        public static string User = "User";
        public static string LoginState = "LoginState";
        public static string UserId = "UserId";
        public static string UserName = "UserName";
        public static string ClientId = "ClientId";
        public static string Role = "Role";
        public static string User_Duplicate_UserName = "This username has been taken. please choose another";
        public static string User_Duplicate_Email = "This email has been taken. please choose another";
        public static string Error_Message_Default = "An error occurred while processing your request";
    }
}
