using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
                jo.Add("FSN", item.FSN); // ViewName
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

        #region Frequency 巡檢頻率
        [HttpGet]
        public ActionResult Frequency()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.InspectionPlanFrequency();

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

        #region MaintainStatus相關 保養單狀態
        [HttpGet]
        public ActionResult MaintainStatus()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.MaintainStatus();

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
        [HttpGet]
        public ActionResult MaintainUserName() //保養人員
        {
            List<JObject> list = new List<JObject> { };
            var data = db.Equipment_MaintenanceFormMember.Select(x => x.Maintainer).ToList();
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

        #region 設備狀態 下拉式選單
        [HttpGet]
        public ActionResult EState()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.EState();

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

        #region InspectionMember巡檢人員
        [HttpGet]
        public ActionResult InspectionMember()
        {
            List<JObject> list = new List<JObject>();
            var members = db.AspNetUsers.Where(x => x.Authority == "4").ToList();
            foreach (var item in members)
            {
                JObject jo = new JObject
                {
                    { "Text", item.MyName },
                    { "Value", item.UserName }
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

        #region 庫存品項名稱下拉式選單
        [HttpGet]
        public ActionResult StockName(string stockTypeSN)
        {
            List<JObject> list = new List<JObject>();
            var Dics = db.ComputationalStock.AsQueryable();

            if (!string.IsNullOrEmpty(stockTypeSN))
            {
                Dics = Dics.Where(x => x.StockTypeSN.ToString() == stockTypeSN);
            }

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.StockName },
                    { "Value", a.SISN }
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
        [HttpGet]
        public ActionResult DSystem()
        {
            List<JObject> list = new List<JObject>();
            var DSystemlist = new List<DrawingSystemManagement>();
            DSystemlist = db.DrawingSystemManagement.ToList();

            foreach (var item in DSystemlist)
            {
                JObject jo = new JObject
                {
                    { "Text", item.DSystem },
                    { "Value", item.DSystemID }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 圖子系統下拉式選單
        [HttpGet]
        public ActionResult DSubSystem(string DSystemID = "")
        {
            List<JObject> list = new List<JObject>();
            var DSubSystemlist = new List<DrawingSubSystemManagement>();

            if (string.IsNullOrEmpty(DSystemID))
            {
                //DSubSystemlist = db.DrawingSubSystemManagement.ToList();
            }
            else
            {
                var dsystemid = Convert.ToInt32(DSystemID);
                DSubSystemlist = db.DrawingSubSystemManagement.Where(x => x.DSystemID == dsystemid).ToList();
            }

            foreach (var item in DSubSystemlist)
            {
                JObject jo = new JObject
                {
                    { "Text", item.DSubSystem },
                    { "Value", item.DSubSystemID }
                };
                list.Add(jo);
            }

            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
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

        #region RepairUserName 報修管理執行人員
        [System.Web.Http.HttpGet]
        public ActionResult RepairUserName()
        {
            List<JObject> list = new List<JObject>();
            var table = db.Equipment_ReportFormMember.Select(e => e.RepairUserName).ToHashSet();
            foreach (var item in table)
            {
                JObject jo = new JObject();
                jo.Add("Text", $"{db.AspNetUsers.Where(a => a.UserName == item).Select(a => a.MyName).FirstOrDefault()}");
                jo.Add("Value", item);
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        //--派工相關
        #region AssignmentUserName 派工人員
        [System.Web.Http.HttpGet]
        public ActionResult AssignmentUserName()
        {
            List<JObject> list = new List<JObject>();
            var table = db.AspNetUsers.Where(a => a.Authority == "4").ToList();
            foreach (var item in table)
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

        #region MaintainPeriod 保養週期
        [HttpGet]
        public ActionResult MaintainPeriod()
        {
            List<JObject> list = ConvertDicToJObjectList(Surface.MaintainPeriod());

            return Content(JsonConvert.SerializeObject(list), "application/json");
        }
        #endregion

        #region OneDeviceOneCardTemplates 一機一卡模板
        [HttpGet]
        public ActionResult OneDeviceOneCardTemplates()
        {
            var list = db.Template_OneDeviceOneCard.Select(x => new
            {
                Text = x.SampleName,
                Value = x.TSN
            }).AsEnumerable();

            return Content(JsonConvert.SerializeObject(list), "application/json");
        }
        #endregion

        #region OneDeviceOneCardTemplates 巡檢路線模板
        [HttpGet]
        public ActionResult InspectionPathSample()
        {
            var list = db.InspectionPathSample.Select(x => new
            {
                Text = x.PathName,
                Value = x.PlanPathSN
            }).AsEnumerable();

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