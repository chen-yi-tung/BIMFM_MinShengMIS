using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using Newtonsoft.Json;

namespace MinSheng_MIS.Controllers
{
    public class MaintainItem_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 保養項目管理
        public ActionResult Management()
        {
            MaintainItemViewModel maintainItemViewModel = new MaintainItemViewModel();

            #region 下拉選單列出所有系統別
            var SystemList = db.EquipmentInfo
                .GroupBy(s => s.System, (key, items) => new SystemItems
                { SystemName = key })
                .ToList();

            if (SystemList == null)
            {
                return HttpNotFound();
            }

            maintainItemViewModel.SystemItemList = SystemList;
            #endregion

            #region 下拉選單列出所有設備資訊
            var MVM = new MaintainItemViewModel();
            var equipmentList = db.EquipmentInfo
                .OrderBy(e => e.ESN)
                .ToList();
            if (equipmentList == null)
            {
                return HttpNotFound();
            }

            if(equipmentList.Count() > 0)
            {
                MVM.EquipmentInfo = new List<EquipmentInfo>();
                foreach (var item in equipmentList)
                {
                    MVM.EquipmentInfo.Add(new EquipmentInfo { ESN = item.ESN, System = item.System, SubSystem = item.SubSystem, EName = item.EName, Area = item.Area, Floor = item.Floor, Room = item.Room, RoomName = item.RoomName });
                }
            }

            var equipmentItemJson = JsonConvert.SerializeObject(MVM, Formatting.Indented);

            maintainItemViewModel.EquipmentInfoList = equipmentItemJson;
            #endregion


            #region 檢索


            #endregion
            return View(maintainItemViewModel);
        }
        #endregion

        #region 新增保養項目
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 查詢保養項目 (詳情)
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯保養項目
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除保養項目
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}