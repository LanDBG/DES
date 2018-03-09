using DSE.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace DSE.App.Helpers
{
    public class CommonFunction
    {
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        static UserFactory _userFactory = new UserFactory(dseCnnString);
        static RoleFactory _roleFactory = new RoleFactory(dseCnnString);
        static FunctionFactory _funcFactory = new FunctionFactory(dseCnnString);

        public static bool IsValidRoleAdmin(int functionId, Guid userId)
        {
            // TODO: cache this value
            var roles = _userFactory.GetListRole(userId, functionId);
            if(roles.Count == 4)
            {
                return true;
            }
            return false;
        }

        public static bool IsValidRoleCreate(int functionId, Guid userId)
        {
            // TODO: cache this value
            var roles = _userFactory.GetListRole(userId, functionId);
            return roles.Contains(1);
        }

        public static bool IsValidRoleRead(int functionId, Guid userId)
        {
            // TODO: cache this value
            var roles = _userFactory.GetListRole(userId, functionId);
            return roles.Contains(2);
        }

        public static bool IsValidRoleUpdate(int functionId, Guid userId)
        {
            // TODO: cache this value
            var roles = _userFactory.GetListRole(userId, functionId);
            return roles.Contains(3);
        }

        public static bool IsValidRoleDelete(int functionId, Guid userId)
        {
            // TODO: cache this value
            var roles = _userFactory.GetListRole(userId, functionId);
            return roles.Contains(4);
        }
    }
}