using MinSheng_MIS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentMaintainPeriod_ManagementController : Controller
    {
        #region 設備保養週期管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 設備保養週期管理編輯
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public ActionResult EditBody(string EMISN)
        {
            #region 我要偷懶不建service
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            var SourceTable = from x1 in db.EquipmentMaintainItem
                              join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                              join x3 in db.MaintainItem on x1.MISN equals x3.MISN
                              select new { x1.EMISN, x1.IsEnable, x2.Area, x2.Floor, x2.System, x2.SubSystem, x1.ESN, x2.EName, x1.MISN, x3.MIName, x1.Unit, x1.Period, x1.LastTime, x1.NextTime };
            var resultrow = SourceTable.Where(x => x.EMISN == EMISN && x.IsEnable != "2").FirstOrDefault();
            if (resultrow != null)
            {
                JObject jo = new JObject();
                jo.Add("EMISN", resultrow.EMISN);
                jo.Add("Area", resultrow.Area);
                jo.Add("Floor", resultrow.Floor);
                jo.Add("System", resultrow.System);
                jo.Add("SubSystem", resultrow.SubSystem);
                jo.Add("ESN", resultrow.ESN);
                jo.Add("EName", resultrow.EName);
                jo.Add("MIName", resultrow.MIName);
                jo.Add("LastTime", resultrow.LastTime?.ToString("yyyy/MM/dd"));
                jo.Add("Date", resultrow.NextTime?.ToString("yyyy/MM/dd"));
                jo.Add("Unit", resultrow.Unit);
                jo.Add("Period", resultrow.Period);
                jo.Add("IsEnable", Int16.Parse(resultrow.IsEnable));
                string result = JsonConvert.SerializeObject(jo);
                return Content(result, "application/json");
            }

            return Content("", "application/json");
            #endregion
        }
        public class EditBodyPostModel{
            public string EMISN { get; set; }
            public string Unit { get; set; }
            public string Period { get; set; }
            public string IsEnable { get; set; }
        }
        [System.Web.Http.HttpPost]
        public ActionResult EditBodySave(EditBodyPostModel model)
        {
            #region 我要偷懶不建service
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            var SourceTable = from x1 in db.EquipmentMaintainItem
                              join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                              join x3 in db.MaintainItem on x1.MISN equals x3.MISN
                              select new { x1.EMISN, x1.IsEnable, x2.Area, x2.Floor, x2.System, x2.SubSystem, x1.ESN, x2.EName, x1.MISN, x3.MIName, x1.Unit, x1.Period, x1.LastTime, x1.NextTime };
            string EMISN = model.EMISN;
            var resultrow = SourceTable.Where(x => x.EMISN == EMISN && x.IsEnable != "2").FirstOrDefault();
            string Unit = model.Unit;
            string Period = model.Period;
            string IsEnable = model.IsEnable;
            if (resultrow != null)
            {
                var row = db.EquipmentMaintainItem.Find(EMISN);
                if (row.LastTime != null)
                {
                    int period = int.Parse(Period);
                    row.Period = period;
                    if (Unit == "日")
                        row.NextTime = row.LastTime?.AddDays(period);
                    else if (Unit == "月")
                        row.NextTime = row.LastTime?.AddMonths(period);
                    else if (Unit == "年")
                        row.NextTime = row.LastTime?.AddYears(period);
                }
                row.Unit = Unit;
                row.IsEnable = IsEnable;

                db.EquipmentMaintainItem.AddOrUpdate(row);
                db.SaveChanges();

                JObject jo = new JObject();
                jo.Add("Succeed", true);
                string result = JsonConvert.SerializeObject(jo);
                return Content(result, "application/json");
            }
            Response.StatusCode = 400;
            return Content("找不到此EMISN相關資料");
            #endregion
        }
        #endregion
    }
}