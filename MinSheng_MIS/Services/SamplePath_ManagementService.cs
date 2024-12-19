using MinSheng_MIS.Models;
using System.Collections.Generic;
using System.Linq;

namespace MinSheng_MIS.Services
{
    public class SamplePath_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;

        public SamplePath_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _rfidService = new RFIDService(_db);
        }

        #region 批次刪除巡檢預設順序 InspectionDefaultOrder
        /// <summary>
        /// 批次刪除巡檢預設順序內的特定設備
        /// </summary>
        /// <param name="esn">特定設備列表</param>
        public void DeleteEquipmentInInspectionPathSample(IEnumerable<string> esnList)
        {
            var inspectItems = esnList
                .SelectMany(e => _rfidService.GetRFIDsByEsn(e))
                .SelectMany(x => x.InspectionDefaultOrder)
                .Distinct();

            _db.InspectionDefaultOrder.RemoveRange(inspectItems);
        }
        #endregion
    }
}