using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Models.ViewModels
{
    public class EquipmentOperatingManualViewModel
    {
        public string System { get; set;}
        public string SubSystem { get; set; }
        public string EName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public HttpPostedFileBase ManualFile { get; set; }
    }
}