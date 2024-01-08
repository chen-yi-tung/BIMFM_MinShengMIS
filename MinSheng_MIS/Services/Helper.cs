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


    }
}