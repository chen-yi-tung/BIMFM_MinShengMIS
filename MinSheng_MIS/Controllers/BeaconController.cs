using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class BeaconController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly BeaconService _beaconService;

        public BeaconController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _beaconService = new BeaconService(_db);
        }

        #region 取得樓層Beacon列表
        public ActionResult GetFloorBeacons(string FSN)
        {
            try
            {
                var beacons = _beaconService.GetBeaconsBimInfo<BeaconBimInfo>(FSN);

                return Content(JsonConvert.SerializeObject(new JsonResService<IEnumerable<BeaconBimInfo>>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = beacons,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }

        private class BeaconBimInfo
        {
            public string GUID { get; set; }
            public int? DBID { get; set; }
            public string ElementID { get; set; }
            public string DeviceName
            {
                get => Minor;
                set => Minor = value;
            } // 將 Minor 映射到 DeviceName
            [JsonIgnore]
            public string Minor { get; set; }
        }
        #endregion
    }
}