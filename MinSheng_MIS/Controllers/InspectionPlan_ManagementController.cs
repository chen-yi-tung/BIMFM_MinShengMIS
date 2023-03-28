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
using System.Web;
using System.Web.Mvc;

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
                itemObjects.Add("LastTime", maintainform.LastTime);
                //最近應保養
                itemObjects.Add("Date", maintainform.Date);

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
            }

            JObject jo = new JObject();
            jo.Add("Succed", true);

            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢計畫-新增維修設備
        #endregion

        #region 新增巡檢計畫-刪除維修設備
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