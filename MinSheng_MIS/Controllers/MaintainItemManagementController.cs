using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class MaintainItemManagementController : Controller
    {
        // GET: MaintainItemManagement
        public ActionResult Index()
        {
            return View();
        }

        //保養項目管理
        public ActionResult MaintainItem_Management()
        {
            return View();
        }

        //新增保養項目
        public ActionResult MaintainItem_Create()
        {
            return View();
        }

        //查詢保養項目 (詳情)
        public ActionResult MaintainItem_Read()
        {
            return View();
        }

        //編輯保養項目
        public ActionResult MaintainItem_Edit()
        {
            return View();
        }

        //刪除保養項目
        public ActionResult MaintainItem_Delete()
        {
            return View();
        }
    }
}