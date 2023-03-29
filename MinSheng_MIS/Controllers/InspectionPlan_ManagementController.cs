using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using static MinSheng_MIS.Models.ViewModels.ReadInspectionPlanPathData;
using PathSample = MinSheng_MIS.Models.ViewModels.ReadInspectionPlanPathData.PathSample;

namespace MinSheng_MIS.Controllers
{
    public class InspectionPlan_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        InspectionPlan_DataService IP_ds = new InspectionPlan_DataService();
        #region 巡檢計畫管理
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public ActionResult InspectionPlan_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = IP_ds.GetJsonForGrid_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫
        public ActionResult Create()
        {
            return View();
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
                //樓層
                if (!string.IsNullOrEmpty(Equipment.Floor))
                {
                    itemObjects.Add("Floor", Equipment.Floor);
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
                //樓層
                if (!string.IsNullOrEmpty(Equipment.Floor))
                {
                    itemObjects.Add("Floor", Equipment.Floor);
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
            //判斷PathTitle是否為空
            if (!string.IsNullOrEmpty(ppi.PathTitle))
            {
                //PathTitle不為空 找出路徑模板資料
                ps.PSSN = ppi.PathTitle;
                var pathtitle = db.PathSample.Find(ps.PSSN).PathTitle;
                //加入預估巡檢計畫單號
                var plandate = Convert.ToDateTime(ppi.PlanDate);
                var IPSNcount = db.InspectionPlan.Where(x => x.PlanDate == plandate).Count();
                ps.PathTitle = pathtitle + " P" + plandate.ToString("yyMMdd") + (IPSNcount+1);
            }
            else
            {

            }


            string result = JsonConvert.SerializeObject(ppo);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢計畫詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯巡檢計畫
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除巡檢計畫
        public ActionResult Delete()
        {
            return View();
        }
        #endregion

        #region 巡檢紀錄
        public ActionResult Record()
        {
            return View();
        }
        #endregion

        #region 巡檢軌跡紀錄
        public ActionResult TrackRecord()
        {
            return View();
        }
        #endregion

        #region 定期保養單詳情
        public ActionResult ReadMaintainForm()
        {
            return View();
        }
        #endregion

        #region 設備維修單詳情
        public ActionResult ReadReportForm()
        {
            return View();
        }
        #endregion

    }
}