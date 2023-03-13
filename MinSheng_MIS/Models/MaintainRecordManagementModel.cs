using MinSheng_MIS.Surfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Models
{
    public class MaintainRecordManagementModel
    {
        Bimfm_MinSheng_MISEntities BM = new Bimfm_MinSheng_MISEntities();
       
        public List<SelectListItem> GetAreaList() 
        { 
            List<SelectListItem> list = new List<SelectListItem>();
            var Area = BM.AreaInfo.AsQueryable();
            foreach (var item in Area)
            {
                list.Add(new SelectListItem { Text = item.ASN.ToString(), Value = item.Area.ToString() });
            }
            return list;
        }
        public List<SelectListItem> GetFloorList(string ASN)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var Floor = BM.Floor_Info.Where(x => x.ASN == Convert.ToInt32(ASN));
            foreach (var item in Floor)
            {
                list.Add(new SelectListItem { Text = item.FSN,Value = item.FloorName });
            }
            return list;
        }
        public List<SelectListItem> GetMaintainStateList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var Dic = Surface.InspectionPlanMaintainState();
            foreach (var item in Dic)
            {
                if (item.Key == "1" || item.Key == "2") { continue; }
                list.Add(new SelectListItem { Text = item.Key , Value = item.Value });
            }
            return list;
        }
        public List<SelectListItem> GetESNList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var Equip = BM.EquipmentInfo.AsQueryable();
            foreach (var item in Equip)
            {
                list.Add(new SelectListItem { Text = item.ESN , Value = item.ESN});
            }
            return list;
        }
        public List<SelectListItem> GetENameList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            return list;
        }
        public List<SelectListItem> GetMaintainUserList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            return list;
        }
        public List<SelectListItem> GetAuditUserList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            return list;
        }
    }
}