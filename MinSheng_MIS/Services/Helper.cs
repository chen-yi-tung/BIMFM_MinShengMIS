using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Services
{
    public static class Helper
    {
        private static string html_newLine = "<br>";
        /// <summary>
        /// 未通過Data Annotaion的錯誤
        /// </summary>
        /// <param name="controller">呼叫的controller</param>
        /// <param name="field">指定的資料驗證欄位，若不指定則不填寫</param>
        /// <returns></returns>
        public static ActionResult HandleInvalidModelState(Controller controller, string field = null)
        {
            controller.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            if (string.IsNullOrEmpty(field))
                return new ContentResult { 
                    Content = string.Join(Environment.NewLine, controller.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).Distinct()), 
                    ContentType = "text/plain" 
                };
            else
                return new ContentResult
                {
                    Content = string.Join(Environment.NewLine, controller.ModelState[field].Errors.Select(e => e.ErrorMessage).Distinct()),
                    ContentType = "text/plain"
                };
        }

        public static string HandleErrorMessageList(List<string> list, string errorType = null)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(errorType)) result += html_newLine + errorType;
            // html列表
            if (!list.Any()) return null;
            result += "<ul>";
            foreach (string error in list)
                result += $"<li>{error}</li>";
            result += "</ul>";
            return result;
        }
    }
}