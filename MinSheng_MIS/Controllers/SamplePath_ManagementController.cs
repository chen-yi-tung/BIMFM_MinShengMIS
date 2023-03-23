using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;

namespace MinSheng_MIS.Controllers
{
    public class SamplePath_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        SamplePath_DataService SP_ds = new SamplePath_DataService();

        #region 巡檢路線模板管理
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SamplePath_Management(FormCollection form)
        {
            var a = SP_ds.GetJsonForGrid_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢路線模板
        public ActionResult Create()
        {
            return View();
        }
        //選定棟別樓層 回傳.svf檔的路徑及所有藍芽點
        [HttpGet]
        public ActionResult ReadBimPathDevices(string id) //FSN
        {
            JObject obj = new JObject();
            //找出BimPath
            var BimPath = db.Floor_Info.Where(x => x.FSN == id).Select(x => x.BIMPath).FirstOrDefault().ToString();
            obj.Add("BIMPath", BimPath);
            //找出該樓層所有藍芽
            var BeaconList = db.EquipmentInfo.Where(x => x.FSN == id && x.EName == "藍芽").ToList();
            JArray ja = new JArray();
            foreach(var item in BeaconList)
            {
                JObject jo = new JObject();
                jo.Add("dbId", Convert.ToInt32(item.DBID));
                jo.Add("deviceType", item.EName);
                jo.Add("deviceName", item.ESN);

                ja.Add(jo);
            }
            obj.Add("BIMDevices", ja);

            string result = JsonConvert.SerializeObject(obj);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢路線模板詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯巡檢路線模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除巡檢路線模板
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}