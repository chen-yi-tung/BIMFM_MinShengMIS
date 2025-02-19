using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

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

        #region 取得Beacon位置X、Y軸並組合成Subset
        //[AllowAnonymous]
        [Route("GetBeaconsPosInfo")]
        [HttpPost]
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
                if (subsets?.Any() != true)
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
        #endregion

        #region [X不準確]以Beacon距離計算使用者定位(至少3個Beacon)
        [Route("GetUserPos")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> GetUserPos(BeaconsPosInfoRequestModel data)
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
                if (subsets?.Any() != true)
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

        #endregion

        #region 紀錄使用者生理狀態及定位
        // 心律是否正常
        // 是否停留過久：以偵測到的beacon作為判斷(app抓取最近的3個beacon作為定位計算，若beacon每5公尺布置一個，則移動約2.5公尺抓取的beacon會不一樣)
        [Route("RecordUserVitalsAndPos")]
        [HttpPost]
        public async Task<IHttpActionResult> RecordUserVitalsAndPosAsync(VitalsAndPosViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 紀錄 InspectionTrack 軌跡紀錄
                await _beaconService.AddInspectionTrackAsync(data);

                return Ok(new JsonResService<string>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = null
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
        #endregion
    }
}
