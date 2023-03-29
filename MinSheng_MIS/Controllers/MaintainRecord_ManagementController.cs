using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class MaintainRecord_ManagementController : Controller
    {

        #region 巡檢保養紀錄管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 查詢巡檢保養紀錄 (詳情)
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var MaintainRecord_Management_ViewModel = new MaintainRecord_Management_ViewModel();

            string result = MaintainRecord_Management_ViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢保養紀錄審核
        public ActionResult Audit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult GetBufferData(string id)
        {
            var MaintainRecord_Management_ViewModel = new MaintainRecord_Management_ViewModel();

            string result = MaintainRecord_Management_ViewModel.GetBufferData(id);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult SubmitAuditData(FormCollection formCollection)
        {
            var MaintainRecord_Management_ViewModel = new MaintainRecord_Management_ViewModel();
            List<HttpPostedFileBase> imglist = new List<HttpPostedFileBase>();
            foreach (string item in Request.Files)
            {
                imglist.Add(Request.Files[item]);
            }
            string result = MaintainRecord_Management_ViewModel.AuditSubmit(formCollection, Server, imglist);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢保養紀錄補件
        public ActionResult Supplement(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Supplement_GetData(string id)
        {
            var MaintainRecord_Management_ViewModel = new MaintainRecord_Management_ViewModel();

            string result = MaintainRecord_Management_ViewModel.Supplement_GetData(id);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult Supplement_Submit(FormCollection formCollection)
        {
            var MaintainRecord_Management_ViewModel = new MaintainRecord_Management_ViewModel();
            List<HttpPostedFileBase> imgList = new List<HttpPostedFileBase>();
            List<HttpPostedFileBase> fileList = new List<HttpPostedFileBase>();
            foreach (string item in Request.Files)
            {
                if (item.Contains("Img"))
                {
                    imgList.Add(Request.Files[item]);
                }
                if (item.Contains("File"))
                {
                    fileList.Add(Request.Files[item]);
                }
            }
            string result = MaintainRecord_Management_ViewModel.Supplement_Submit(formCollection, Server, imgList, fileList);
            return Content(result, "application/json");
        }
        #endregion
    }
}