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
    public class EquipmentInfo_ManagementService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public void AddEquipmentInfo(EquipmentInfo_ManagementViewModel eim, string newESN)
        {
            #region 新增設備

            var eiitem = new EquipmentInfo();
            eiitem.ESN = newESN;
            eiitem.FSN = eim.FSN;
            eiitem.RoomName = eim.RoomName;
            eiitem.System = eim.System;
            eiitem.SubSystem = eim.SubSystem;
            eiitem.PropertyCode = eim.PropertyCode;
            eiitem.EName = eim.EName;
            eiitem.Brand = eim.Brand;
            eiitem.Model = eim.Model;
            eiitem.LocationX = eim.LocationX;
            eiitem.LocationY = eim.LocationY;
            eiitem.EState = "1";
            eiitem.Area = db.AreaInfo.Where(a => a.ASN == eim.ASN).FirstOrDefault().Area;
            eiitem.Floor = db.Floor_Info.Where(f => f.FSN == eim.FSN).FirstOrDefault().FloorName;

            db.EquipmentInfo.AddOrUpdate(eiitem);
            db.SaveChanges();
            #endregion
        }
        
    }
}