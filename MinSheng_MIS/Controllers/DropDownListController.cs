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
        // GET: DropDownList
        [System.Web.Http.HttpGet]
        public ActionResult Area()
        {
            List<JObject> list = new List<JObject>();

            JObject jo = new JObject();
            jo.Add("Text", "進流抽水站");//Area Name
            jo.Add("Value", "1"); // ASN 

            list.Add(jo);
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        [System.Web.Http.HttpGet]
        public ActionResult Floor(string ASN)
        {
            List<JObject> list = new List<JObject>();

            JObject jo = new JObject();
            jo.Add("Text", "B1F");//Floor Name
            jo.Add("Value", "1_1"); // FSN 

            list.Add(jo);
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        [System.Web.Http.HttpGet]
        public ActionResult Report_Management_Management_ReportState()
        {
            List<JObject> list = new List<JObject>();

            JObject jo = new JObject
            {
                { "Text", "待派工" },//Floor Name
                { "Value", "1" } // FSN 
            };
            list.Add(jo);
            jo = new JObject
            {
                { "Text", "B1F" },//Floor Name
                { "Value", "1_1" } // FSN 
            };
            list.Add(jo);
            string text = JsonConvert.SerializeObject(list);
            return Content(text, "application/json");
        }
        #region reportstate相關

        //根據不同當下路由決定不同的下拉式選單
        private static Dictionary<string,string> ReportState(string url="")
        {
            //預設空字串回傳全部 key為
            Dictionary<string, string> abc =   new Dictionary<string, string>();
            abc.Add("URL", url);
            return abc;
        }

        //根據輸入的數值回傳對照表
        private static string ReportStatefullName(string ReportState) {
            return "";
        }
        #endregion
    }
}