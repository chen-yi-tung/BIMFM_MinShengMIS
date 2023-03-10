using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Models.ViewModels
{
    public class MaintainRecordManagementViewModel
    {
        public class Management
        {
            public List<SelectListItem> AreaList { get; set; }
            public string ASN { get; set; }
            public List<SelectListItem> FloorList { get; set; }
            public string FSN { get; set; }
            public List<SelectListItem> MaintainStateList { get; set; }
            public string MaintainState { get; set; }
            public List<SelectListItem> ESNList { get; set; }
            public string ESN { get; set; }
            public List<SelectListItem> ENameList { get; set; }
            public string EName { get; set; }
            public List<SelectListItem> MaintainUserIDList { get; set; }
            public string MaintainUserID { get; set; }
            public List<SelectListItem> AuditUserIDList { get; set; }
            public string AuditUserID { get; set; }
        }
    }
}