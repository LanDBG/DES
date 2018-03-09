using DSE.Common;
using DSE.DataAccess.Data;
using DSE.DataAccess.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Mvc;
using DSE.App.Helpers;

namespace DSE.App.Controllers
{
    public class AdminUserController : Controller
    {
        const int FUNCTION_ID = 1;
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        UserFactory _userFactory = new UserFactory(dseCnnString);
        RoleFactory _roleFactory = new RoleFactory(dseCnnString);
        FunctionFactory _funcFactory = new FunctionFactory(dseCnnString);
        ClientFactory _clientFactory = new ClientFactory(dseCnnString);
        ILog log = log4net.LogManager.GetLogger(typeof(AdminUserController));
        private bool CheckLogin()
        {
            if (Session[DSEConstant.LoginState] == null
                || Convert.ToBoolean(Session[DSEConstant.LoginState]) == false)
            {
                return false;
            }
            return true;
        }

        private bool CheckRole()
        {
            var a = Session[DSEConstant.Role];
            //if (int.Parse(Session[DSEConstant.Role].ToString()) != 1)
            //if (false)
            //{
            //    return false;
            //}
            return true;
        }


        private bool CheckRole(ERole role)
        {
            Guid userId = new Guid(Session[DSEConstant.UserId].ToString());
            switch (role)
            {
                case ERole.Create:
                    return CommonFunction.IsValidRoleCreate(FUNCTION_ID, userId);
                case ERole.Read:
                    return CommonFunction.IsValidRoleRead(FUNCTION_ID, userId);
                case ERole.Update:
                    return CommonFunction.IsValidRoleUpdate(FUNCTION_ID, userId);
                case ERole.Delete:
                    return CommonFunction.IsValidRoleDelete(FUNCTION_ID, userId);
                default:
                    return false;
            }
        }
        // GET: AdminUser
        public ActionResult Index()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            if (!CheckRole(ERole.Read))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.canEdit = CheckRole(ERole.Update);
            ViewBag.canCreate = CheckRole(ERole.Create);
            ViewBag.canDelete = CheckRole(ERole.Delete);

            List<User> userCollection = _userFactory.GetUserCollection();

            return View(userCollection);
        }

        public ActionResult Create()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Create))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Roles = _roleFactory.GetSelectListRole();
            ViewBag.Clients = _clientFactory.GetSelectListClient();
            return View();
        }

        [HttpPost]
        public ActionResult Create(User user)
        {
            if (!CheckRole()) return RedirectToAction("Index");
            if (!CheckRole(ERole.Create))
            {
                return RedirectToAction("Index");
            }

            user.UserId = Guid.Empty;
            ViewBag.Roles = _roleFactory.GetSelectListRole();
            ViewBag.Clients = _clientFactory.GetSelectListClient();
            ViewBag.Edit = true;

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = DSEConstant.DefaultPasswords;
            }

            user = _userFactory.Create(user);

            if (user.Updated)
            {
                return RedirectToAction("Edit", "AdminUser", new { id = user.UserId });
            }
            else
            {
                log.ErrorFormat("{0} at {1}  ", user.ErrorMessage, DateTime.Now);
                if (user.ErrorCode == 2) ModelState.AddModelError("CustomError", DSEConstant.User_Duplicate_UserName);
                else
                if (user.ErrorCode == 3) ModelState.AddModelError("CustomError", DSEConstant.User_Duplicate_Email);
                else ModelState.AddModelError("CustomError", DSEConstant.Error_Message_Default);
            }

            ViewBag.Roles = _roleFactory.GetSelectListRole();
            ViewBag.Clients = _clientFactory.GetSelectListClient();
            return View(user);
        }

        public ActionResult Edit(Guid id)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Update))
            {
                return RedirectToAction("Index");
            }

            if (id != null)
            {
                try
                {
                    User user = new User();
                    user = _userFactory.GetUser(id);
                    ViewBag.Roles = _roleFactory.GetSelectListRole();
                    ViewBag.Clients = _clientFactory.GetSelectListClient();
                    return View(user);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("{0} \n {1} ", e.Message, e.StackTrace);
                    return RedirectToAction("Index", "AdminUser");
                }
            }

            return RedirectToAction("Index", "AdminUser");
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            //Validation
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Update))
            {
                return RedirectToAction("Index");
            }

            //Update
            ViewBag.Roles = _roleFactory.GetSelectListRole();
            ViewBag.Clients = _clientFactory.GetSelectListClient();
            ViewBag.Edit = true;

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user = _userFactory.Update(user);

            if (!user.Updated)
            {
                log.ErrorFormat("user {0} try to update at {1} \n {2} ", user.UserName, DateTime.Now, user.ErrorMessage);
                ModelState.AddModelError(string.Empty, "Update unsuccessful");
            }

            return View(user);
        }

        public ActionResult Delete(Guid userId)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Delete))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            if (!_userFactory.Delete(userId))
            {

                ViewData.ModelState.AddModelError(string.Empty, "Can not delete this user.");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateFunctionRole(Guid userId, int functionId, string roles)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Update))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            bool result = _userFactory.UpdateFunctionRole(userId, functionId, roles);

            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateRoles(Guid userId)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Update))
            {
                return RedirectToAction("Index");
            }

            List<UserFuncRoleViewModel> root = new List<UserFuncRoleViewModel>();

            var roles = _roleFactory.GetListRole();
            var funcs = _funcFactory.GetListFunction();
            foreach (var item in funcs)
            {
                var userRoles = _userFactory.GetListRole(userId, item.o_function_id);
                List<RoleStatus> roleStatus = new List<RoleStatus>();
                foreach (var role in roles)
                {
                    roleStatus.Add(new RoleStatus()
                    {
                        RoleId = role.RoleId,
                        RoleName = role.RoleName,
                        IsActive = userRoles.Contains(role.RoleId)
                    });
                }
                root.Add(new UserFuncRoleViewModel()
                {
                    FunctionId = item.o_function_id,
                    FunctionName = item.function_name,
                    Roles = roleStatus
                });
            }

            ViewBag.roles = roles;
            ViewBag.funcs = funcs;
            ViewBag.userId = userId.ToString();
            return View(root);
        }

        [HttpPost]
        public ActionResult UpdateRoles(List<UserFuncRoleViewModel> roleMatrix, Guid userId)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            if (!CheckRole(ERole.Update))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            bool result = false;
            foreach (var func in roleMatrix)
            {
                string roles = string.Empty;
                foreach (var item in func.Roles)
                {
                    if (item.IsActive)
                    {
                        roles += item.RoleId + ',';
                    }
                }
                if (!string.IsNullOrEmpty(roles))
                {
                    result = _userFactory.UpdateFunctionRole(userId, func.FunctionId, roles);
                }
            }

            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }
    }

    enum ERole
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4
    }
}