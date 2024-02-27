using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using System.Data.Entity.Migrations;

namespace MinSheng_MIS.Controllers
{
    public class MaintainForm_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 定期保養管理
        public ActionResult Management()
        {
            //檢查該產單的設備保養項目是否產單 沒有的話 就產單
            Check_EquipmentFormItem c = new Check_EquipmentFormItem();
            c.CheckEquipmentFormItem();

            return View();
        }
        #endregion

        #region 定期保養詳情
        public ActionResult Read(string id)
        {
            ViewBag.EMFISN = id;
            return View();
        }
        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var readEqMaintainItemFormViewModel = new ReadEqMaintainItemFormViewModel();

            string result = readEqMaintainItemFormViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 定期保養取消保留
        [HttpPost]
        public ActionResult Cancel(string id)
        {
            //取消保留
            var MaintainFormItem = db.EquipmentMaintainFormItem.Find(id);
            switch (MaintainFormItem.FormItemState)
            {
                case "9":
                    MaintainFormItem.FormItemState = "1";
                    break;
                case "10":
                    MaintainFormItem.FormItemState = "5";
                    break;
                case "11":
                    MaintainFormItem.FormItemState = "8";
                    break;
            }
            db.EquipmentMaintainFormItem.AddOrUpdate(MaintainFormItem);
            db.SaveChanges();

            JObject jo = new JObject();
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 定期保養匯出
        [HttpPost]
        public ActionResult Export(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_MaintainForm(form);
            string ctrlName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var result = ComFunc.ExportExcel(Server, a["rows"], ctrlName);

            return Json(result);
        }
        #endregion
    }
}