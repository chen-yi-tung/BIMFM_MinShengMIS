using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class RFIDService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        public RFIDService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 新增設備RFID
        public async Task AddEquipRFIDAsync(EquipRFID data, string esn)
        {
            // 資料驗證
            await RFIDDataAnnotationAsync(data);
            if (string.IsNullOrEmpty(esn))
                throw new ArgumentNullException($"{nameof(esn)}不可為null!");
            if (!await _db.EquipmentInfo.AnyAsync(x => x.ESN == esn))
                throw new ArgumentException($"{nameof(esn)}不存在!");

            // 新增/更新RFID
            var rfid = new RFID
            {
                RFIDInternalCode = data.InternalCode,
                SARSN = null,
                ESN = esn,
                RFIDExternalCode = data.ExternalCode,
                FSN = data.FSN,
                Location_X = data.Location_X,
                Location_Y = data.Location_Y,
                Memo = data.Memo
            };

            _db.RFID.AddOrUpdate(rfid);
        }
        #endregion

        //-----資料驗證
        #region RFID資料驗證
        public async Task RFIDDataAnnotationAsync(IRFIDInfo data)
        {
            var floorSNList = await _db.Floor_Info.Select(x => x.FSN).ToListAsync(); // 取得所有樓層SN列表

            // 不可重複: RFID內碼
            if (await _db.RFID.AnyAsync(x => x.RFIDInternalCode == data.InternalCode))
                throw new MyCusResException("RFID內碼已存在!");
            // 不可重複: RFID外碼
            if (await _db.RFID.AnyAsync(x => x.RFIDExternalCode == data.ExternalCode))
                throw new MyCusResException("RFID外碼已存在!");
            // 關聯性PK是否存在: 樓層
            if (!floorSNList.Contains(data.FSN))
                throw new MyCusResException("樓層不存在!");

            #region 批次驗證(X)
            //// 不可重複: RFID內碼
            //if (await _db.RFID.Where(x => data.RFIDList.Any(r => r.InternalCode == x.RFIDInternalCode)).AnyAsync())
            //    throw new MyCusResException("RFID內碼已存在!");
            //// 不可重複: RFID外碼
            //if (await _db.RFID.Where(x => data.RFIDList.Any(r => r.ExternalCode == x.RFIDExternalCode)).AnyAsync())
            //    throw new MyCusResException("RFID外碼已存在!");
            //// 關聯性PK是否存在: 樓層
            //if (!floorSNList.All(floor => data.RFIDList.Select(x => x.FSN).AsEnumerable().Contains(floor)))
            //    throw new MyCusResException("樓層不存在!");
            #endregion
        }
        #endregion
    }
}