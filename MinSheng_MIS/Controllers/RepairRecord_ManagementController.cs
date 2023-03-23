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

        [HttpGet]
        public ActionResult AuditBody(string id) //上方資料顯示
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }

        [HttpGet]
        public ActionResult GetBufferData(string id) //檢查下方審核資料有沒有草稿(isBuffer = 1)，有就帶資料，沒有就空字串
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.AuditCheckBuffer(id);
            return Content(result, "application/json");
        }

        [HttpPost]
        public ActionResult CreateDataAudit(FormCollection formCollection) //新增審核資料，看是要暫存還是直接新增
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();
            List<HttpPostedFileBase> fileList = new List<HttpPostedFileBase>();
            foreach (string item in Request.Files)
            {
                fileList.Add(Request.Files[item] as HttpPostedFileBase);
            }
            string result = repairRecord_Management_ReadViewModel.CreateAuditData(formCollection,Server, fileList);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢維修紀錄補件
        public ActionResult Supplement(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult SupplementBody(string id) //[補件]的顯示資料與[詳情]都相同除了沒有[維修資料]
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        [HttpGet]
        public ActionResult Supplement_GetData(string id) //取得下方補件資料
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.GetSupplementEditData(id);
            return Content(result, "application/json");
        }

        [HttpPost]
        public ActionResult Supplement_Submit(FormCollection formCollection) //下方補件資料'提交'
        {
            var repairRecord_Management_ReadViewModel = new RepairRecord_Management_ReadViewModel();

            string result = repairRecord_Management_ReadViewModel.UpdateSuppleData(formCollection);
            return Content(result, "application/json");
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