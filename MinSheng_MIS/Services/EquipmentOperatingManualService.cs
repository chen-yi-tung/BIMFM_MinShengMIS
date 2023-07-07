using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class EquipmentOperatingManualService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public void AddEquipmentOperatingManual(EquipmentOperatingManualViewModel eom, string newEOMSN, string Filename)
        {
            #region 新增設備操作手冊

            var eomitem = new EquipmentOperatingManual();
            eomitem.EOMSN = newEOMSN;
            eomitem.System = eom.System;
            eomitem.SubSystem = eom.SubSystem;
            eomitem.EName = eom.EName;
            eomitem.Brand = eom.Brand;
            eomitem.Model = eom.Model;
            eomitem.FilePath = "/" + Filename;

            db.EquipmentOperatingManual.AddOrUpdate(eomitem);
            db.SaveChanges();
            #endregion
        }
        public void EditEquipmentOperatingManual(EquipmentOperatingManualViewModel eom, string newEOMSN, string Filename)
        {
            #region 編輯設備操作手冊

            var eomitem = db.EquipmentOperatingManual.Find(newEOMSN);
            eomitem.Brand = eom.Brand;
            eomitem.Model = eom.Model;
            if (!string.IsNullOrEmpty(Filename))
            {
                eomitem.FilePath = "/" + Filename;
            }

            db.EquipmentOperatingManual.AddOrUpdate(eomitem);
            db.SaveChanges();
            #endregion
        }
    }
}