using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Data.Entity.Migrations;
using System;
using System.Web;
using System.Configuration;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Collections.Generic;

namespace MinSheng_MIS.Services
{
    public class UserVitalsAndPositionService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly int _maxInspectDwellTime = Convert.ToInt32(ConfigurationManager.AppSettings["FloatAlarm_TimeInterval"]);

        public UserVitalsAndPositionService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 是否停留時間過久
        /// <summary>
        /// 是否停留時間過久
        /// </summary>
        /// <param name="posTime">定位的時間戳記</param>
        /// <returns>停留時間過久(<see cref="true"/>)；反之(<seealso cref="false"/>)</returns>
        /// <remarks>
        /// 以定位使用之Beacon進行判斷，若最大停留期間內用於定位的Beacon編號不變，表示使用者停留於原地。
        /// </remarks>
        public bool IsExcessiveDwellTime(DateTime posTime)
        {
            // 使用者名稱
            var userName = HttpContext.Current.User.Identity.Name;
            // 最大可停留之起始時間
            var startTime = posTime - TimeSpan.FromMinutes(_maxInspectDwellTime);

            // 以posTime抓取_maxInspectDwellTime期間的BeaconData
            var beaconDatas = _db.BeaconData
                .Where(x => x.UserName == userName)
                .Where(x => x.Timestamp <= posTime && x.Timestamp >= startTime);
            // 每個時間點的Beacon編號列表
            var userPosBeacons = beaconDatas
                .GroupBy(x => x.Timestamp)
                .Select(x => new BeaconsAtTime
                {
                    Timestamp = x.Key.Value,
                    Minors = x.Select(b => b.Minor)
                })
                .AsEnumerable();

            // 每個時間點的Beacon標識是否都相同，是則表示定位不變；反之有變動。
            return userPosBeacons.All(x => x.Minors.Equals(userPosBeacons.First().Minors));
        }

        private class BeaconsAtTime
        {
            public DateTime Timestamp;
            public IEnumerable<string> Minors { get; set; }
        }
        #endregion

        #region 新增使用者定位
        public void AddUserPos(IUserPos data)
        {
            // 使用者名稱
            var userName = HttpContext.Current.User.Identity.Name;

            // 新增使用者定位資訊
            _db.UserPositionData.AddOrUpdate(new UserPositionData
            {
                UserName = userName,
                FSN = data.FSN,
                Location_X = ((decimal)data.X),
                Location_Y = ((decimal)data.Y),
                Timestamp = DateTime.Now,
            });
        }
        #endregion

        #region 新增使用者生理狀態
        #endregion

        #region 獲取使用者定位
        #endregion

        #region 獲取使用者生理狀態
        #endregion
    }
}