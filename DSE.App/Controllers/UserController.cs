using DSE.App.Models;
using DSE.Common;
using DSE.DataAccess.Data;
using DSE.DataAccess.Models;
using log4net;
using System;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSE.App.Controllers
{
    public class UserController : Controller
    {
        static string dseCnnString = WebConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        UserFactory _userFactory = new UserFactory(dseCnnString);
        RoleFactory _roleFactory = new RoleFactory(dseCnnString);
        ClientFactory _clientFactory = new ClientFactory(dseCnnString);
        // GET: User

        ILog log = log4net.LogManager.GetLogger(typeof(UserController));

        private bool CheckLogin()
        {
            if (Session[DSEConstant.LoginState] == null
                || Convert.ToBoolean(Session[DSEConstant.LoginState]) == false)
            {
                return false;
            }
            return true;
        }

        private void SetLoginState(User loginUser)
        {
            Session[DSEConstant.User] = loginUser;
            Session[DSEConstant.LoginState] = true;
            Session[DSEConstant.ClientId] = loginUser.ClientId;
            Session[DSEConstant.UserName] = loginUser.UserName;
            Session[DSEConstant.UserId] = loginUser.UserId;
        }


        public ActionResult Index()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");
            return View();
        }

        public ActionResult Login()
        {
            UserLogin model = new UserLogin();
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public ActionResult Login(UserLogin model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int valid = 0;
            User loginUser = _userFactory.AuthentiationLogin(model, ref valid);

            if (valid == 1 && loginUser.IsActive.Value)
            {
                // Set login session
                SetLoginState(loginUser);

                log.Info(string.Format("User {0} has logged at {1}", loginUser.UserName, DateTime.Now));

                // Redirect to home page
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Edit = true;
                model.ErrorMessage = "Login fail, please contact webmaster for more information";
                switch (valid)
                {
                    case 0:
                        ModelState.AddModelError(string.Empty, "Invalid username or password");
                        log.Warn(string.Format("User {0} try to login at {1}", model.UserName, DateTime.Now));
                        break;
                    case 2:
                        ModelState.AddModelError(string.Empty, "User not active");
                        log.Warn(string.Format("Inactive user: {0} try to login at {1}", model.UserName, DateTime.Now));
                        break;
                    case 3:
                        ModelState.AddModelError(string.Empty, DSEConstant.Error_Message_Default);
                        log.Error(string.Format("User: {0} try to login at {1} \n {2} ", model.UserName, DateTime.Now,loginUser.ErrorMessage));
                        break;
                }
                return View(model);
            }

        }

        public ActionResult Edit()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            string userName = Session[DSEConstant.UserName].ToString();

            if (!string.IsNullOrEmpty(userName))
            {
                if (Session[DSEConstant.UserName].ToString().Equals(userName))
                {
                    try
                    {
                        User user = new User();
                        user = _userFactory.GetUser(userName);
                        ViewBag.Roles = _roleFactory.GetSelectListRole();
                        ViewBag.Clients = _clientFactory.GetSelectListClient();
                        return View(user);
                    }
                    catch (Exception e)
                    {
                        log.Error(string.Format("{0} at {1} \n {2} ", e.Message, DateTime.Now, e.Source));
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { id = 502 });
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

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

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(UserForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            return View();
        }

        public ActionResult ChangePassword()
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            var user = new UserChangePassword();
            user.UserName = Session[DSEConstant.UserName].ToString();
            return View(user);
        }

        [HttpPost]
        public ActionResult ChangePassword(UserChangePassword user)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "User");

            ViewBag.Roles = _roleFactory.GetSelectListRole();
            ViewBag.Clients = _clientFactory.GetSelectListClient();
            ViewBag.Edit = true;

            ModelState.Remove("Updated");

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user = _userFactory.ChangePassword(user);

            switch (user.Updated)
            {
                case 0:
                    ModelState.AddModelError("", "Invalid username or password");
                    break;
                case 1:
                    ModelState.AddModelError("", "New pasword can not be the same as the old password");
                    break;
                case 2:
                    ModelState.AddModelError("", "New password and confirm new password must be the same");
                    break;
            }

            return View(user);
        }


    }
}