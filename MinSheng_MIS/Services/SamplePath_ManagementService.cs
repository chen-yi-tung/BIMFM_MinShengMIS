using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace MinSheng_MIS.Services
{
    public class SamplePath_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;

        public SamplePath_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _rfidService = new RFIDService(_db);
        }

        #region 批次刪除巡檢預設順序 InspectionDefaultOrder
        /// <summary>
        /// 批次刪除巡檢預設順序內的特定設備
        /// </summary>
        /// <param name="esn">特定設備列表</param>
        public void DeleteEquipmentInInspectionPathSample(IEnumerable<string> esnList)
        {
            var inspectItems = esnList
                .SelectMany(e => _rfidService.GetRFIDsByEsn(e))
                .SelectMany(x => x.InspectionDefaultOrder)
                .Distinct();

            _db.InspectionDefaultOrder.RemoveRange(inspectItems);
        }
        #endregion

        #region 欲新增的設備
        public JObject GetJsonForGrid_DailyInspectionSample(System.Web.Mvc.FormCollection form)
        {
            #region datagrid呼叫時的預設參數有 rows 跟 page
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString()))
            {
                page = short.Parse(form["page"].ToString());
            }
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
            {
                rows = short.Parse(form["rows"]?.ToString());
            }
            #endregion

            #region 塞來自formdata的資料
            //RFID內碼

            //RFID外碼

            //設備名稱

            //設備編號

            //棟別

            //樓層

            //廠牌

            //型號

            //巡檢頻率

            //已選清單

            string PathName = form["PathName"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = db.InspectionPathSample.AsQueryable();

            if (!string.IsNullOrEmpty(PathName)) //查詢巡檢路線名稱模糊查詢
            {
                SourceTable = SourceTable.Where(x => x.PathName.Contains(PathName));
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order  
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderBy(x => x.PathName);
            }
            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);
            var dic_frequency = Surface.InspectionPlanFrequency();
            foreach (var a in SourceTable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("PlanPathSN", a.PlanPathSN);//巡檢路線編號
                itemObjects.Add("PathName", a.PathName);//巡檢路線名稱
                itemObjects.Add("Frequency", dic_frequency[a.Frequency.ToString()]);//巡檢頻率
                itemObjects.Add("InspectionNum", db.InspectionDefaultOrder.Where(x => x.PlanPathSN == a.PlanPathSN).Count());//巡檢數量
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion
    }
}