using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Models
{
    public class MaintainRecordManagementModel
    {
        public List<SelectListItem> GetAreaList() 
        { 
            List<SelectListItem> list = new List<SelectListItem>();
            return list;
        }
        public List<SelectListItem> GetFloorList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            return list;
        }
        public List<SelectListItem> GetMaintainStateList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            return list;
        }
        public List<SelectListItem> GetESNList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
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