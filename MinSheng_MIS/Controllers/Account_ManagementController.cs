using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;

namespace MinSheng_MIS.Controllers
{
    public class Account_ManagementController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
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
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        // GET: Account_Management
        #region 帳號管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增帳號
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create_Add(FormCollection form)
        {
            if (form["UserName"].IsNullOrWhiteSpace()) { return Content("帳號為必填欄位!", "application/json"); }
            if (form["UserPassword"].IsNullOrWhiteSpace()) { return Content("密碼為必填欄位!", "application/json"); }
            if (form["UserPWR"].IsNullOrWhiteSpace()) { return Content("確認密碼為必填欄位!", "application/json"); }
            if (form["MyName"].IsNullOrWhiteSpace()) { return Content("姓名為必填欄位!", "application/json"); }
            if (form["Authority"].IsNullOrWhiteSpace()) { return Content("權限為必填欄位!", "application/json"); }
            if (form["Email"].IsNullOrWhiteSpace()) { return Content("信箱為必填欄位!", "application/json"); }

            //密碼與確認密碼是否一致
            if (form["UserPassword"].ToString() != form["UserPWR"].ToString())
            {
                return Content("密碼與確認密碼不一致!", "application/json");
            }

            //檢查帳號有沒有重複

            //新增帳號
            string Pd = form["UserPassword"].ToString();
            var user = new ApplicationUser()
            {
                UserName = form["UserName"].ToString(),
                MyName = form["MyName"].ToString(),
                Authority = form["Authority"].ToString(),
                Email = form["Email"].ToString(),
                PhoneNumber = form["PhoneNumber"].ToString(),
                Apartment = form["Apartment"].ToString(),
                Title = form["Title"].ToString(),
                IsEnabled = true
            };
            var result = await UserManager.CreateAsync(user, Pd);
            if (result.Succeeded)
            {
                string ResultString = JsonConvert.SerializeObject("新增成功!");
                return Content(ResultString, "application/json");
            }
            else
            {
                string ResultString = JsonConvert.SerializeObject(result.Errors);
                return Content(ResultString, "application/json");
            }
        }
        #endregion

        #region 編輯帳號
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除帳號
        public ActionResult Delete()
        {
            return View();
        }
        #endregion

        #region 變更密碼
        public ActionResult ChangePassword()
        {
            return View();
        }
        #endregion
    }
}