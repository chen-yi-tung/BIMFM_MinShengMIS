using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class DropDownListController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: DropDownList
        #region 樓層
        [System.Web.Http.HttpGet]
        public ActionResult Area()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.AreaInfo.ToList();
            foreach (var item in abc) {
                JObject jo = new JObject();
                jo.Add("Text", item.Area);//Area Name
                jo.Add("Value", item.ASN); // ASN 
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 根據樓層查詢棟別
        [System.Web.Http.HttpGet]
        public ActionResult Floor(int? ASN)
        {
            List<JObject> list = new List<JObject>();
            if (ASN != null) {
                var abc = db.Floor_Info.Where(x => x.ASN == ASN).ToList();
                foreach (var item in abc)
                {
                    JObject jo = new JObject();
                    jo.Add("Text", item.FloorName);//Floor Name
                    jo.Add("Value", item.FSN); // FSN 
                    list.Add(jo);
                }
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region ReportFormState相關 報修單狀態
        [System.Web.Http.HttpGet]
        public ActionResult Report_Management_Management_ReportFormState(string url = "")
        {
            List<JObject> list = new List<JObject>();
            var Dics = ReportState(url);

            foreach (var a in Dics) {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }
            
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }


        //根據不同當下路由決定不同的下拉式選單
        private static Dictionary<string,string> ReportState(string url="")
        {
            //預設空字串回傳全部 key為
            var abc = Surface.EquipmentReportFormState();
            var result = new Dictionary<string,string>();
            if (url == "") {
                foreach (var a in abc)
                {
                    result.Add(a.Key, a.Value);
                }
            }
            return abc;
        }

        #endregion

        #region ReportLevel 報修等級
        [System.Web.Http.HttpGet]
        public ActionResult ReportLevel()
        {
            List<JObject> list = new List<JObject>();
            var Dics = Surface.ReportLevel();

            foreach (var a in Dics)
            {
                JObject jo = new JObject
                {
                    { "Text", a.Value },
                    { "Value", a.Key }
                };
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region InformantUserID 使用者名稱
        [System.Web.Http.HttpGet]
        public ActionResult InformantUserID()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.AspNetUsers.ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item.MyName);//Area Name
                jo.Add("Value", item.UserName); // ASN 
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 主系統
        [System.Web.Http.HttpGet]
        public ActionResult System()
        {
            List<JObject> list = new List<JObject>();
            var abc = db.EquipmentInfo.Select(x=>x.System).Distinct().ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item);//System
                jo.Add("Value", item); // System
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion

        #region 子系統
        [System.Web.Http.HttpGet]
        public ActionResult SubSystem(string System)
        {
            List<JObject> list = new List<JObject>();
            var abc = db.EquipmentInfo.Where(x=>x.System == System).Select(x => x.SubSystem).Distinct().ToList();
            foreach (var item in abc)
            {
                JObject jo = new JObject();
                jo.Add("Text", item);//SubSystem
                jo.Add("Value", item); // SubSystem
                list.Add(jo);
            }
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #endregion
    }
}