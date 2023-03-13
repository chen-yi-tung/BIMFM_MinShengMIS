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
       
        //public List<SelectListItem> GetAreaList() 
        //{ 
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    var Area = BM.AreaInfo.AsQueryable();
        //    foreach (var item in Area)
        //    {
        //        list.Add(new SelectListItem { Value = item.ASN.ToString(), Text = item.Area.ToString() });
        //    }
        //    return list;
        //}
        //public List<SelectListItem> GetFloorList(string ASN)
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    int SN = Convert.ToInt32(ASN);
        //    var Floor = BM.Floor_Info.Where(x => x.ASN == SN);
        //    foreach (var item in Floor)
        //    {
        //        list.Add(new SelectListItem { Value = item.FSN, Text = item.FloorName });
        //    }
        //    return list;
        //}
        //public List<SelectListItem> GetMaintainStateList()
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    var Dic = Surface.InspectionPlanMaintainState();
        //    foreach (var item in Dic)
        //    {
        //        if (item.Key == "1" || item.Key == "2") { continue; }
        //        list.Add(new SelectListItem { Value = item.Key , Text = item.Value });
        //    }
        //    return list;
        //}
        //public List<SelectListItem> GetESNList()
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    var Equip = BM.EquipmentInfo.AsQueryable();
        //    foreach (var item in Equip)
        //    {
        //        list.Add(new SelectListItem { Value = item.ESN , Text = item.ESN});
        //    }
        //    return list;
        //}
        //public List<SelectListItem> GetENameList()
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    return list;
        //}
        //public List<SelectListItem> GetMaintainUserList()
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    var MTID = from x1 in BM.InspectionPlanMaintain
        //               join x2 in BM.AspNetUsers on x1.MaintainUserID equals x2.UserName
        //               select new { x1.MaintainUserID, x2.MyName };
        //    foreach (var item in MTID)
        //    {
        //        list.Add(new SelectListItem { Value = item.MaintainUserID, Text = item.MyName });
        //    }
        //    return list;
        //}
        //public List<SelectListItem> GetAuditUserList()
        //{
        //    List<SelectListItem> list = new List<SelectListItem>();
        //    var AuditUser = from x1 in BM.InspectionPlanMaintain
        //                    join x2 in BM.MaintainAuditInfo on x1.IPMSN equals x2.IPMSN
        //                    join x3 in BM.AspNetUsers on x2.AuditUserID equals x3.UserName
        //                    select new { x2.AuditUserID,x3.MyName };
        //    foreach (var item in AuditUser)
        //    {
        //        list.Add(new SelectListItem { Value = item.AuditUserID, Text = item.MyName });
        //    }
        //    return list;
        //}
    }
}