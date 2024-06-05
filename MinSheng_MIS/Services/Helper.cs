using MinSheng_MIS.Models.ViewModels;
using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// 將錯誤訊息清單改寫成html
        /// </summary>
        /// <param name="list">錯誤訊息清單</param>
        /// <param name="errorTitle">錯誤訊息主題</param>
        /// <returns></returns>
        public static string HandleErrorMessageList(List<string> list, string errorTitle = null)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(errorTitle)) result += html_newLine + errorTitle;
            // html列表
            if (!list.Any()) return null;
            result += "<ul>";
            foreach (string error in list)
                result += $"<li>{error}</li>";
            result += "</ul>";
            return result;
        }

        /// <summary>
        /// 讀取伺服器內的Json檔案
        /// </summary>
        /// <param name="ser">伺服器位址取得</param>
        /// <param name="path">檔案相對根目錄的路徑(ex:"~/Content/...")</param>
        /// <returns>JObject</returns>
        public static JObject ReadJsonFile(HttpServerUtilityBase ser, string path)
        {
            string fullpath = ser.MapPath(path); //取得檔案在Server上的實體路徑
            string content = System.IO.File.ReadAllText(fullpath);
            JObject jo = JObject.Parse(content);
            return jo;
        }
    }
}