using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinSheng_MIS.Models.ViewModels
{
    /// <summary>
    /// DataGrid參數
    /// </summary>
    public abstract class GridParams
    {
        [JsonProperty("page")]
        public int Page { get; set; } = 1; //頁數
        [JsonProperty("rows")]
        public int Rows { get; set; } = 10; //每頁筆數
        [JsonProperty("sort")]
        public string Sort { get; set; } //排列
        [JsonProperty("order")]
        public string Order { get; set; } //順序(asc/desc)
    }

    /// <summary>
    /// DataGrid查詢結果回傳
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridResult<T>
    {
        [JsonProperty("total")]
        public string Total { get; set; } = "0"; // 總筆數
        [JsonProperty("rows")]
        public IEnumerable<T> Rows { get; set; } // 內容
    }
}