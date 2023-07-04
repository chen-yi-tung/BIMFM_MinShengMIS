using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Models.ViewModels
{
    public class EquipmentInfo_ManagementViewModel
    {
        public string ESN { get; set; }
        public string ASN { get; set;}
        public string FSN { get; set; }
        public string RoomName { get; set; }
        public string System { get; set; }
        public string SubSystem { get; set; }
        public string PropertyCode { get; set; }
        public string EName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal LocationX { get; set; }
        public decimal LocationY { get; set; }
        public HttpPostedFileBase ManualFile { get; set; }
    }
}