using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MinSheng_MIS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Newtonsoft.Json.Linq;

namespace MinSheng_MIS.Attributes
{
    public class JWTAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (CheckAuthorization(actionContext.Request.Headers.Authorization?.ToString()))
            {
                return true;
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new JObject
            {
                { "State" , "Failed" },
                { "ErrorMessage" , "Invalid Token" },
                { "Datas" , null }
            });
        }

        private bool CheckAuthorization(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false;
                token = token.Substring("Bearer ".Length);

                var tokenHandler = new JwtSecurityTokenHandler();
                if (string.IsNullOrEmpty(JWTKey.Key)) JWTKey.Key = JWTKey.GenerateKey();
                var key = Encoding.ASCII.GetBytes(JWTKey.Key); // 設定金鑰
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // 是否驗證發行者
                    ValidateAudience = false, // 是否驗證接收者
                    ValidateLifetime = true, // 是否驗證有效期
                    ValidateIssuerSigningKey = true, // 是否驗證簽章金鑰

                    IssuerSigningKey = new SymmetricSecurityKey(key), // 設定簽章金鑰
                };
                // Validate and decode the JWT token
                HttpContext.Current.User = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                if (HttpContext.Current.Request.UserHostAddress != ((ClaimsIdentity)HttpContext.Current.User.Identity).FindFirst("ipAddress").ToString().Substring("ipAddress: ".Length))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}