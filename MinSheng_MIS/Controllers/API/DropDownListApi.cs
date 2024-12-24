using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace MinSheng_MIS.Controllers.API
{
    #region 棟別
    public class GetAreaListController : ApiController
    {
        public JObject Get()
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                JArray ja = new JArray();
                using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
                {
                    var areaList = db.AreaInfo.ToList();
                    foreach (var area in areaList)
                    {
                        JObject itemObject = new JObject
                        {
                            { "Text", area.Area },
                            { "Value", area.ASN }
                        };
                        ja.Add(itemObject);
                    }
                    jo["Datas"] = ja;
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion

    #region 樓層
    public class FloorListController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementAppSearchEquipmentViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                JArray ja = new JArray();
                using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
                {
                    var floorList = db.Floor_Info.Where(f => f.ASN == item.ASN).ToList();
                    foreach (var area in floorList)
                    {
                        JObject itemObject = new JObject
                        {
                            { "Text", area.FloorName },
                            { "Value", area.FSN }
                        };
                        ja.Add(itemObject);
                    }
                    jo["Datas"] = ja;
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion

    #region 設備名稱
    public class EquipmentENameListController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementAppSearchEquipmentViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                JArray ja = new JArray();
                using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
                {
                    var equipmentList = db.EquipmentInfo.Where(e => e.Floor_Info.ASN == item.ASN && e.FSN == item.FSN).ToList();
                    foreach (var equipment in equipmentList)
                    {
                        JObject itemObject = new JObject
                        {
                            { "Text", equipment.EName },
                            { "Value", equipment.EName }
                        };
                        ja.Add(itemObject);
                    }
                    jo["Datas"] = ja;
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion



    #region 設備編號
    public class EquipmentNOListController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementAppSearchEquipmentViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                JArray ja = new JArray();
                using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
                {
                    var equipmentList = db.EquipmentInfo.Where(e => e.Floor_Info.ASN == item.ASN && e.FSN == item.FSN && e.EName == item.EName).ToList();
                    foreach (var equipment in equipmentList)
                    {
                        JObject itemObject = new JObject
                        {
                            { "Text", equipment.NO },
                            { "Value", equipment.NO }
                        };
                        ja.Add(itemObject);
                    }
                    jo["Datas"] = ja;
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion

    #region 報修單等級
    public class GetReportLevelListController : ApiController
    {
        public JObject Get()
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                JArray ja = new JArray();
                var Dics = Surface.ReportLevel();

                foreach (var a in Dics)
                {
                    JObject itemObject = new JObject
                    {
                        { "Text", a.Value },
                        { "Value", a.Key }
                    };
                    ja.Add(itemObject);
                }
                jo["Datas"] = ja;

            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion
}