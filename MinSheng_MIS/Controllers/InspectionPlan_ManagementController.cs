using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class InspectionPlan_ManagementController : Controller
    {
        #region 巡檢計畫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增巡檢計畫
        public ActionResult Create()
        {
            return View();
        }
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