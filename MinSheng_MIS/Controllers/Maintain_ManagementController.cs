using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Maintain_ManagementController : Controller
    {
        #region 定期保養單管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 定期保養單 審核
        public ActionResult Review()
        {
            return View();
        }
        #endregion

        #region 定期保養單 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion
    }
}