using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Inquiry_ManagementController : Controller
    {
        // GET: Inquiry_Management
        #region 詢價計畫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增詢價計畫
        public ActionResult CreatePlan()
        {
            return View();
        }
        #endregion

        #region 詢價計畫詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯詢價計畫
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 新增詢價
        public ActionResult CreateInquiry()
        {
            return View();
        }
        #endregion

        #region 詢價管理
        public ActionResult InquiryManagement()
        {
            return View();
        }
        #endregion
    }
}