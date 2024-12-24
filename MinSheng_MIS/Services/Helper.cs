using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Data;
using MinSheng_MIS.Models;

namespace MinSheng_MIS.Services
{
    public static class Helper
    {
        /// <summary>
        /// 未通過Data Annotaion的錯誤
        /// </summary>
        /// <param name="controller">呼叫的controller</param>
        /// <param name="field">指定的資料驗證欄位，若不指定則不填寫</param>
        /// <param name="applyFormat">是否套用<see cref="JsonResService{String}"/>的回傳格式，預設為否</param>
        /// <returns></returns>
        public static ActionResult HandleInvalidModelState(Controller controller, string field = null, bool applyFormat = false)
        {
            var errorMsg = string.IsNullOrEmpty(field) ?
                string.Join(Environment.NewLine, controller.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).Distinct()) :
                string.Join(Environment.NewLine, controller.ModelState[field].Errors.Select(e => e.ErrorMessage).Distinct());

            if (applyFormat)
            {
                controller.Response.StatusCode = (int)HttpStatusCode.OK;
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject(new JsonResService<string>
                        {
                            AccessState = ResState.Failed,
                            ErrorMessage = errorMsg,
                            Datas = null,
                        }),
                    ContentType = "application/json"
                };
            }
            else
            {
                controller.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new ContentResult
                {
                    Content = "</br>" + errorMsg,
                    ContentType = "text/plain"
                };
            }
        }

        /// <summary>
        /// <see cref="MyCusResException"/>的錯誤訊息回傳
        /// </summary>
        /// <param name="controller">呼叫的controller</param>
        /// <param name="error">例外錯誤</param>
        /// <returns>套用<see cref="JsonResService{String}"/>的回傳格式</returns>
        public static ActionResult HandleMyCusResException(Controller controller, MyCusResException error)
        {
            controller.Response.StatusCode = (int)HttpStatusCode.OK;
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new JsonResService<string>
                    {
                        AccessState = ResState.Failed,
                        ErrorMessage = $"</br>{error.Message}",
                        Datas = null
                    }),
                ContentType = "application/json"
            };
        }

        /// <summary>
        /// <see cref="Exception"/>的錯誤訊息回傳
        /// </summary>
        /// <param name="controller">呼叫的controller</param>
        /// <returns>套用<see cref="JsonResService{String}"/>的回傳格式</returns>
        public static ActionResult HandleException(Controller controller)
        {
            controller.Response.StatusCode = (int)HttpStatusCode.OK;
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new JsonResService<string>
                {
                    AccessState = ResState.Failed,
                    ErrorMessage = $"</br>系統異常!",
                    Datas = null
                }),
                ContentType = "application/json"
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
            if (!string.IsNullOrEmpty(errorTitle)) result += "</br>" + errorTitle;
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

        public static ICollection<T> AddOrUpdateList<T, TSource>(
            IEnumerable<TSource> list,
            string mainSn,
            Func<TSource, string, T> createInstance) where T : new()
        {
            return list.Select(x => createInstance(x, mainSn)).ToList();
        }

        public static ICollection<T> AddOrUpdateList<T, TSource>(
            IEnumerable<TSource> list,
            Func<TSource, T> createInstance) where T : new()
        {
            return list.Select(x => createInstance(x)).ToList();
        }

        public static ICollection<T> AddOrUpdateList<T, TSource>(
            IEnumerable<TSource> list,
            string esn,
            string initialLatestId,
            Func<string, string, string> generateIdFunc,
            Func<TSource, string, string, string, T> createInstance)
        {
            string latestId = initialLatestId;
            var results = new List<T>();

            foreach (var item in list)
            {
                // 生成Id
                string newId = generateIdFunc(esn, latestId);

                // 創建實例，傳遞當前 Id 和 Id 生成函數
                var result = createInstance(item, esn, latestId, newId);
                results.Add(result);

                // 更新 latestId
                latestId = newId;
            }

            return results;
        }

        #region DTO轉換
        public static TDestination ToDto<TSource, TDestination>(this TSource source)
            where TSource : class
            where TDestination : class, new()  // TDestination 必須是具體類型
        {
            var destination = new TDestination();  // 創建具體類型的實例

            // 遍歷源對象的屬性，並將它們映射到目標對象
            foreach (var sourceProp in typeof(TSource).GetProperties())
            {
                var destProp = typeof(TDestination).GetProperty(sourceProp.Name);
                if (destProp != null 
                    && destProp.CanWrite
                    && destProp.PropertyType == sourceProp.PropertyType) // 類型必須相同
                {
                    destProp.SetValue(destination, sourceProp.GetValue(source));
                }
            }

            return destination;
        }
        #endregion
    }
}