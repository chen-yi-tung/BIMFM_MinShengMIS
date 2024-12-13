using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Repair_ManagementController : Controller
    {
        #region 頁面
        #region 報修管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 報修單
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 報修單 審核
        public ActionResult Review()
        {
            return View();
        }
        #endregion

        #region 報修單 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion
        #endregion

        #region API
        #region 報修管理 DataGrid
        [HttpPost]
        public ActionResult ManagementDataGrid(FormCollection form)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                DatagridService ds = new DatagridService();
                jo["Datas"] = ds.RepairManagementDataGrid(form);
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 新增 報修單
        [HttpPost]
        public ActionResult Create(Repair_ManagementCreateViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.CreateFromWeb(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修單 派工
        [HttpPost]
        public ActionResult Assignment(Repair_ManagementAssignmentViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.Assignment(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修單 詳情
        [HttpPost]
        public ActionResult Detail(string rsn)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    jo["Datas"] = ds.Detail(rsn);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修單 審核
        [HttpPost]
        public ActionResult Audit(Repair_ManagementAuditViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.Audit(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion
        #endregion
    }
}