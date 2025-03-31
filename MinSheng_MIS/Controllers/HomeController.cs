using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using MinSheng_MIS.Services.Helpers;


namespace MinSheng_MIS.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public HomeController()
        {
        }

        public HomeController(ApplicationSignInManager signInManager, ApplicationUserManager userManager)
        {
            SignInManager = signInManager;
            UserManager = userManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        #region 登入
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(FormCollection form)
        {
            string account = form["UserID"];
            string password = form["UserPW"];
            string controllerName = "AccountController";

            var userdata = UserManager.Users.Where(x => x.IsEnabled == true).Where(x => x.UserName == account && x.Authority != "4").FirstOrDefault();
            if (userdata != null)
            {
                var result = await SignInManager.PasswordSignInAsync(account, password, isPersistent: false, shouldLockout: false);
                if (result == SignInStatus.Success)
                {
                    Session["MyName"] = userdata.MyName;
                    LogHelper.WriteLoginLog(this, account, "後台登入成功!");
                    return RedirectToAction("Index", "PlanManagement");
                }
            }
            LogHelper.WriteLoginLog(this, account, "後台登入嘗試失敗!");
            ViewBag.Message = "登入嘗試失敗!";
            return View();
        }
        #endregion

        #region 登出
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Home");
        }
        #endregion
    }
}