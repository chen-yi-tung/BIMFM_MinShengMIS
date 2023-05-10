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
using System.Web.Http.Results;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data.Entity.Validation;
using static MinSheng_MIS.Controllers.ManageController;

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
            #region 判斷資料是否為空
            string responsemessage = form["UserName"].IsNullOrWhiteSpace() ? "帳號為必填欄位!\n" : string.Empty;
            responsemessage += form["UserPassword"].IsNullOrWhiteSpace() ? "密碼為必填欄位!\n" : string.Empty;
            responsemessage += form["UserPWR"].IsNullOrWhiteSpace() ? "密碼與確認密碼不一致!\n" : string.Empty;
            responsemessage += form["MyName"].IsNullOrWhiteSpace() ? "姓名為必填欄位!\n" : string.Empty;
            responsemessage += form["Authority"].IsNullOrWhiteSpace() ? "權限為必填欄位!\n" : string.Empty;
            responsemessage += form["Email"].IsNullOrWhiteSpace() ? "信箱為必填欄位!\n" : string.Empty;
            if(responsemessage != string.Empty)
            {
                Response.StatusCode = 400;
                return Content(responsemessage);
            }
            #endregion

            //新增帳號
            try
            {
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
                    Response.StatusCode = 200;
                    return Content("新增成功!");
                }
                else
                {
                    Response.StatusCode = 500;
                    string message = string.Join(",", result.Errors.ToList());
                    return Content(message);
                }
            }
            catch (Exception ex)
            {
                //JObject jo = new JObject
                //{
                //    { "Succeed", false },
                //    { "ErrorMessage",string.Join( ",", ex.Message )}
                //};
                Response.StatusCode = 500;
                //string ResultString = JsonConvert.SerializeObject(jo);
                return Content(string.Join(",", ex.Message));
            }
        }
        #endregion

        #region 編輯帳號
        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Edit_LoadData(string id)
        {
            AccountData accountData = new AccountData();
            var data = accountData.GetCurAccountData(id, false);
            string result = JsonConvert.SerializeObject(data);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult Edit_SaveData(FormCollection form)
        {
            #region 判斷資料是否為空
            string responsemessage = form["UserName"].IsNullOrWhiteSpace() ? "需要提供使用者帳號!\n" : string.Empty;
            responsemessage += form["MyName"].IsNullOrWhiteSpace() ? "姓名為必填欄位!\n" : string.Empty;
            responsemessage += form["Authority"].IsNullOrWhiteSpace() ? "權限為必填欄位!\n" : string.Empty;
            responsemessage += form["Email"].IsNullOrWhiteSpace() ? "信箱為必填欄位!\n" : string.Empty;
            if (responsemessage != string.Empty)
            {
                Response.StatusCode = 400;
                return Content(responsemessage);
            }
            #endregion

            AccountData accountData = new AccountData();
            int resultCode = accountData.UpdateUserData(form);

            JsonResponseViewModel Jresult = new JsonResponseViewModel()
            {
                ResponseCode = resultCode
            };
            
            switch (resultCode)
            {
                case 200:
                    Jresult.ResponseMessage = "編輯成功!";
                    break;
                case 400:
                    Jresult.ResponseMessage = "無此使用者!";
                    break;
                default: //500
                    Jresult.ResponseMessage = "編輯過程出錯!";
                    break;
            }

            return Content(JsonConvert.SerializeObject(Jresult), "application/json");
        }
        #endregion

        #region 刪除帳號
        [HttpGet]
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Delete_LoadData(string id)
        {
            AccountData accountData = new AccountData();
            var data = accountData.GetCurAccountData(id, true);
            string result = JsonConvert.SerializeObject(data);
            return Content(result, "application/json");
        }
        [HttpDelete]
        public ActionResult Delete_Account(string id)
        {
            AccountData accountData = new AccountData();
            int resultCode = accountData.DeleteAccount(id);

            JsonResponseViewModel Jresult = new JsonResponseViewModel()
            { 
                ResponseCode = resultCode
            };

            switch (resultCode)
            {
                case 200:
                    Jresult.ResponseMessage = "刪除成功!";
                    break;
                case 400:
                    Jresult.ResponseMessage = "無此使用者!";
                    break;
                default: //500
                    Jresult.ResponseMessage = "處理過程出錯!";
                    break;
            }
            return Content(JsonConvert.SerializeObject(Jresult), "application/json");
        }
        #endregion

        #region 變更密碼
        public ActionResult ChangePassword()
        {
            return View();
        }
        //[HttpPost]
        //public async Task<ActionResult> ChangePD_Submit(FormCollection form)
        //{
        //    string old = form["OldPassword"].ToString().Trim();
        //    string newPd = form["NewPassword"].ToString().Trim();
        //    string confirm = form["ConfirmPassword"].ToString().Trim();
        //    try
        //    {
        //        var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), old, newPd);
        //        if (result.Succeeded)
        //        {
        //            return Content("變更成功!");
        //        }
        //        return Content("變更失敗!");
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        var entityError = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
        //        var getFullMessage = string.Join("; ", entityError);
        //        return Content(getFullMessage);
        //    }
        //}
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("ChangePassword", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            return View(model);
        }
        #endregion
    }
}