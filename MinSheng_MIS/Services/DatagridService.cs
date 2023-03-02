using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class DatagridService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        int TotalNo_ReportManagement = 0;
        public JArray GetJsonForGrid_ReportManagement(string Query, int page, int pageSize, string propertyName = "MISN", string order = "asc")
        {
            //說明: 因為JSON字串格式中包含""，Easy UI Post上傳時，會將""轉換為&quot; 無法直接解析回正確的JSON Object，因此需將其轉換為""以利後續處理 
            string QT = Query.Trim();
            QT = QT.Replace("&quot;", "\"");

            #region 解析查詢字串
            var Target = JsonConvert.DeserializeObject<MaintainItemQuery>(QT);
            #endregion

            #region 依據查詢字串檢索資料表
            var table = db.MaintainItem.AsQueryable();
            if (!string.IsNullOrEmpty(Target.System) && Target.System != "none")
                table = table.Where(t => t.System == Target.System);
            if (!string.IsNullOrEmpty(Target.SubSystem) && Target.SubSystem != "none")
                table = table.Where(t => t.SubSystem == Target.SubSystem);
            if (!string.IsNullOrEmpty(Target.EName) && Target.EName != "none")
                table = table.Where(t => t.EName == Target.EName);
            if (!string.IsNullOrEmpty(Target.MIName))
                table = table.Where(t => t.MIName.Contains(Target.MIName));
            if (!string.IsNullOrEmpty(Target.Unit))
                table = table.Where(t => t.Unit == Target.Unit);
            if (!string.IsNullOrEmpty(Target.Period))
            {
                int period = Convert.ToInt16(Target.Period);
                table = table.Where(t => t.Period == period);
            }

            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();

            //注意:"{0} {1}"中間必須為一個空格，以讓系統識別此二參數，注意:必須使用OrderBy，不可使用 OrderByDescent
            //table = table.OrderBy(string.Format("{0} {1}", propertyName, order));
            TotalNo_ReportManagement = table.Count();

            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            //table = table.Skip((page - 1) * pageSize).Take(pageSize);

            if (table != null && TotalNo_ReportManagement > 0)
            {
                foreach (var item in table)
                {
                    var itemObjects = new JObject();
                    if (itemObjects["MISN"] == null)
                        itemObjects.Add("MISN", item.MISN);                                    //保養設備編號

                    if (itemObjects["MIName"] == null)
                        itemObjects.Add("MIName", item.MIName);                                //保養項目名稱

                    if (itemObjects["System"] == null)
                        itemObjects.Add("System", item.System);                                //系統別

                    if (itemObjects["SubSystem"] == null)
                        itemObjects.Add("SubSystem", item.SubSystem);                          //子系統別

                    if (itemObjects["EName"] == null)
                        itemObjects.Add("EName", item.EName);                                  //設備名稱

                    if (itemObjects["Unit"] == null)
                        itemObjects.Add("Unit", item.Unit);                                    //保養週期單位

                    if (itemObjects["Period"] == null)
                        itemObjects.Add("Period", item.Period);                                //保養週期

                    if (itemObjects["MaintainItemIsEnable"] == null)
                        itemObjects.Add("MaintainItemIsEnable", item.MaintainItemIsEnable);    //保養項目停啟用狀態

                    ja.Add(itemObjects);
                }
            }
            return ja;
        }

        public int TotalCount_ReportManagement()
        {
            return TotalNo_ReportManagement;
        }
    }
}