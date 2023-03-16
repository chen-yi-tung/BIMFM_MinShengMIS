using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class MaintainForm_ManagementController : Controller
    {
        EquipmentMaintainForm_DataService EMF_ds = new EquipmentMaintainForm_DataService();
        #region 定期保養管理
        public ActionResult Management()
        {
            //檢查該產單的設備保養項目是否產單 沒有的話 就產單
            Check_EquipmentFormItem c = new Check_EquipmentFormItem();
            c.CheckEquipmentFormItem();

            return View();
        }
        [HttpPost]
        public ActionResult MaintainForm_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = EMF_ds.GetJsonForGrid_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 定期保養詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}