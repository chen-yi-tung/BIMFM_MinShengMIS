using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinSheng_MIS.Controllers
{
    public class MaintainItem_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        int TotalNo_AsBuilt = 0;

        #region 保養項目管理
        public ActionResult Management()
        {
            MaintainItemViewModel maintainItemViewModel = new MaintainItemViewModel();

            #region 下拉選單列出所有系統別
            var SystemList = db.EquipmentInfo
                .GroupBy(s => s.System, (key, items) => new SystemItems
                { SystemName = key })
                .ToList();

            if (SystemList == null)
            {
                return HttpNotFound();
            }

            maintainItemViewModel.SystemItemList = SystemList;
            #endregion

            #region 下拉選單列出所有設備資訊
            var MVM = new MaintainItemViewModel();
            var equipmentList = db.EquipmentInfo
                .OrderBy(e => e.ESN)
                .ToList();
            if (equipmentList == null)
            {
                return HttpNotFound();
            }

            if (equipmentList.Count() > 0)
            {
                MVM.EquipmentInfo = new List<EquipmentInfo>();
                foreach (var item in equipmentList)
                {
                    MVM.EquipmentInfo.Add(new EquipmentInfo { ESN = item.ESN, System = item.System, SubSystem = item.SubSystem, EName = item.EName, Area = item.Area, Floor = item.Floor, Room = item.Room, RoomName = item.RoomName });
                }
            }

            var equipmentItemJson = JsonConvert.SerializeObject(MVM, Formatting.Indented);

            maintainItemViewModel.EquipmentInfoList = equipmentItemJson;
            #endregion


            #region 檢索
            MaintainItemQuery query = new MaintainItemQuery();

            //初始狀態，查詢欄位為空值
            query.System = null;
            query.SubSystem = null;
            query.EName = null;
            query.Unit = null;
            query.Period = null;
            query.MaintainItemIsEnable = null;
            //依照前面的查詢參數，建立JSON格式之查詢參數字串(不包含下拉式選單選項表列)
            string querystr = JsonConvert.SerializeObject(query, Formatting.Indented);
            maintainItemViewModel.QueryStr = querystr;

            #endregion
            return View(maintainItemViewModel);
        }

        [HttpPost]
        public ActionResult VerifyField(MaintainItemQuery queryViewModel)
        {
            JsonResponseViewModel model = new JsonResponseViewModel();

            try
            {
                //驗證欄位，視需求調整
                if (queryViewModel.System == "none")
                    queryViewModel.System = null;
                if (queryViewModel.SubSystem == "none")
                    queryViewModel.SubSystem = null;
                if (queryViewModel.EName == "none")
                    queryViewModel.EName = null;
                if (!string.IsNullOrEmpty(queryViewModel.Period))
                {
                    int PP = Convert.ToInt16(queryViewModel.Period);
                }
                //if (queryViewModel.System == "none" || queryViewModel.SubSystem == "none")
                //{
                //    model.ResponseCode = 1;
                //    model.ResponseMessage = "請選擇必填的檢索欄位";
                //    return Json(model);
                //}

                string querystr = JsonConvert.SerializeObject(queryViewModel, Formatting.Indented);
                model.QueryStr = querystr;
                model.ResponseCode = 0;
                model.ResponseMessage = "已完成檢索";
            }
            catch(System.OverflowException)
            {
                model.ResponseCode = 1;
                model.ResponseMessage = "保養週期必須小於或等於 32767";

            }
            catch (Exception)
            {
                model.ResponseCode = 2;
                model.ResponseMessage = "檢索未完成";
            }
            return Json(model);
        }

        [HttpPost]
        public ActionResult GetGridJson(string Query, int page = 1, int rows = 1, string sort = "MISN", string order = "asc")
        {
            //重點:Easy UI 回送時(包括Paging)必定包含:total & rows 二部分，且名稱全為小寫，寫成Total則不會接受!!

            JObject result = new JObject();

            result.Add("rows", GetJsonForGrid_AsBuilt(Query, page, rows, sort, order));
            result.Add("total", TotalCount_AsBuilt());

            return Content(JsonConvert.SerializeObject(result), "application/json");

        }

        public JArray GetJsonForGrid_AsBuilt(string Query, int page, int pageSize, string propertyName = "Date", string order = "desc")
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
            if(!string.IsNullOrEmpty(Target.MIName))
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
            TotalNo_AsBuilt = table.Count();

            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            //table = table.Skip((page - 1) * pageSize).Take(pageSize);

            if (table != null && TotalNo_AsBuilt > 0)
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

        public int TotalCount_AsBuilt()
        {
            return TotalNo_AsBuilt;
        }
        #endregion

        #region 新增保養項目
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 查詢保養項目 (詳情)
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯保養項目
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除保養項目
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}