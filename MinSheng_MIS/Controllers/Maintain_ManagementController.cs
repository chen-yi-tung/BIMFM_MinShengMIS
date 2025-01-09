using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using OfficeOpenXml;
using System.IO.Pipes;

namespace MinSheng_MIS.Controllers
{
    public class Maintain_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly Maintain_ManagementService _maintainService;
        private readonly DatagridService _datagridService;

        public Maintain_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _maintainService = new Maintain_ManagementService(_db);
            _datagridService = new DatagridService();
        }

        #region 定期保養單管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 定期保養單 審核
        public ActionResult Review()
        {
            string userName = HttpContext.User.Identity.Name;
            using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
            {
                ViewBag.MyName = db.AspNetUsers.Where(a => a.UserName == userName).Select(a => a.MyName).FirstOrDefault();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Audit(Maintain_ManagementAuditViewModel datas)
        {
            try
            {
                var result = _maintainService.MaintainManagement_Audit(datas, User.Identity.Name);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion

        #region 定期保養單 詳情
        public ActionResult Detail(string emfsn)
        {
            ViewBag.emfsn = emfsn;
            return View();
        }
        [HttpGet]
        public ActionResult ReadBody(string emfsn)
        {
            JsonResService<Maintain_ManagementDetailViewModel> result = new JsonResService<Maintain_ManagementDetailViewModel>();
            try
            {
                result = _maintainService.MaintainManagement_Details(emfsn);

                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion

        #region 定期保養單 派工
        [HttpPost]
        public ActionResult Assignment(Maintain_ManagementAssignmentViewModel datas)
        {
            try
            {
                var result = _maintainService.MaintainManagement_Assignment(datas, User.Identity.Name);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion

        #region 定期保養單 匯出
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExportToExcel(FormCollection datas)
        {
            try
            {
                var result = _maintainService.MaintainManagement_Export(datas);

                string filename = $"定期保養單管理_{DateTime.Now.Ticks}.xlsx";

                return File(result,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           filename);
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception ex)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion
    }
}