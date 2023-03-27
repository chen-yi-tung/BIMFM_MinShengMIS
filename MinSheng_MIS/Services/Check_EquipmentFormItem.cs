using MinSheng_MIS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class Check_EquipmentFormItem
    {
        public void CheckEquipmentFormItem()
        {
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            //找尋從以前到七天後的那天 (以前~Today+7)
            DateTime DateTo = DateTime.Today.AddDays(8); //因為是迄所以需+1

            //找出設備保養項目為啟用&產單狀態為0(待產單)&最近應保養日期在Today+7天內&設備狀態不為3(停用)
            var query = from x1 in db.EquipmentMaintainItem
                        join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                        where x1.IsEnable == "1" && x1.IsCreate == false && x1.NextTime < DateTo && x2.EState != "3"
                        select new { x1.EMISN, x1.LastTime, x1.NextTime, x1.Unit, x1.Period};
            var list = query.ToList();
            //新增設備保養單項目
            foreach(var item in list)
            {
                //產出設備保養單項目
                EquipmentMaintainFormItem addEMFI = new EquipmentMaintainFormItem();

                addEMFI.EMFISN = item.EMISN + "_" + item.NextTime?.ToString("yyMMdd");
                addEMFI.EMISN = item.EMISN;
                addEMFI.LastTime = (DateTime)item.LastTime;
                addEMFI.Date = (DateTime)item.NextTime;
                addEMFI.Unit = item.Unit;
                addEMFI.Period = item.Period;
                addEMFI.FormItemState = "1"; //待派工
                addEMFI.StockState = false;

                db.EquipmentMaintainFormItem.Add(addEMFI);
                db.SaveChanges();

                //產出設備保養單項目後需把設備保養項目產單狀態改完true(已產單)
                var EMI = db.EquipmentMaintainItem.Find(item.EMISN);
                EMI.IsCreate = true;
                db.EquipmentMaintainItem.AddOrUpdate(EMI);
                db.SaveChanges();
            }
        }
    }
}