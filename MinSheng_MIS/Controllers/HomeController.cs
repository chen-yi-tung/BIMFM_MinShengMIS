using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MinSheng_MIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;

        public HomeController()
        {
        }

        public HomeController(ApplicationSignInManager signInManager)
        {
            SignInManager = signInManager;
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

        public ActionResult Index()
        {
            return View();
        }

        #region 登入
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(FormCollection form)
        {
            var result = await SignInManager.PasswordSignInAsync(form["UserID"], form["UserPW"], isPersistent: false, shouldLockout: false);
            if (result == SignInStatus.Success)
            {
                return RedirectToAction("Management", "InspectionPlan_Management");
            }
            return Content("登入嘗試失敗!");
        }
        #endregion
    }
}