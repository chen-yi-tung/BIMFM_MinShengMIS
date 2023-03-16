using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class RepairRecord_ManagementController : Controller
    {
        #region 巡檢維修紀錄管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 查詢巡檢維修紀錄 (詳情)
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢維修紀錄審核
        public ActionResult Audit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        #endregion

        #region 
        public ActionResult AuditBody(string id) //上面表格部分和詳情的資料都相同，只差下面編輯的部分要做
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        #endregion

        //[補件]的顯示資料與[詳情]都相同除了沒有[維修資料]

        #region 巡檢維修紀錄補件
        public ActionResult Supplement()
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

        #region 設備報修單詳情
        public ActionResult ReadReportForm()
        {
            return View();
        }
        #endregion

    }
}