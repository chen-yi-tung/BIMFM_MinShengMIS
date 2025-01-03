using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class UserVitalsAndPositionService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public UserVitalsAndPositionService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 新增使用者定位
        public void AddUserPos(double x, double y, string fsn)
        {
            // 使用者名稱
            var userName = HttpContext.Current.User.Identity.Name;

            // 新增使用者定位資訊
            _db.UserPositionData.AddOrUpdate(new UserPositionData
            {
                UserName = userName,
                FSN = fsn,
                Location_X = ((decimal)x),
                Location_Y = ((decimal)y),
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