using Antlr.Runtime.Misc;
using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace MinSheng_MIS.Controllers
{
    public class MaintainItem_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 保養項目管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增保養項目
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateMaintainItem(NewMaintainItems inputItems)
        {
            JsonResponseViewModel model = new JsonResponseViewModel();

            try
            {
                int itemCount = inputItems.MaintainItem.Count();

                #region 檢查待新增保養項目名稱之間是否重複
                List<string> tempList = new List<string>();

                foreach (var item in inputItems.MaintainItem)
                {
                    bool isRepeat = false;
                    string miName = item.MIName;
                    int tempListLength = tempList.Count();
                    for (var i = 0; i < tempListLength; i++)
                    {
                        if (miName == tempList[i])
                        {
                            isRepeat = true;
                            model.ResponseCode = 3;
                            model.ResponseMessage = $"保養項目:{item.MIName} 已重複，請重新命名保養項目";
                            return Json(model);
                        }
                    }

                    if (!isRepeat)
                    {
                        tempList.Add(item.MIName);
                    }
                }

                #endregion

                #region 檢查保養項目名稱是否已存在資料庫內
                bool isItemExist = false;

                foreach (var item in inputItems.MaintainItem)
                {
                    var aaa = db.MaintainItem
                        .Where(m => m.System == inputItems.System)
                        .Where(m => m.SubSystem == inputItems.SubSystem)
                        .Where(m => m.EName == inputItems.EName)
                        .Where(m => m.MIName == item.MIName)
                        .Take(1)
                        .FirstOrDefault();

                    if (aaa != null)
                    {
                        isItemExist = true;
                        model.ResponseCode = 2;
                        model.ResponseMessage = $"曾新增過 保養項目:{item.MIName} ，請重新命名保養項目名稱";
                        return Json(model);
                    }
                }
                #endregion


                var lastItem = db.MaintainItem
                .OrderByDescending(m => m.MISN)
                .Take(1)
                .FirstOrDefault();

                int lastItemIndex = Convert.ToInt32(lastItem.MISN);

                for (int i = 0; i < itemCount; i++)
                {
                    #region 將保養項目新增至 table Maintain Item
                    Models.MaintainItem newItems = new Models.MaintainItem();
                    newItems.System = inputItems.System;
                    newItems.SubSystem = inputItems.SubSystem;
                    newItems.EName = inputItems.EName;

                    string newMISN = (lastItemIndex + 1).ToString();
                    newMISN = newMISN.PadLeft(6, '0');

                    newItems.MISN = newMISN;
                    newItems.MIName = inputItems.MaintainItem[i].MIName;
                    newItems.Unit = inputItems.MaintainItem[i].Unit;
                    newItems.Period = inputItems.MaintainItem[i].Period;
                    newItems.MaintainItemIsEnable = inputItems.MaintainItem[i].MaintainItemIsEnable;
                    lastItemIndex++;
                    db.MaintainItem.AddOrUpdate(newItems);
                    #endregion

                    #region 將目前相關設備(同系統別、子系統別與設備名稱)都加上該新增的保養項目 table Equipment Maintain Item
                    //若設備狀態為3停用則不新增
                    var eqList = db.EquipmentInfo
                        .Where(e => e.System == inputItems.System)
                        .Where(e => e.SubSystem == inputItems.SubSystem)
                        .Where(e => e.EName == inputItems.EName)
                        .Where(e => e.EState != "3")
                        .ToList();

                    int eqNum = eqList.Count();
                    for (int j = 0; j < eqNum; j++)
                    {
                        Models.EquipmentMaintainItem equipmentMaintainItem = new Models.EquipmentMaintainItem();
                        equipmentMaintainItem.MISN = newMISN;
                        equipmentMaintainItem.ESN = eqList[j].ESN;
                        equipmentMaintainItem.EMISN = eqList[j].ESN + "_" + newMISN;
                        equipmentMaintainItem.Unit = inputItems.MaintainItem[i].Unit;
                        equipmentMaintainItem.Period = inputItems.MaintainItem[i].Period;
                        equipmentMaintainItem.IsEnable = "1";
                        equipmentMaintainItem.IsCreate = false;

                        string today = DateTime.Now.ToString("yyyy'-'MM'-'dd");
                        equipmentMaintainItem.LastTime = Convert.ToDateTime(today);
                        equipmentMaintainItem.NextTime = Convert.ToDateTime(today);

                        db.EquipmentMaintainItem.AddOrUpdate(equipmentMaintainItem);
                    }

                    #endregion

                    await db.SaveChangesAsync();

                }

                //檢查該產單的設備保養項目是否產單 沒有的話 就產單
                Check_EquipmentFormItem c = new Check_EquipmentFormItem();
                c.CheckEquipmentFormItem();

                model.ResponseCode = 0;
                model.ResponseMessage = "已新增完成";
                return Json(model);
            }
            catch (Exception ex)
            {
                model.ResponseCode = 1;
                model.ResponseMessage = $"新增失敗\r\n{ex.Message}";
                return Json(model);
            }
        }
        #endregion

        #region 查詢保養項目 (詳情)
        [HttpGet]
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpGet]
        public ActionResult ReadBody(string MISN)
        {
            MISN = MISN.PadLeft(6, '0');
            ReviewEqMaintainItemViewModel viewModel = new ReviewEqMaintainItemViewModel();

            //設備名稱
            var mItem = db.MaintainItem.Where(m => m.MISN == MISN).FirstOrDefault();

            if (mItem == null)
            {
                return HttpNotFound();
            }
            viewModel.System = mItem.System;
            viewModel.SubSystem = mItem.SubSystem;
            viewModel.EName = mItem.EName;
            viewModel.MIName = mItem.MIName;
            viewModel.Unit = mItem.Unit;
            viewModel.Period = mItem.Period;

            var eqList = db.EquipmentMaintainItem
                .Where(x => x.MISN == MISN)
                .Join(db.EquipmentInfo, x1 => x1.ESN, x2 => x2.ESN, (x1, x2) => new { ESN = x1.ESN, Unit = x1.Unit, Period = x1.Period, EState = x2.EState })
                .Where(x => x.EState != "3")
                .ToList();

            if (eqList == null)
            {
                return HttpNotFound();
            }

            int Count = eqList.Count;
            if (Count > 0)
            {
                viewModel.EquipmentMaintainItem = new List<EquipmentMaintainItemInfo>();
                foreach (var item in eqList)
                {
                    viewModel.EquipmentMaintainItem.Add(new EquipmentMaintainItemInfo { ESN = item.ESN, Unit = item.Unit, Period = item.Period });
                }
            }

            return Content(JsonConvert.SerializeObject(viewModel, Formatting.Indented), "application/json");
        }
        #endregion

        #region 編輯保養項目
        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateMaintainItem(UpdateEqMaintainItemViewModel inputItems)
        {
            JsonResponseViewModel model = new JsonResponseViewModel();
            try
            {
                #region 更新保養項目的單位和週期 in table MaintainItem
                var MI = db.MaintainItem.Where(x => x.MISN == inputItems.MISN).FirstOrDefault();
                if (MI == null)
                {
                    return HttpNotFound();
                }

                MI.Unit = inputItems.Unit;
                MI.Period = inputItems.Period;

                db.MaintainItem.AddOrUpdate(MI);
                #endregion

                #region 更新各設備項目的保養單位和週期 in table EquipmentMaintainItem

                int Count = inputItems.EquipmentMaintainItem.Count();
                for (int i = 0; i < Count; i++)
                {
                    string ESN = inputItems.EquipmentMaintainItem[i].ESN;
                    var eqItem = db.EquipmentMaintainItem
                    .Where(em => em.MISN == inputItems.MISN)
                    .Where(em => em.ESN == ESN)
                    .Take(1)
                    .SingleOrDefault();

                    if (eqItem == null)
                    {
                        return HttpNotFound();
                    }

                    eqItem.Unit = inputItems.EquipmentMaintainItem[i].Unit;
                    eqItem.Period = inputItems.EquipmentMaintainItem[i].Period;
                    db.EquipmentMaintainItem.AddOrUpdate(eqItem);
                }

                await db.SaveChangesAsync();

                model.ResponseCode = 0;
                model.ResponseMessage = "更新成功";
                return Json(model);
                #endregion
            }
            catch (Exception ex)
            {
                model.ResponseCode = 1;
                model.ResponseMessage = $"新增失敗\r\n{ex.Message}";
                return Json(model);
            }

        }
        #endregion

        #region 刪除保養項目

        [HttpGet]
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMaintainItem(string MISN)
        {
            JsonResponseViewModel model = new JsonResponseViewModel();

            try
            {
                #region 刪除保養項目 in table MaintainItem
                var MI = db.MaintainItem
                    .Where(mi => mi.MISN == MISN)
                    .SingleOrDefault();

                if (MI == null)
                    return HttpNotFound();

                MI.MaintainItemIsEnable = "2";

                db.MaintainItem.AddOrUpdate(MI);
                #endregion

                #region 刪除各保養設備下的保養項目 in table EquipmentMaintainItem
                var EM = db.EquipmentMaintainItem
                    .Where(em => em.MISN == MISN)
                    .ToList();

                if (EM == null)
                    return HttpNotFound();

                foreach (var item in EM)
                {
                    item.IsEnable = "2";

                    db.EquipmentMaintainItem.AddOrUpdate(item);
                }
                await db.SaveChangesAsync();
                #endregion

                model.ResponseCode = 0;
                model.ResponseMessage = "刪除成功";
                return Json(model);
            }
            catch (Exception ex)
            {
                model.ResponseCode = 1;
                model.ResponseMessage = $"刪除失敗\r\n{ex.Message}";
                return Json(model);
            }
        }
        #endregion
    }
}