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
            string responsemessage = form["UserName"].IsNullOrWhiteSpace() ? "帳號為必填欄位!" : string.Empty;
            responsemessage += form["UserPassword"].IsNullOrWhiteSpace() ? "密碼為必填欄位!" : string.Empty;
            responsemessage += form["UserPWR"].IsNullOrWhiteSpace() ? "密碼與確認密碼不一致!" : string.Empty;
            responsemessage += form["MyName"].IsNullOrWhiteSpace() ? "姓名為必填欄位!" : string.Empty;
            responsemessage += form["Authority"].IsNullOrWhiteSpace() ? "權限為必填欄位!" : string.Empty;
            responsemessage += form["Email"].IsNullOrWhiteSpace() ? "信箱為必填欄位!" : string.Empty;
            if(responsemessage != string.Empty)
            {
                Response.StatusCode = 400;
                return Content(responsemessage);
            }
            #endregion

            //檢查帳號有沒有重複

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
                    JObject jo = new JObject
                    {
                        { "Succeed", true }
                    };
                    Response.StatusCode = 201;
                    string ResultString = JsonConvert.SerializeObject(jo);
                    return Content(ResultString, "application/json");
                }
                else
                {
                    JObject jo = new JObject
                    {
                        { "Succeed", false },
                        { "ErrorMessage",string.Join( ",", result.Errors.ToList() )}
                    };
                    Response.StatusCode = 500;
                    string ResultString = JsonConvert.SerializeObject(jo);
                    return Content(ResultString, "application/json");
                }
            }
            catch (Exception ex)
            {
                JObject jo = new JObject
                {
                    { "Succeed", false },
                    { "ErrorMessage",string.Join( ",", ex.Message )}
                };
                Response.StatusCode = 500;
                string ResultString = JsonConvert.SerializeObject(jo);
                return Content(ResultString, "application/json");
            }
        }
        #endregion

        #region 編輯帳號
        public ActionResult Edit()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Edit_LoadData(string id)
        {
            AccountData accountData = new AccountData();
            var data = accountData.GetCurAccountData(id);
            string result = JsonConvert.SerializeObject(data);
            return Content(result, "application/json");
        }
        public async Task<ActionResult> Edit_SaveData(FormCollection form)
        {
            #region 判斷資料是否為空
            string responsemessage = form["UserName"].IsNullOrWhiteSpace() ? "需要提供使用者帳號!!" : string.Empty;
            responsemessage += form["MyName"].IsNullOrWhiteSpace() ? "姓名為必填欄位!" : string.Empty;
            responsemessage += form["Authority"].IsNullOrWhiteSpace() ? "權限為必填欄位!" : string.Empty;
            responsemessage += form["Email"].IsNullOrWhiteSpace() ? "信箱為必填欄位!" : string.Empty;
            if (responsemessage != string.Empty)
            {
                Response.StatusCode = 400;
                return Content(responsemessage);
            }
            #endregion
            try
            {
                var getData = await UserManager.FindByIdAsync(form["UserName"].ToString());
                if (getData != null)
                {
                    getData.MyName = form["MyName"].ToString();
                    getData.Authority = form["Authority"].ToString();
                    getData.Email = form["Email"].ToString();
                    getData.PhoneNumber = form["PhoneNumber"].ToString();
                    getData.Apartment = form["Apartment"].ToString();
                    getData.Title = form["Title"].ToString();
                    var result = await UserManager.UpdateAsync(getData);
                    if (result.Succeeded)
                    {
                        Response.StatusCode = 200;
                        return Content("編輯成功!");
                    }
                    else
                    {
                        Response.StatusCode = 500;
                        return Content("編輯失敗!");
                    }
                }
                else
                {
                    Response.StatusCode = 500;
                    return Content("無此使用者!");
                }
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return Content("更新過程出錯!");
            }
        }
        #endregion

        #region 刪除帳號
        public ActionResult Delete()
        {
            return View();
        }
        [HttpGet]
        public async Task<string> Delete_Account(string id) //把IsEnable轉為0就是不啟用此帳號了
        {
            var getData = await UserManager.FindByIdAsync(id);
            if (getData != null)
            {
                getData.IsEnabled = false;
                var result = await UserManager.UpdateAsync(getData);
                if (result.Succeeded)
                {
                    return "刪除成功!";
                }
                else
                {
                    return "刪除失敗!";
                }
            }
            else
            {
                return "無此使用者!";
            }
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