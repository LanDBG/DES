using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class UserFuncRoleViewModel
    {
        public int FunctionId { get; set; }
        public string FunctionName { get; set; }
        public List<RoleStatus> Roles { get; set; }
    }

    public class RoleStatus
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
