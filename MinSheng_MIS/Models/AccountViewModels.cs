using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Xml.Linq;

namespace MinSheng_MIS.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "代碼")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "記住此瀏覽器?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "電子郵件")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [Display(Name = "記住我?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 的長度至少必須為 {2} 個字元。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不相符。")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 的長度至少必須為 {2} 個字元。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不相符。")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }
    }

    public class AccountData 
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        public JObject GetCurAccountData(string UserName,bool dicConvert)
        { 
            var data = db.AspNetUsers.Where(x => x.UserName == UserName).FirstOrDefault();
            var dic = Surfaces.Surface.Authority();
            if (data != null)
            {
                string Authority = dicConvert ? dic[data.Authority] : data.Authority;
                JObject jo = new JObject();
                jo.Add("UserName", data.UserName);
                jo.Add("MyName", data.MyName);
                jo.Add("Authority", Authority);
                jo.Add("Email", data.Email);
                jo.Add("PhoneNumber", data.PhoneNumber);
                jo.Add("Apartment", data.Apartment);
                jo.Add("Title", data.Title);
                return jo;
            }
            else
            {
                return null;
            }
        }

        public string UpdateUserData(System.Web.Mvc.FormCollection form)
        {
            try
            {
                string UserName = form["UserName"].ToString();
                var data = db.AspNetUsers.Where(x => x.UserName == UserName).FirstOrDefault();
                if (data != null)
                {
                    data.MyName = form["MyName"].ToString();
                    data.Authority = form["Authority"].ToString();
                    data.Email = form["Email"].ToString();
                    data.PhoneNumber = form["PhoneNumber"].ToString();
                    data.Apartment = form["Apartment"].ToString();
                    data.Title = form["Title"].ToString();
                    db.AspNetUsers.AddOrUpdate(data);
                    db.SaveChanges();
                    return "200";
                }
                else
                {
                    return "400";
                }
            }
            catch (DbEntityValidationException ex)
            {
                var entityError = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
                var getFullMessage = string.Join("; ", entityError);
                //var exceptionMessage = string.Concat(ex.Message, "errors are: ", getFullMessage);
                return getFullMessage;
            }
        }

        public string DeleteAccount(string username)
        {
            try
            {
                var data = db.AspNetUsers.Where(x => x.UserName == username).FirstOrDefault();
                if (data != null)
                {
                    data.IsEnabled = false;
                    db.AspNetUsers.AddOrUpdate(data);
                    db.SaveChanges();
                    return "200"; //刪除成功
                }
                else
                {
                    return "400"; //無此使用者
                }
            }
            catch (Exception ex) 
            { 
                return ex.Message; 
            }
        }
    }
}
