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
using System.Data.Entity.Migrations;
//using static MinSheng_MIS.Models.ViewModels.PathSampleViewModel;
using System.Web.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        [System.Web.Mvc.HttpPost]
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
        #region 選定棟別樓層 回傳.svf檔的路徑及所有藍芽點
        [System.Web.Mvc.HttpGet]
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

        #region 新增巡檢路線模板api
        [System.Web.Mvc.HttpPost]
        public ActionResult CreateSamplePath(PathSampleViewModel.PathSampleInfo model)
        {

            JObject jo = new JObject();
            jo.Add("ResponseCode", 0);
            string result = JsonConvert.SerializeObject(jo);
            //return Content(result, "application/json");
            return Content("");
        }
        #endregion

        #endregion

        #region 判斷巡檢路線模板名稱是否重複
        [System.Web.Mvc.HttpGet]
        public ActionResult CheckPathTitleISvalid(string id) //PathTitle
        {
            JObject obj = new JObject();
            var existPathTitle = db.PathSample.Where(x => x.PathTitle == id).FirstOrDefault();
            if(existPathTitle != null)
            {
                obj.Add("IsVaild", false);
            }
            else
            {
                obj.Add("IsVaild", true);
            }
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