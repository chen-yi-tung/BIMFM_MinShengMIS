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
using System.Linq.Expressions;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Results;

namespace MinSheng_MIS.Services
{
    public static class Helper
    {
        #region InvalidModelState/ Exception回傳
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

        public static IHttpActionResult HandleInvalidModelState(ApiController controller, string field = null)
        {
            var errorMsg = string.IsNullOrEmpty(field) ?
                string.Join(Environment.NewLine, controller.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).Distinct()) :
                string.Join(Environment.NewLine, controller.ModelState[field].Errors.Select(e => e.ErrorMessage).Distinct());

            return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<JsonResService<string>>(new JsonResService<string>
                {
                    AccessState = ResState.Failed,
                    ErrorMessage = errorMsg,
                    Datas = null,
                }, new JsonMediaTypeFormatter())
            });
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

        public static IHttpActionResult HandleMyCusResException(ApiController controller, MyCusResException error)
        {
            return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<JsonResService<string>>(new JsonResService<string>
                {
                    AccessState = ResState.Failed,
                    ErrorMessage = error.Message,
                    Datas = null,
                }, new JsonMediaTypeFormatter())
            });
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
                    ErrorMessage = $"系統異常！",
                    Datas = null
                }),
                ContentType = "application/json"
            };
        }

        public static IHttpActionResult HandleException(ApiController controller)
        {
            return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<JsonResService<string>>(new JsonResService<string>
                {
                    AccessState = ResState.Failed,
                    ErrorMessage = $"</br>系統異常！",
                    Datas = null,
                }, new JsonMediaTypeFormatter())
            });
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
        #endregion

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

        #region 以DTO抓取資料庫資料(DTO屬性需與資料庫欄位同名)
        // 直接使用表達式映射
        public static List<TDestination> MapDatabaseToDto<TSource, TDestination>(IEnumerable<TSource> source)
            where TDestination : new()
        {
            var mapExpression = MapDatabaseToQuery<TSource, TDestination>();
            var mapFunc = mapExpression.Compile();

            return source.Select(mapFunc).ToList();
        }

        // 定義映射表達式
        public static Expression<Func<TSource, TDestination>> MapDatabaseToQuery<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceProperties = typeof(TSource).GetProperties();
            var destinationProperties = typeof(TDestination).GetProperties();

            var parameter = Expression.Parameter(typeof(TSource), "x");
            var bindings = destinationProperties
                .Where(d => sourceProperties.Any(s => s.Name == d.Name && s.PropertyType == d.PropertyType))
                .Select(d =>
                    Expression.Bind(
                        d,
                        Expression.Property(parameter, sourceProperties.First(s => s.Name == d.Name && s.PropertyType == d.PropertyType))
                    ))
                .ToArray();

            return Expression.Lambda<Func<TSource, TDestination>>(
                Expression.MemberInit(
                    Expression.New(typeof(TDestination)),
                    bindings
                ),
                parameter
            );
        }
        #endregion

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

        public static ICollection<T> AddOrUpdateList<T, TSource>(
            IEnumerable<TSource> list,
            string sn,
            string initialLatestId,
            string format,
            int emptySnLength,
            Func<string, int, string, string, string> generateIdFunc,  // 修改為新的 generateIdFunc 簽名
            Func<TSource, string, string, T> createInstance)
        {
            string latestId = initialLatestId;
            var results = new List<T>();

            foreach (var item in list)
            {
                // 生成Id，傳遞格式、空位長度、最新Id 和 SN
                string newId = generateIdFunc(format, emptySnLength, latestId, sn);

                // 創建實例，傳遞當前 Id 和 Id 生成函數
                var result = createInstance(item, sn, newId);
                results.Add(result);

                // 更新 latestId
                latestId = newId;
            }

            return results;
        }

        public static ICollection<T> AddOrUpdateList<T, TSource>(
            IEnumerable<TSource> list,
            string sn,
            string initialLatestId,
            string format,
            int emptySnLength,
            string[] param,
            Func<string, int, string, string, string> generateIdFunc,  // 修改為新的 generateIdFunc 簽名
            Func<TSource, string[], string, T> createInstance)
        {
            string latestId = initialLatestId;
            var results = new List<T>();

            foreach (var item in list)
            {
                // 生成Id，傳遞格式、空位長度、最新Id 和 SN
                string newId = generateIdFunc(format, emptySnLength, latestId, sn);

                // 創建實例，傳遞當前 Id 和 Id 生成函數
                var result = createInstance(item, param, newId);
                results.Add(result);

                // 更新 latestId
                latestId = newId;
            }

            return results;
        }

        /// <summary>
        /// 比較2個List忽略排序後的內容是否完全相等
        /// </summary>
        /// <typeparam name="T">IEnumerable類型</typeparam>
        /// <param name="list1">List1</param>
        /// <param name="list2">List2</param>
        /// <returns></returns>
        public static bool AreListsEqualIgnoreOrder<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null && list2 == null)
                return true;

            if (list1 == null || list2 == null || list1.Count() != list2.Count())
                return false;

            // 排序後比較
            return list1.OrderBy(x => x).SequenceEqual(list2.OrderBy(x => x));
        }

    }
}