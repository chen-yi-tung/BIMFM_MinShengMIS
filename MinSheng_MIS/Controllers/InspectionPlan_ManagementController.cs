using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using static MinSheng_MIS.Models.ViewModels.AddInspectionPlan;
using static MinSheng_MIS.Models.ViewModels.PathSampleViewModel;
using static MinSheng_MIS.Models.ViewModels.ReadInspectionPlanPathData;
using PathSample = MinSheng_MIS.Models.ViewModels.ReadInspectionPlanPathData.PathSample;

namespace MinSheng_MIS.Controllers
{
    public class InspectionPlan_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 巡檢計畫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增巡檢計畫
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateInspectionPlan(AddInspectionPlan InspectionPlan)
        {
            #region 新增巡檢計畫 dbo.InspectionPlan
            InspectionPlan plan = new InspectionPlan();
            //為巡檢計畫編號
            var count = db.InspectionPlan.Where(x => x.PlanDate == InspectionPlan.PlanDate.Date).Count() + 1;
            plan.IPSN = "P" + InspectionPlan.PlanDate.ToString("yyMMdd") + count.ToString().PadLeft(2, '0');
            var IPSN = plan.IPSN;
            plan.IPName = InspectionPlan.IPName;
            plan.PlanDate = InspectionPlan.PlanDate.Date;
            plan.Shift = InspectionPlan.Shift;
            plan.MaintainUserID = InspectionPlan.MaintainUserID;
            plan.RepairUserID = InspectionPlan.RepairUserID;
            //計算定期保養數量
            plan.MaintainAmount = InspectionPlan.MaintainEquipment.Count();
            //計算維修設備數量
            plan.RepairAmount = InspectionPlan.RepairEquipment.Count();
            plan.PlanState = "1"; //=待執行
            plan.PlanCreateUserID = InspectionPlan.PlanCreateUserID; //當前登入者帳號
            db.InspectionPlan.AddOrUpdate(plan);
            db.SaveChanges();
            #endregion
            #region 新增巡檢計畫人員資料 dbo.InspectionPlanMember
            if (InspectionPlan.UserID != null)
            {
                int i = 1;
                foreach (var member in InspectionPlan.UserID)
                {
                    InspectionPlanMember planMember = new InspectionPlanMember();
                    planMember.IPSN = IPSN;
                    planMember.PMSN = IPSN + "_" + i;
                    planMember.UserID = member;
                    db.InspectionPlanMember.AddOrUpdate(planMember);
                    db.SaveChanges();
                    i++;
                }
            }
            #endregion 巡檢計畫含保養 dbo.InspectionPlanMaintain
            if (InspectionPlan.MaintainEquipment != null)
            {
                int i = 1;
                foreach (var EMFISN in InspectionPlan.MaintainEquipment)
                {
                    //新增巡檢計畫含保養
                    InspectionPlanMaintain planMaintain = new InspectionPlanMaintain();
                    planMaintain.IPSN = IPSN;
                    planMaintain.IPMSN = IPSN + "_M" + i.ToString().PadLeft(2, '0');
                    planMaintain.EMFISN = EMFISN;
                    planMaintain.MaintainState = "1";//已派工
                    db.InspectionPlanMaintain.AddOrUpdate(planMaintain);
                    db.SaveChanges();
                    //更改該保養單狀態
                    EquipmentMaintainFormItem maintainformitem = db.EquipmentMaintainFormItem.Find(EMFISN);
                    maintainformitem.FormItemState = "2"; //已派工
                    db.EquipmentMaintainFormItem.AddOrUpdate(maintainformitem);
                    db.SaveChanges();
                    i++;
                }
            }
            #region 巡檢計畫含維修 dbo.InspectionPlanRepair
            if (InspectionPlan.RepairEquipment != null)
            {
                int i = 1;
                foreach (var RSN in InspectionPlan.RepairEquipment)
                {
                    //巡檢計畫含維修
                    InspectionPlanRepair planRepair = new InspectionPlanRepair();
                    planRepair.IPSN = IPSN;
                    planRepair.IPRSN = IPSN + "_R" + i.ToString().PadLeft(2, '0');
                    planRepair.RSN = RSN;
                    planRepair.RepairState = "1"; //已派工
                    db.InspectionPlanRepair.AddOrUpdate(planRepair);
                    db.SaveChanges();
                    //更改報修單狀態
                    EquipmentReportForm report = db.EquipmentReportForm.Find(RSN);
                    report.ReportState = "2"; //已派工
                    db.EquipmentReportForm.AddOrUpdate(report);
                    db.SaveChanges();
                    i++;
                }
            }
            #endregion
            #region 巡檢路線規劃(路徑標題及路徑順序)
            if (InspectionPlan.InspectionPlanPaths != null)
            {
                var i = 1;
                foreach (var paths in InspectionPlan.InspectionPlanPaths)
                {
                    #region 巡檢計畫路徑 dbo.InspectionPlanPath
                    InspectionPlanPath planPath = new InspectionPlanPath();
                    planPath.IPSN = IPSN;
                    planPath.PSN = IPSN + "_" + i.ToString().PadLeft(2, '0');
                    var PSN = planPath.PSN;
                    planPath.FSN = paths.PathSample.FSN;
                    planPath.PathTitle = paths.PathSample.PathTitle;
                    db.InspectionPlanPath.AddOrUpdate(planPath);
                    db.SaveChanges();
                    #endregion
                    #region 巡檢計畫單樓層路徑 dbo.InspectionPlanFloorPath
                    if (paths.PathSampleOrder != null)
                    {
                        int j = 1;
                        foreach (var DeviceID in paths.PathSampleOrder)
                        {
                            InspectionPlanFloorPath floorPath = new InspectionPlanFloorPath();
                            floorPath.FPSN = PSN + "_" + j.ToString().PadLeft(2, '0');
                            floorPath.PSN = PSN;
                            floorPath.DeviceID = DeviceID;
                            db.InspectionPlanFloorPath.AddOrUpdate(floorPath);
                            db.SaveChanges();
                            j++;
                        }
                    }
                    #endregion
                    #region 巡檢計畫路徑繪製 dbo.DrawInspectionPlanPath
                    if (paths.PathSampleRecord != null)
                    {
                        int j = 1;
                        foreach (var location in paths.PathSampleRecord)
                        {
                            DrawInspectionPlanPath drawPath = new DrawInspectionPlanPath();
                            drawPath.ISN = PSN + "_" + j.ToString().PadLeft(2, '0');
                            drawPath.PSN = PSN;
                            drawPath.LocationX = location.LocationX;
                            drawPath.LocationY = location.LocationY;
                            db.DrawInspectionPlanPath.AddOrUpdate(drawPath);
                            db.SaveChanges();
                            j++;
                        }
                    }
                    #endregion
                    i++;
                }
            }
            #endregion
            JObject jo = new JObject();
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-新增定期保養單
        [HttpPost]
        public ActionResult AddMaintainForm(List<String> EMFISN)
        {
            JArray ja = new JArray();
            foreach (var item in EMFISN)
            {
                //變更狀態為保留中
                var maintainform = db.EquipmentMaintainFormItem.Find(item);
                switch (maintainform.FormItemState)
                {
                    case "1":
                        maintainform.FormItemState = "9";
                        break;
                    case "5":
                        maintainform.FormItemState = "10";
                        break;
                    case "8":
                        maintainform.FormItemState = "11";
                        break;
                }
                db.EquipmentMaintainFormItem.AddOrUpdate(maintainform);
                db.SaveChanges();
                //回傳的顯示資料
                JObject itemObjects = new JObject();
                //庫存狀態
                var StockStatedic = Surface.StockState();
                if (maintainform.StockState)
                {
                    itemObjects.Add("StockState", StockStatedic["1"]);
                }
                else
                {
                    itemObjects.Add("StockState", StockStatedic["0"]);
                }
                //保養項目狀態
                if (!string.IsNullOrEmpty(maintainform.FormItemState))
                {
                    var dic = Surface.EquipmentMaintainFormItemState();
                    itemObjects.Add("FormItemState", dic[maintainform.FormItemState]);
                }
                //保養單項目編號
                itemObjects.Add("EMFISN", maintainform.EMFISN);
                //週期
                itemObjects.Add("Period", maintainform.Period);
                //週期單位
                itemObjects.Add("Unit", maintainform.Unit);
                //上次保養
                itemObjects.Add("LastTime", maintainform.LastTime.ToString("yyyy/MM/dd"));
                //最近應保養
                itemObjects.Add("Date", maintainform.Date.ToString("yyyy/MM/dd"));

                var EMISN = maintainform.EMISN;
                var ESN = db.EquipmentMaintainItem.Find(EMISN).ESN;
                var MISN = db.EquipmentMaintainItem.Find(EMISN).MISN;
                var Equipment = db.EquipmentInfo.Find(ESN);
                var ASN = db.Floor_Info.Find(Equipment.FSN).ASN;
                //設備狀態
                if (!string.IsNullOrEmpty(Equipment.EState))
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[Equipment.EState]);
                }
                //棟別
                if (!string.IsNullOrEmpty(Equipment.Area))
                {
                    itemObjects.Add("Area", Equipment.Area);
                }
                if (!string.IsNullOrEmpty(ASN.ToString()))
                {
                    itemObjects.Add("ASN", ASN);
                }
                //樓層
                if (!string.IsNullOrEmpty(Equipment.Floor))
                {
                    itemObjects.Add("Floor", Equipment.Floor);
                }
                if (!string.IsNullOrEmpty(Equipment.FSN))
                {
                    itemObjects.Add("FSN", Equipment.FSN);
                }
                //設備編號
                if (!string.IsNullOrEmpty(ESN))
                {
                    itemObjects.Add("ESN", ESN);
                }
                //設備名稱
                if (!string.IsNullOrEmpty(Equipment.EName))
                {
                    itemObjects.Add("EName", Equipment.EName);
                }
                var maintainitem = db.MaintainItem.Find(MISN);
                //保養項目
                if (!string.IsNullOrEmpty(maintainitem.MIName))
                {
                    itemObjects.Add("MIName", maintainitem.MIName);
                }
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);

            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-新增定期保養/維修設備 回傳設備資料
        [HttpPost]
        public ActionResult ResponseDeviceInfo(List<String> ESN)
        {
            JArray ja = new JArray();
            foreach (var item in ESN)
            {
                var deviceinfo = db.EquipmentInfo.Find(item);
                JObject itemObjects = new JObject();
                itemObjects.Add("FSN", deviceinfo.FSN);
                itemObjects.Add("ESN", item);
                itemObjects.Add("DBID", deviceinfo.DBID);
                JObject positionObjects = new JObject();
                if (deviceinfo.LocationX != null && deviceinfo.LocationY != null)
                {
                    positionObjects.Add("LocaiotnX", deviceinfo.LocationX);
                    positionObjects.Add("LocaiotnY", deviceinfo.LocationY);
                    itemObjects.Add("Position", positionObjects);
                }
                else
                {
                    itemObjects.Add("Position", null);
                }

                var FSN = deviceinfo.FSN;
                var ASN = db.Floor_Info.Find(FSN).ASN;
                itemObjects.Add("ASN", ASN);
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("DeviceData", ja);

            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-刪除定期保養單
        [HttpPost]
        public ActionResult DeleteMaintainForm(List<String> EMFISN)
        {
            JArray ja = new JArray();
            foreach (var item in EMFISN)
            {
                //變更狀態為保留中
                var maintainform = db.EquipmentMaintainFormItem.Find(item);
                switch (maintainform.FormItemState)
                {
                    case "9":
                        maintainform.FormItemState = "1";
                        break;
                    case "10":
                        maintainform.FormItemState = "5";
                        break;
                    case "11":
                        maintainform.FormItemState = "8";
                        break;
                }
                db.EquipmentMaintainFormItem.AddOrUpdate(maintainform);
                db.SaveChanges();
                JObject itemObjects = new JObject();
                itemObjects.Add("EMFISN", maintainform.EMFISN);
                var EMISN = maintainform.EMISN;
                var ESN = db.EquipmentMaintainItem.Find(EMISN).ESN;
                itemObjects.Add("ESN", ESN);
                ja.Add(itemObjects);
            }

            string result = JsonConvert.SerializeObject(ja);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-新增維修設備
        [HttpPost]
        public ActionResult AddReportForm(List<String> RSN)
        {
            JArray ja = new JArray();
            foreach (var item in RSN)
            {
                //變更狀態為保留中
                var RSNInfo = db.EquipmentReportForm.Find(item);
                switch (RSNInfo.ReportState)
                {
                    case "1":
                        RSNInfo.ReportState = "9";
                        break;
                    case "5":
                        RSNInfo.ReportState = "10";
                        break;
                    case "8":
                        RSNInfo.ReportState = "11";
                        break;
                }
                db.EquipmentReportForm.AddOrUpdate(RSNInfo);
                db.SaveChanges();
                //回傳的顯示資料
                JObject itemObjects = new JObject();
                //庫存狀態
                var StockStatedic = Surface.StockState();
                if (RSNInfo.StockState)
                {
                    itemObjects.Add("StockState", StockStatedic["1"]);
                }
                else
                {
                    itemObjects.Add("StockState", StockStatedic["0"]);
                }
                //報修單狀態
                if (!string.IsNullOrEmpty(RSNInfo.ReportState))
                {
                    var dic = Surface.EquipmentReportFormState();
                    itemObjects.Add("ReportState", dic[RSNInfo.ReportState]);
                }
                //設備編號
                var ESN = RSNInfo.ESN;
                itemObjects.Add("ESN", RSNInfo.ESN);
                //報修單號
                itemObjects.Add("RSN", RSNInfo.RSN);
                //報修等級
                var ReportLeveldic = Surface.ReportLevel();
                itemObjects.Add("ReportLevel", ReportLeveldic[RSNInfo.ReportLevel]);
                //報修時間
                itemObjects.Add("Date", RSNInfo.Date.ToString("yyyy/MM/dd HH:mm:ss"));
                //報修內容
                itemObjects.Add("ReportContent", RSNInfo.ReportContent);

                //報修人員
                var InformatUserID = RSNInfo.InformatUserID;
                var MyName = db.AspNetUsers.Where(x => x.UserName == InformatUserID).Select(x => x.MyName).FirstOrDefault();
                itemObjects.Add("InformatUserID", MyName);

                var Equipment = db.EquipmentInfo.Find(ESN);
                var ASN = db.Floor_Info.Find(Equipment.FSN).ASN;
                //設備狀態
                if (!string.IsNullOrEmpty(Equipment.EState))
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[Equipment.EState]);
                }
                //棟別
                if (!string.IsNullOrEmpty(Equipment.Area))
                {
                    itemObjects.Add("Area", Equipment.Area);
                }
                if (!string.IsNullOrEmpty(ASN.ToString()))
                {
                    itemObjects.Add("ASN", ASN);
                }
                //樓層
                if (!string.IsNullOrEmpty(Equipment.Floor))
                {
                    itemObjects.Add("Floor", Equipment.Floor);
                }
                if (!string.IsNullOrEmpty(Equipment.FSN))
                {
                    itemObjects.Add("FSN", Equipment.FSN);
                }
                //設備名稱
                if (!string.IsNullOrEmpty(Equipment.EName))
                {
                    itemObjects.Add("EName", Equipment.EName);
                }

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);

            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-刪除維修設備
        [HttpPost]
        public ActionResult DeleteReportForm(List<String> RSN)
        {
            JArray ja = new JArray();
            foreach (var item in RSN)
            {
                //變更狀態為保留中
                var RSNInfo = db.EquipmentReportForm.Find(item);
                switch (RSNInfo.ReportState)
                {
                    case "9":
                        RSNInfo.ReportState = "1";
                        break;
                    case "10":
                        RSNInfo.ReportState = "5";
                        break;
                    case "11":
                        RSNInfo.ReportState = "8";
                        break;
                }
                db.EquipmentReportForm.AddOrUpdate(RSNInfo);
                db.SaveChanges();
                JObject itemObjects = new JObject();
                itemObjects.Add("RSN", item);
                var ESN = db.EquipmentReportForm.Find(item).ESN;
                itemObjects.Add("ESN", ESN);
                ja.Add(itemObjects);
            }

            string result = JsonConvert.SerializeObject(ja);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-新增巡檢路線
        [HttpPost]
        public ActionResult AddPlanPath(PlanPathInput ppi)
        {
            PlanPathOutput ppo = new PlanPathOutput();

            PathSample ps = new PathSample();
            ps.ASN = ppi.ASN;
            ps.FSN = ppi.FSN;
            var floorinfo = db.Floor_Info.Find(ps.FSN);
            ps.Floor = floorinfo.FloorName;
            ps.BIMPath = floorinfo.BIMPath;
            ps.BeaconPath = floorinfo.BeaconPath;
            var areainfo = db.AreaInfo.Find(Convert.ToInt32(ps.ASN));
            ps.Area = areainfo.Area;
            //var plandate = Convert.ToDateTime(ppi.PlanDate);
            //var IPSNcount = db.InspectionPlan.Where(x => x.PlanDate == plandate).Count();
            //var tmpIPSN = "P" + plandate.ToString("yyMMdd") + (IPSNcount + 1).ToString().PadLeft(2, '0');
            //判斷PathTitle是否為空
            if (!string.IsNullOrEmpty(ppi.PathTitle))
            {
                //PathTitle不為空 找出路徑模板資料
                ps.PSSN = ppi.PathTitle;
                var pathtitle = db.PathSample.Find(ps.PSSN).PathTitle;
                //加入預估巡檢計畫單號
                ps.PathTitle = ps.Area + " " + ps.Floor + " " + pathtitle; // + " " + tmpIPSN
                                                                           //找出來藍芽路徑
                var BeaconOrder = db.PathSampleOrder.Where(x => x.PSSN == ps.PSSN).OrderBy(x => x.FPSSN).ToList();
                List<string> psos = new List<string>();
                foreach (var item in BeaconOrder)
                {
                    psos.Add(item.BeaconID);
                }
                ppo.PathSampleOrder = psos;
                //找出繪製路徑
                var DrawList = db.DrawPathSample.Where(x => x.PSSN == ps.PSSN).OrderBy(x => x.SISN).ToList();
                List<ReadInspectionPlanPathData.PathSampleRecord> psrs = new List<ReadInspectionPlanPathData.PathSampleRecord>();
                foreach (var item in DrawList)
                {
                    ReadInspectionPlanPathData.PathSampleRecord psr = new ReadInspectionPlanPathData.PathSampleRecord();
                    psr.LocationX = item.LocationX;
                    psr.LocationY = item.LocationY;
                    psrs.Add(psr);
                }
                ppo.PathSampleRecord = psrs;
            }
            else
            {
                //加入預估巡檢計畫單號
                ps.PathTitle = ps.Area + " " + ps.Floor + " 巡檢路線"; // + " " + tmpIPSN
                List<string> psos = new List<string>();
                ppo.PathSampleOrder = psos;
                List<ReadInspectionPlanPathData.PathSampleRecord> psrs = new List<ReadInspectionPlanPathData.PathSampleRecord>();
                ppo.PathSampleRecord = psrs;
            }
            //Beacon
            //找出該樓層所有藍芽
            var BeaconList = db.EquipmentInfo.Where(x => x.FSN == ps.FSN && x.EName == "藍芽").ToList();
            List<Beacon> beacons = new List<Beacon>();
            foreach (var item in BeaconList)
            {
                Beacon beacon = new Beacon();
                beacon.dbId = Convert.ToInt32(item.DBID);
                beacon.deviceType = item.EName;
                beacon.deviceName = item.ESN;

                beacons.Add(beacon);
            }
            ps.Beacon = beacons;
            ppo.PathSample = ps;
            /*
            List<MaintainEquipment> maintainEquipments = new List<MaintainEquipment>();
            if (ppi.MaintainEquipment != null)
            {
                foreach (var item in ppi.MaintainEquipment)
                {
                    MaintainEquipment maintainEquipment = new MaintainEquipment();
                    maintainEquipment.ESN = item;
                    var DBID = db.EquipmentInfo.Find(item).DBID;
                    if (!string.IsNullOrEmpty(DBID.ToString()))
                    {
                        maintainEquipment.DBID = (int)DBID;
                    }
                    else
                    {
                        Position position = new Position();
                        var xy = db.EquipmentInfo.Find(item);
                        position.LocationX = (decimal)xy.LocationX;
                        position.LocationY = (decimal)xy.LocationY;
                    }
                    maintainEquipments.Add(maintainEquipment);
                }
            }
            ppo.MaintainEquipment = maintainEquipments;

            List<RepairEquipment> repairEquipments = new List<RepairEquipment>();
            if(ppi.RepairEquipment != null)
            {
                foreach (var item in ppi.RepairEquipment)
                {
                    RepairEquipment repairEquipment = new RepairEquipment();
                    repairEquipment.ESN = item;
                    var DBID = db.EquipmentInfo.Find(item).DBID;
                    if (!string.IsNullOrEmpty(DBID.ToString()))
                    {
                        repairEquipment.DBID = (int)DBID;
                    }
                    else
                    {
                        Position position = new Position();
                        var xy = db.EquipmentInfo.Find(item);
                        position.LocationX = (decimal)xy.LocationX;
                        position.LocationY = (decimal)xy.LocationY;
                    }
                    repairEquipments.Add(repairEquipment);
                }
            }
            ppo.RepairEquipment = repairEquipments;*/

            string result = JsonConvert.SerializeObject(ppo);
            return Content(result, "application/json");

        }
        #endregion

        #region 巡檢計畫詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }
        public ActionResult Read_Data(string id)
        {
            InspectionPlan_ManagementViewModel IMV = new InspectionPlan_ManagementViewModel();
            string result = IMV.InspectationPlan_Read_Data(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 編輯巡檢計畫
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Edit_Data(string id)
        {
            var IMV = new InspectionPlan_ManagementViewModel();
            string result = IMV.InspectationPlan_Edit_Data(id);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult Edit_Update(FormCollection form)
        {
            int resultCode = 400;
            var IMV = new InspectionPlan_ManagementViewModel();
            string result = IMV.InspectationPlan_Edit_Update(form, ref resultCode);
            Response.StatusCode = resultCode;
            return Content(result, "application/json");
        }
        #endregion

        #region 刪除巡檢計畫
        [HttpDelete]
        public ActionResult Delete(string id)
        {
            return View();
        }
        #endregion

        #region 巡檢紀錄
        public ActionResult Record(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Record_Data(string id)
        {
            var IMV = new InspectionPlan_ManagementViewModel();
            string result = IMV.GetJsonForRecord(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢軌跡紀錄
        public ActionResult TrackRecord()
        {
            return View();
        }
        #endregion

        #region 巡檢資訊管理
        public ActionResult InformationManagement()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetIspectionPlanInformation(int? year1 = null, int? month1 = null, int? year2 = null, int? month2 = null)
        {
            #region 檢索年份月份處理
            //檢查年分與月份，若為空則填入當下年份月份，若有值則將民國轉西元。
            if (year1.HasValue && month1.HasValue)
            {
                year1 += 1911;
            }
            else
            {
                year1 = DateTime.Today.Year;
                month1 = DateTime.Today.Month;
            }

            if(year2.HasValue && month2.HasValue)
            {
                year2 += 1911;
            }
            else
            {
                year2 = DateTime.Today.Year;
                month2 = DateTime.Today.Month;
            }
            //檢索之起始和結束日期
            DateTime StartDate = new DateTime((int)year1, (int)month1, 1); //起始年月第一天
            DateTime EndDate = new DateTime((int)year2, (int)month2, 1).AddMonths(1).AddDays(-1); //起始年月最後一天
            #endregion

            JObject InspectionPlanInformation = new JObject();

            #region 巡檢總計畫完成狀態
            JArray Inspection_Complete_State = new JArray();
            var inspectionplan = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate <= EndDate);
            var InspectionPlanStatedic = Surface.InspectionPlanState();
            foreach(var item in InspectionPlanStatedic)
            {
                if(item.Key != "5")
                {
                    JObject jo = new JObject();
                    jo.Add("label", item.Value);
                    jo.Add("value", Convert.ToInt32(inspectionplan.Where(x => x.PlanState == item.Key).Count()));
                    Inspection_Complete_State.Add(jo);
                }
            }
            InspectionPlanInformation.Add("Inspection_Complete_State", Inspection_Complete_State);
            #endregion

            #region 巡檢總設備狀態
            JArray Inspection_Equipment_State = new JArray();
            var RepairEquipments = (from x1 in db.InspectionPlanRepair
                                  join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN
                                  where x2.PlanDate >= StartDate && x2.PlanDate <= EndDate
                                  join x3 in db.EquipmentReportForm on x1.RSN equals x3.RSN
                                  select new { x3.ESN })
                                  .Distinct();
            var MaintainEquipments = (from x1 in db.InspectionPlanMaintain
                                      join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN
                                      where x2.PlanDate >= StartDate && x2.PlanDate <= EndDate
                                      join x3 in db.EquipmentMaintainFormItem on x1.EMFISN equals x3.EMFISN
                                      join x4 in db.EquipmentMaintainItem on x3.EMISN equals x4.EMISN
                                      select new { x4.ESN}).Distinct();
            var intersection = RepairEquipments.Intersect(MaintainEquipments); //找出在該檢索時間段有做保養及維修之設備
            JObject rm = new JObject {{ "label","保養" },{"value", MaintainEquipments.Count()}};
            Inspection_Equipment_State.Add(rm);
            JObject r = new JObject { { "label", "維修" }, { "value", RepairEquipments.Count() } };
            Inspection_Equipment_State.Add(r);
            JObject m = new JObject { { "label", "保養+維修" }, { "value", intersection.Count() } };
            Inspection_Equipment_State.Add(m);
            InspectionPlanInformation.Add("Inspection_Equipment_State", Inspection_Equipment_State);
            #endregion

            #region 巡檢人員清單
            JArray Inspection_All_Members = new JArray();
            var IPSNList = inspectionplan.Select(x => x.IPSN).ToList();
            var UserNameList = db.InspectionPlanMember.Where(x => IPSNList.Contains(x.IPSN)).Select(x => x.UserID).Distinct().ToList();
            foreach(var planmember in UserNameList)
            {
                JObject jo = new JObject();
                jo.Add("MyName", db.AspNetUsers.Where(x => x.UserName == planmember).FirstOrDefault().MyName.ToString()); //人員姓名
                var memberIPSN = db.InspectionPlanMember.Where(x => x.UserID == planmember && IPSNList.Contains(x.IPSN)).Select(x => x.IPSN).ToList();

                int PlanNum = memberIPSN.Count();//巡檢總數
                int MaintainNum = db.InspectionPlanMaintain.Where(x => memberIPSN.Contains(x.IPSN)).Count();//保養總數
                int RepairNum = db.InspectionPlanRepair.Where(x => memberIPSN.Contains(x.IPSN)).Count();//維修總數
                int FinishPlanNum = db.InspectionPlan.Where(x => memberIPSN.Contains(x.IPSN) && x.PlanState == "3").Count();//巡檢完成總數
                int FinishMaintainNum = db.InspectionPlanMaintain.Where(x => memberIPSN.Contains(x.IPSN) && x.MaintainState == "6").Count();//保養完成總數
                int FinishRepairNum = db.InspectionPlanRepair.Where(x => memberIPSN.Contains(x.IPSN) && x.RepairState == "6").Count();//維修完成總數

                jo.Add("PlanNum", PlanNum);
                jo.Add("MaintainNum", MaintainNum);
                jo.Add("RepairNum", RepairNum);
                jo.Add("CompleteNum", FinishPlanNum + FinishMaintainNum + FinishRepairNum);
                jo.Add("CompletionRate", (float)(FinishPlanNum + FinishMaintainNum + FinishRepairNum) / (PlanNum + MaintainNum + RepairNum));
                Inspection_All_Members.Add(jo);
            }
            InspectionPlanInformation.Add("Inspection_All_Members", Inspection_All_Members);
            #endregion

            #region 緊急事件等級占比/處理狀況
            var MessageList = db.WarningMessage.Where(x => x.TimeOfOccurrence >= StartDate && x.TimeOfOccurrence <= EndDate);
            //緊急事件等級占比
            JArray Inspection_Aberrant_Level = new JArray();
            var WMTypeDic = Surface.WMType();
            for(int i = 1; i <= 2; i++)
            {
                JObject jo = new JObject();
                jo.Add("label", WMTypeDic[i.ToString()]);
                jo.Add("value", MessageList.Where(x => x.WMType == i.ToString()).Count());
                Inspection_Aberrant_Level.Add(jo);
            }
            InspectionPlanInformation.Add("Inspection_Aberrant_Level", Inspection_Aberrant_Level);
            //緊急事件處理狀況
            JArray Inspection_Aberrant_Resolve = new JArray();
            var WMStateDic = Surface.WMState();
            for(int i = 1; i <= 3; i++)
            {
                JObject jo = new JObject();
                jo.Add("label", WMStateDic[i.ToString()]);
                jo.Add("value", MessageList.Where(x => x.WMState == i.ToString()).Count());
                Inspection_Aberrant_Resolve.Add(jo);
            }
            InspectionPlanInformation.Add("Inspection_Aberrant_Resolve", Inspection_Aberrant_Resolve);
            #endregion

            #region 設備保養及維修進度統計
            JArray Equipment_Maintain_And_Repair_Statistics = new JArray();
            var InspectionPlanRepairStateDic = Surface.InspectionPlanRepairState();
            var MaintainList = from x1 in db.InspectionPlanMaintain
                    join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN
                    where x2.PlanDate >= StartDate && x2.PlanDate <= EndDate
                    select new { x1.IPMSN, x1.MaintainState };
            var RepairList = from x1 in db.InspectionPlanRepair
                    join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN
                    where x2.PlanDate >= StartDate && x2.PlanDate <= EndDate
                    select new { x1.IPRSN, x1.RepairState };

            foreach (var item in InspectionPlanRepairStateDic)
            {
                JObject jo = new JObject();
                JObject mr = new JObject();
                mr.Add("Maintain", MaintainList.Where(x => x.MaintainState == item.Key).Count());
                mr.Add("Repair", RepairList.Where(x => x.RepairState == item.Key).Count());
                jo.Add("label", item.Value);
                jo.Add("value", mr);
                Equipment_Maintain_And_Repair_Statistics.Add(jo);
            }
            InspectionPlanInformation.Add("Equipment_Maintain_And_Repair_Statistics", Equipment_Maintain_And_Repair_Statistics);
            #endregion

            #region 設備故障等級分布
            JArray Equipment_Level_Rate = new JArray();
            var reportList = db.EquipmentReportForm.Where(x => x.Date >= StartDate && x.Date <= EndDate);
            var ReportLevelDic = Surface.ReportLevel();

            for(int i = 1; i <=3; i++)
            {
                JObject jo = new JObject();
                jo.Add("label", ReportLevelDic[i.ToString()]);
                jo.Add("value", reportList.Where(x => x.ReportState == i.ToString()).Count());
                Equipment_Level_Rate.Add(jo);
            }
            InspectionPlanInformation.Add("Equipment_Level_Rate", Equipment_Level_Rate);
            #endregion

            #region 設備故障類型占比
            JArray Equipment_Type_Rate = new JArray();
            //統計該區間設備故障類型占比
            var RepairEquipment = from x1 in db.EquipmentReportForm
                          where x1.Date >= StartDate && x1.Date <= EndDate
                          join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                          group x1 by new { x2.System, x2.SubSystem } into grouped
                          orderby grouped.Count() descending
                          select new { Type = grouped.Key, Count = grouped.Count() };
            var typenum = 5; //前五多故障設備種類
            if(RepairEquipment.Count() < 5)
            {
                typenum = RepairEquipment.Count();
            }
            int c = 1;
            foreach(var item in RepairEquipment)
            {
                if(c <= typenum)
                {
                    JObject jo = new JObject();
                    jo.Add("label", item.Type.System + " " + item.Type.SubSystem);
                    jo.Add("value", item.Count);
                    Equipment_Type_Rate.Add(jo);
                    c++;
                }
                else
                {
                    break;
                }
            }
            InspectionPlanInformation.Add("Equipment_Type_Rate", Equipment_Type_Rate);
            #endregion

            string result = JsonConvert.SerializeObject(InspectionPlanInformation);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢即時資訊
        public ActionResult CurrentInformation()
        {
            return View();
        }
        #endregion

        #region 巡檢即時位置
        public ActionResult CurrentPosition()
        {
            return View();
        }
        #endregion
    }
}