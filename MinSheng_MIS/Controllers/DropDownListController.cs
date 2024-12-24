using Microsoft.Ajax.Utilities;
using Microsoft.Owin.Security.DataHandler.Encoder;
using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace MinSheng_MIS.Controllers
{
    public class DropDownListController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: DropDownList
        #region 棟別
        [System.Web.Http.HttpGet]
        public ActionResult Area()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.AreaInfo.ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.Area);//Area Name
                jo.Add("Value", item.ASN); // ASN 
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 根據樓層查詢模版路徑名稱
        [System.Web.Http.HttpGet]
        public ActionResult PathTitle(string FSN)
        {
            List<JObject> list = new List<JObject>();
            if (FSN != null)
            {
                var abc = db.PathSample.Where(x => x.FSN == FSN).ToList();
                foreach (var item in abc)
                {
                    JObject jo = new JObject();
                    jo.Add("Text", item.PathTitle);// PathTitle
                    jo.Add("Value", item.PSSN); // PSSN
                    list.Add(jo);
                }
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 全部人
        [System.Web.Http.HttpGet]
        public ActionResult AllMyName()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.AspNetUsers.Where(x => x.IsEnabled == true).ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.MyName);// MyName
                jo.Add("Value", item.UserName); // UserName
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 巡檢班別
        [HttpGet]
        public ActionResult Shift()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.Shift();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 根據棟別查詢樓層
        [System.Web.Http.HttpGet]
        public ActionResult Floor(int? ASN)
        {
            List<JObject> list = new List<JObject>();
            if (ASN != null)
            {
                var abc = db.Floor_Info.Where(x => x.ASN == ASN).ToList();
                foreach (var item in abc)
                {
                    JObject jo = new JObject();
                    jo.Add("Text", item.FloorName);//Floor Name
                    jo.Add("Value", item.FSN); // FSN 
                    list.Add(jo);
                }
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 樓層模型名稱 (BIM用)
        [System.Web.Http.HttpGet]
        public ActionResult ViewName(int? ASN)
        {
            List<JObject> list = new List<JObject>();
            var abc = db.Floor_Info.AsQueryable();
            if (ASN != null)
            {
                abc = db.Floor_Info.Where(x => x.ASN == ASN);
            }
            var dl = abc.ToList();
            foreach (var item in dl)
            {
                JObject jo = new JObject();
                jo.Add("Text", $"{item.AreaInfo.Area} {item.FloorName}"); // Area Name Floor Name
                jo.Add("Value", item.ViewName.Trim()); // ViewName
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region PlanState 巡檢計畫狀態
        [HttpGet]
        public ActionResult PlanState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.InspectionPlanState();

            foreach (var a in Dics)
            {
                if (a.Key != "5")
                {
                    JObject jo = new JObject
                    {
                        { "Text", a.Value },
                        { "Value", a.Key }
                    };
                    list.Add(jo);
                }
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 當日巡檢計畫下拉選單
        [HttpGet]
        public ActionResult TodayPlanList()
        {
            List<JObject> list = new List<JObject>();
            var planlist = db.InspectionPlan.Where(x => x.PlanDate == DateTime.Today).ToList();

            foreach (var plan in planlist)
            {
                JObject jo = new JObject
                {
                    { "Text", plan.IPSN },
                    { "Value", plan.IPSN }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region MaintainState相關 保養單狀態
        [HttpGet]
        public ActionResult MaintainRecord_MaintainState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.InspectionPlanMaintainState();

            foreach (var a in Dics)
            {
                if (a.Key == "1" || a.Key == "2") { continue; } //巡檢保養紀錄 不用1. 2
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region ReportFormState相關 報修單狀態
        [System.Web.Http.HttpGet]
        public ActionResult Report_Management_Management_ReportFormState(string url = "")
        {
            List<JObject> list = new List<JObject>();
            var Dics = ReportState(url);

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }


        //根據不同當下路由決定不同的下拉式選單
        private static Dictionary<string, string> ReportState(string url = "")
        {
            //預設空字串回傳全部 key為
            var abc = Surface.EquipmentReportFormState();
            var result = new Dictionary<string, string>();
            if (url == "")
            {
                foreach (var a in abc)
                {
                    result.Add(a.Key, a.Value);
                }
            }
            else if (url == "CanAddToPlanReportState") //新增巡檢計畫->新增報修單 DataGrid 報修單狀態
            {
                foreach (var a in abc)
                {
                    if (a.Key == "1" || a.Key == "5" || a.Key == "8" || a.Key == "9" || a.Key == "10" || a.Key == "11")
                    {
                        result.Add(a.Key, a.Value);
                    }
                }
            }
            return result;
        }

        #endregion

        #region ReportLevel 報修等級
        //[System.Web.Http.HttpGet]
        //public ActionResult ReportLevel()
        //{
        //    List<JObject> list = new List<JObject>();
        //    var Dics = Surface.ReportLevel();

        //    foreach (var a in Dics)
        //    {
        //        JObject jo = new JObject
        //        {
        //            { "Text", a.Value },
        //            { "Value", a.Key }
        //        };
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region InformantUserID 使用者名稱
        [System.Web.Http.HttpGet]
        public ActionResult InformantUserID()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.AspNetUsers.Where(x => x.IsEnabled == true).ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.MyName);//Area Name
                jo.Add("Value", item.UserName); // ASN 
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region MaintainUser保養人員
        //[HttpGet]
        //public ActionResult MaintainUser() //保養人員
        //{
        //    List<JObject> list = new List<JObject> { };
        //    var data = db.InspectionPlanMaintain.Select(x => x.MaintainUserID).ToList();
        //    var mynamedatalist = db.AspNetUsers.Where(x => data.Contains(x.UserName)).ToList();
        //    foreach (var item in mynamedatalist)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item.MyName);
        //        jo.Add("Value", item.UserName);
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        # region 審核人員_保養
        //[HttpGet]
        //public ActionResult AuditUser_Maintain() //審核人員_保養
        //{
        //    List<JObject> list = new List<JObject> { };
        //    var data = db.MaintainAuditInfo.Select(x => x.AuditUserID).ToList();
        //    var mynamedatalist = db.AspNetUsers.Where(x => data.Contains(x.UserName)).ToList();
        //    foreach (var item in mynamedatalist)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item.MyName);
        //        jo.Add("Value", item.UserName);
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion 

        #region ReportUser報修人員
        [HttpGet]
        public ActionResult ReportUser() //報修人員
        {
            List<JObject> list = new List<JObject> { };
            var data = db.EquipmentReportForm.Select(x => x.InformatUserID).ToList();
            var mynamedatalist = db.AspNetUsers.Where(x => data.Contains(x.UserName)).ToList();
            foreach (var item in mynamedatalist)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.MyName);
                jo.Add("Value", item.UserName);
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion 

        #region RepairUser施工人員
        //[HttpGet]
        //public ActionResult RepairUser()
        //{
        //    List<JObject> list = new List<JObject> { };
        //    var data = db.InspectionPlanRepair.Select(x => x.RepairUserID).ToList();
        //    var mynamedatalist = db.AspNetUsers.Where(x => data.Contains(x.UserName)).ToList();
        //    foreach (var item in mynamedatalist)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item.MyName);
        //        jo.Add("Value", item.UserName);
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region AuditUser_Repair審核人員_維修
        //[HttpGet]
        //public ActionResult AuditUser_Repair()
        //{
        //    List<JObject> list = new List<JObject> { };
        //    var data = db.RepairAuditInfo.Select(x => x.AuditUserID).ToList();
        //    var mynamedatalist = db.AspNetUsers.Where(x => data.Contains(x.UserName)).ToList();
        //    foreach (var item in mynamedatalist)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item.MyName);
        //        jo.Add("Value", item.UserName);
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region 主系統 //要換掉
        //[System.Web.Http.HttpGet]
        //public ActionResult System()
        //{
        //    List<JObject> list = new List<JObject>();
        //    var abc = db.EquipmentInfo.Select(x => x.System).Distinct().ToList();
        //    foreach (var item in abc)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item);//System
        //        jo.Add("Value", item); // System
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region 子系統 //要換掉
        //[System.Web.Http.HttpGet]
        //public ActionResult SubSystem(string System)
        //{
        //    List<JObject> list = new List<JObject>();
        //    var abc = db.EquipmentInfo.Where(x => x.System == System).Select(x => x.SubSystem).Distinct().ToList();
        //    foreach (var item in abc)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item);//SubSystem
        //        jo.Add("Value", item); // SubSystem
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region 主系統/主系統
        [System.Web.Http.HttpGet]
        public ActionResult SystemName()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.SystemManagement.Where(x => x.SystemIsEnable == true).ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.System);//System
                jo.Add("Value", item.System); // System
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 子系統/子系統
        [System.Web.Http.HttpGet]
        public ActionResult SubSystemName()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.SubSystemManagement.Where(x => x.SubSystemIsEnable == true).ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.SubSystem);//SubSystem
                jo.Add("Value", item.SubSystem); //SubSystem
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region FormItemState 保養項目狀態
        [HttpGet]
        public ActionResult FormItemState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.EquipmentMaintainFormItemState();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 新增巡檢計畫->新增定期保養單DataGrid 保養狀態下拉式選單
        [HttpGet]
        public ActionResult AddFormItemState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.EquipmentMaintainFormItemState();

            foreach (var a in Dics)
            {
                if (a.Key == "1" || a.Key == "5" || a.Key == "8" || a.Key == "9" || a.Key == "10" || a.Key == "11")
                {
                    JObject jo = new JObject
                    {
                        { "Text", a.Value },
                        { "Value", a.Key }
                    };
                    list.Add(jo);
                }
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 設備狀態 下拉式選單
        [HttpGet]
        public ActionResult EState(string url = "")
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.EState();

            foreach (var a in Dics)
            {
                if (url == "AddToPlan" && a.Key == "3")
                {
                    continue;
                }
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 庫存狀態 下拉式選單
        [HttpGet]
        public ActionResult StockStatus()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.StockStatus();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region InspectionUser巡檢人員
        //[HttpGet]
        //public ActionResult InspectionUser()
        //{
        //    List<JObject> list = new List<JObject> { };
        //    var data = db.InspectionPlanMember.Select(x => x.UserID).Distinct().ToList();
        //    var mynamedatalist = db.AspNetUsers.Where(x => data.Contains(x.UserName)).ToList();
        //    foreach (var item in mynamedatalist)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item.MyName);
        //        jo.Add("Value", item.UserName);
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region 單位下拉式選單
        [HttpGet]
        public ActionResult Unit()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.Unit();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 庫存類別下拉式選單
        [HttpGet]
        public ActionResult StockType()
        {
            List<JObject> list = new List<JObject>();
            var Dics = db.StockType.ToList();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.StockTypeName },
                    { "Value", a.StockTypeSN }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 設計圖說種類下拉式選單
        [HttpGet]
        public ActionResult ImgType()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.ImgType();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 設備名稱下拉式選單
        [HttpGet]
        public ActionResult EName()
        {
            List<JObject> list = new List<JObject>();
            var ENamelist = new List<string>();
            ENamelist = db.EquipmentInfo.Select(x => x.EName).Distinct().ToList();

            foreach (var item in ENamelist)
            {
                JObject jo = new JObject
                {
                    { "Text", item },
                    { "Value", item }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 圖系統下拉式選單
        //[HttpGet]
        //public ActionResult DSystem()
        //{
        //    List<JObject> list = new List<JObject>();
        //    var DSystemlist = new List<DrawingSystemManagement>();
        //    DSystemlist = db.DrawingSystemManagement.Where(x => x.SystemIsEnable == true).ToList();

        //    foreach (var item in DSystemlist)
        //    {
        //        JObject jo = new JObject
        //        {
        //            { "Text", item.DSystem },
        //            { "Value", item.DSystemID }
        //        };
        //        list.Add(jo);
        //    }

        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region 圖子系統下拉式選單
        //[HttpGet]
        //public ActionResult DSubSystem(string DSystemID = "")
        //{
        //    List<JObject> list = new List<JObject>();
        //    var DSubSystemlist = new List<DrawingSubSystemManagement>();

        //    if (string.IsNullOrEmpty(DSystemID))
        //    {
        //        DSubSystemlist = db.DrawingSubSystemManagement.Where(x => x.SubSystemIsEnable == true).ToList();
        //    }
        //    else
        //    {
        //        var dsystemid = Convert.ToInt32(DSystemID);
        //        DSubSystemlist = db.DrawingSubSystemManagement.Where(x => x.SubSystemIsEnable == true && x.DSystemID == dsystemid).ToList();
        //    }

        //    foreach (var item in DSubSystemlist)
        //    {
        //        JObject jo = new JObject
        //        {
        //            { "Text", item.DSubSystem },
        //            { "Value", item.DSubSystemID }
        //        };
        //        list.Add(jo);
        //    }

        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        //--實驗室管理--
        #region FormMType 實驗室維護類型
        [HttpGet]
        public ActionResult FormMType()
        {
            List<JObject> list = new List<JObject>();
            var type = db.LaboratoryMaintenance.Select(x => x.MType).Distinct().ToList();
            foreach (var item in type)
            {
                JObject jo = new JObject
                {
                    { "Text", item },
                    { "Value", item }
                };
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region FormExperimentType 實驗室實驗類型
        [HttpGet]
        public ActionResult FormExperimentType()
        {
            List<JObject> list = new List<JObject>();
            var experiment = db.TestingAndAnalysisWorkflow.Select(x => x.ExperimentType).Distinct();
            foreach (var item in experiment)
            {
                JObject jo = new JObject
                {
                    { "Text", item },
                    { "Value", item }
                };
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region FormExperimentName 實驗室實驗名稱
        [HttpGet]
        public ActionResult FormExperimentName(string ExperimentType)
        {
            List<JObject> list = new List<JObject>();
            var experiment = db.TestingAndAnalysisWorkflow.Where(x => x.ExperimentType == ExperimentType).Select(x => new { x.TAWSN, x.ExperimentName }).ToDictionary(k => k.TAWSN, v => v.ExperimentName);
            foreach (var item in experiment)
            {
                JObject jo = new JObject
                {
                    { "Text", item.Value },
                    { "Value", item.Key }
                };
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        //--庫存管理--
        #region FormPRDept 請購部門
        //[HttpGet]
        //public ActionResult FormPRDept()
        //{
        //    List<JObject> list = new List<JObject>();
        //    var dept = db.PurchaseRequisition.Select(x => x.PRDept).Distinct().ToList();
        //    foreach (var item in dept)
        //    {
        //        JObject jo = new JObject
        //        {
        //            { "Text", item },
        //            { "Value", item }
        //        };
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region FormPRState 請購申請狀態
        [HttpGet]
        public ActionResult FormPRState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.PRState();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region FormStockName 品名
        /// <summary>
        /// 以庫存種類對庫存(表[ComputationalStock])進行搜尋
        /// </summary>
        /// <param name="StockType">使用者選擇的庫存類型</param>
        /// <returns>所有指定種類下的品名及對應SN的Option List</returns>
        //[HttpGet]
        //public ActionResult FormStockName(string StockType)
        //{
        //    List<JObject> list = new List<JObject>();
        //    var query = db.ComputationalStock.Select(x => new { x.SISN, x.StockType, x.StockName });
        //    if (!string.IsNullOrEmpty(StockType))
        //        query = query.Where(x => x.StockType == StockType);

        //    list = query.AsEnumerable().Select(a => new JObject
        //    {
        //        { "Text", a.StockName },
        //        { "Value", a.SISN }
        //    }).ToList();

        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        #endregion

        #region GetUnitText 單位
        /// <summary>
        /// 以庫存項目編碼對庫存(表[ComputationalStock])進行搜尋
        /// </summary>
        /// <param name="SISN">使用者選擇的庫存名稱value</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetUnitText(string SISN)
        {
            var Dics = Surface.Unit();
            var unit = db.ComputationalStock.Where(x => x.SISN == SISN).AsEnumerable().Select(x => new JObject { { "Unit", Dics[x.Unit] } }).FirstOrDefault();
            string text = JsonConvert.SerializeObject(unit);
            return Content(text, "application/json");
        }
        #endregion

        #region FormSRState 領用申請狀態
        [HttpGet]
        public ActionResult FormSRState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.SRState();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        //--警示訊息管理--
        #region WMType 事件等級
        [HttpGet]
        public ActionResult WMType()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.WMType();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region WMState 事件處理狀況
        [HttpGet]
        public ActionResult WMState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.WMState();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        //--報修管理
        //#region Floor 樓層
        //[System.Web.Http.HttpGet]
        //public ActionResult Floor(int ASN)
        //{
        //    List<JObject> list = new List<JObject>();
        //    var table = db.Floor_Info.Where(f => f.ASN == ASN).ToList();
        //    foreach (var item in table)
        //    {
        //        JObject jo = new JObject();
        //        jo.Add("Text", item.FloorName);
        //        jo.Add("Value", item.FSN);
        //        list.Add(jo);
        //    }
        //    string text = JsonConvert.SerializeObject(list);
        //    return Content(text, "application/json");
        //}
        //#endregion

        #region RepairState 報修單狀態
        [HttpGet]
        public ActionResult ReportState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.ReportState();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region RepairLevel 報修等級
        [HttpGet]
        public ActionResult ReportLevel()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.ReportLevel();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region EquipmentNoEName 設備編號/名稱
        [System.Web.Http.HttpGet]
        public ActionResult EquipmentNoEName(string FSN)
        {
            List<JObject> list = new List<JObject>();
            var table = db.EquipmentInfo.Where(e => e.FSN == FSN).ToList();
            foreach (var item in table)
            {
                JObject jo = new JObject();
                jo.Add("Text", $"{item.NO} {item.EName}");
                jo.Add("Value", item.ESN);
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region InspectionUserName 執行人員
        [System.Web.Http.HttpGet]
        public ActionResult InspectionUserName()
        {
            List<JObject> list = new List<JObject>();
            var table = db.AspNetUsers.Where(a => a.Authority == "4").ToList();
            foreach (var item in table)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.UserName);
                jo.Add("Value", item.UserName);
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        // --2024/10/25後更新
        #region MaintainPeriod 保養週期
        [HttpGet]
        public ActionResult MaintainPeriod()
        {
            List<JObject> list = ConvertDicToJObjectList(Surface.MaintainPeriod());

            return Content(JsonConvert.SerializeObject(list), "application/json");
        }
        #endregion

        #region Helper
        private List<JObject> ConvertDicToJObjectList(Dictionary<string, string> dic)
        {
            return dic.Select(x => new JObject
            {
                { "Text", x.Value },
                { "Value", x.Key }
            }).ToList();
        }
        #endregion
    }
}