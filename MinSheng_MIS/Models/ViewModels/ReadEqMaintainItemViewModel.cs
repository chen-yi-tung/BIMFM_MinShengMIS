using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ReviewEqMaintainItemViewModel
    {
        public string System { get; set; }
        public string SubSystem { get; set; }

        public string EName { get; set; }

        public string MIName { get; set; }
        public string Unit { get; set; }
        public int Period { get; set; }

        public string JsonStr { get; set; }

        public List<EquipmentMaintainItemInfo> EquipmentMaintainItem { get; set; }

        public string MISN { get; set; }
    }

    public class UpdateEqMaintainItemViewModel
    {
        public string System { get; set; }
        public string SubSystem { get; set; }

        public string EName { get; set; }

        public string MIName { get; set; }
        public string Unit { get; set; }
        public int Period { get; set; }

        public string JsonStr { get; set; }

        public List<EquipmentMaintainItemInfo> EquipmentMaintainItem { get; set; }

        //for Edit
        public string MISN { get; set; }
    }

    public class EquipmentMaintainItemInfo
    {
        public string ESN { get; set; }
        public string Unit { get; set; }
        public int Period { get; set; }

    }
}