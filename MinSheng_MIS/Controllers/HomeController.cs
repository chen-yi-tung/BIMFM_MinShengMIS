using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        #region 登入
        public ActionResult Login()
        {
            return View();
        }
        #endregion
    }
}