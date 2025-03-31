using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MinSheng_MIS.Services.Helpers;

namespace MinSheng_MIS.Controllers.API
{
    [RoutePrefix("api")]
    public class AccountController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private readonly Bimfm_MinSheng_MISEntities _db;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public AccountController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
        }

        [AllowAnonymous]
        [Route("Login")]
        public async Task<JObject> Login([FromBody] AppLoginViewModel user)
        {
            string loginResult = "巡檢APP登入嘗試失敗!"; // 預設登入失敗
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {

                var result = SignInManager.PasswordSignIn(user.Account, user.Password, isPersistent: false, shouldLockout: false);
                if (result == SignInStatus.Success)
                {
                    jo["Datas"] = JWTToken(user.Account);
                    loginResult = "巡檢APP登入成功!";
                    // 紀錄該使用者已登入
                    if (!_db.LoginUserList.Any(x => x.UserID == user.Account))
                    {
                        _db.LoginUserList.Add(new LoginUserList { UserID = user.Account });
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    jo["Datas"] = "登入失敗";
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            // 記錄登入 Log
            LogHelper.WriteLoginLog("AccountController", user.Account, loginResult);
            return jo;
        }

        private string JWTToken(string userName)
        {
            string ipAddress = "";
            if (Request.Properties.ContainsKey("MS_HttpContext"))
            {
                var context = Request.Properties["MS_HttpContext"] as HttpContextBase;
                if (context != null)
                {
                    ipAddress = context.Request.UserHostAddress;
                }
            }
            // 定義加密金鑰
            if (string.IsNullOrEmpty(JWTKey.Key)) JWTKey.Key = JWTKey.GenerateKey();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTKey.Key));

            // 建立 Token
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddMinutes(30),
                claims: new[] { new Claim(nameof(ipAddress), ipAddress), new Claim(ClaimTypes.Name, userName) },
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            // 將 Token 轉換為字串格式
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        [Route("Logout")]
        public async Task<JObject> Logout()
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                var userName = HttpContext.Current.User.Identity.Name;

                // 刪除該使用者已登入紀錄
                var user = _db.LoginUserList.FirstOrDefault(x => x.UserID == userName);
                _db.LoginUserList.Remove(user);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
                jo["Datas"] = "登出失敗";
            }
            return jo;
        }
    }

    public class ChangePasswordController : ApiController
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public JObject Post([FromBody] AppChangePasswordViewModel user)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                if (user.NewPassword != user.CheckPassword)
                {
                    jo["ErrorMessage"] = "新密碼與再次輸入新密碼不相符。";
                }
                var userName = HttpContext.Current.User.Identity.Name;
                var appUser = UserManager.Find(userName, user.OldPassword);
                if (appUser != null && appUser.Authority == "4")
                {
                    var token = UserManager.GeneratePasswordResetToken(appUser.Id);
                    var result = UserManager.ResetPassword(appUser.Id, token, user.NewPassword);
                    if (!result.Succeeded)
                    {
                        jo["ErrorMessage"] = string.Join(",", result.Errors);
                    }
                }
                else
                {
                    jo["ErrorMessage"] = "舊密碼輸入錯誤。";
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class UserInfoController : ApiController
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public JObject Get()
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                var userName = HttpContext.Current.User.Identity.Name;
                var appUser = UserManager.FindByName(userName);
                if (appUser != null)
                {
                    JObject itemObject = new JObject();
                    itemObject.Add("MyName", appUser.MyName);
                    itemObject.Add("UserName", appUser.UserName);
                    itemObject.Add("Email", appUser.Email);
                    itemObject.Add("PhoneNumber", appUser.PhoneNumber);
                    itemObject.Add("Apartment", appUser.Apartment);
                    itemObject.Add("Title", appUser.Title);
                    jo["Datas"] = itemObject;
                }
                else
                {
                    jo["ErrorMessage"] = "帳號異常。";
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
}