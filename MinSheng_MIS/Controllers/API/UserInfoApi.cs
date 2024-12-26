using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;

namespace MinSheng_MIS.Controllers.API
{
    public class LoginController : ApiController
    {
        private ApplicationSignInManager _signInManager;

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

        [AllowAnonymous]
        public JObject Post([FromBody] AppLoginViewModel user)
        {
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
                claims: new[] { new Claim(nameof(ipAddress), ipAddress), new Claim(nameof(userName), userName) },
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            // 將 Token 轉換為字串格式
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }
    }
}