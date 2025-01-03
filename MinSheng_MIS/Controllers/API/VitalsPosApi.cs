using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;

namespace MinSheng_MIS.Controllers.API
{
    [RoutePrefix("api")]
    public class VitalsPosApiController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly BeaconService _beaconService;

        public VitalsPosApiController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _beaconService = new BeaconService(_db);
        }

        [Route("GetBeaconsPosInfo")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> GetBeaconsPosInfo(BeaconsPosInfoRequestModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 紀錄BeaconData
                await _beaconService.AddBeaconDatasAsync(data.Beacons, data.Timestamp.Value);

                // 獲取Beacon Position
                var beacons = await _beaconService.GetBeaconPositionAsync<BeaconPosition>(data.Beacons);

                // 篩選有效Beacon
                beacons = _beaconService.GetValidateBeacon(beacons, true);

                // 將有效Beacon組合成子集合
                var subsets = BeaconService.GenerateSubsets(beacons, 3)?
                    .Concat(BeaconService.GenerateSubsets(beacons, 4));
                if (subsets?.Count() < 3)
                    throw new MyCusResException("未偵測到至少三個有效Beacon，無法進行定位計算！");

                return Ok(new JsonResService<BeaconsPosInfoResultModel>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = new BeaconsPosInfoResultModel
                    {
                        BeaconSubset = subsets,
                        FSN = subsets.First().First().FSN,
                        Timestamp = data.Timestamp.Value
                    }
                });
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

        [Route("GetUserPos")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> GetUserPos(BeaconsPosInfoRequestModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                var userName = HttpContext.Current.User.Identity.Name;

                // 紀錄BeaconData
                await _beaconService.AddBeaconDatasAsync(data.Beacons, data.Timestamp.Value);

                // 獲取Beacon Position
                var beacons = await _beaconService.GetBeaconPositionAsync<BeaconPosition>(data.Beacons);

                // 篩選有效Beacon
                beacons = _beaconService.GetValidateBeacon(beacons, true);

                // 將有效Beacon組合成子集合
                var subsets = BeaconService.GenerateSubsets(beacons, 3)?
                    .Concat(BeaconService.GenerateSubsets(beacons, 4));
                if (subsets?.Count() < 3)
                    throw new MyCusResException("未偵測到至少三個有效Beacon，無法進行定位計算！");

                // 計算子集合的中心座標
                var result = new List<(double, double)>();
                foreach (var subset in subsets)
                {
                    var coordinate = BeaconService.CalculateDevicePosition(subset.ToList());
                    if (coordinate == null) continue;
                    result.Add(coordinate.Value);
                }

                if (result?.Any() != true)
                    throw new MyCusResException("定位計算失敗！");

                return Ok(new JsonResService<object>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = new
                    {
                        avgX = result.Average(p => p.Item1),
                        avgY = result.Average(p => p.Item2)
                    }
                });
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
    }


    public class BeaconController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly BeaconService _beaconService;

        public BeaconController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _beaconService = new BeaconService(_db);
        }

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
    }
}
