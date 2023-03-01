using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models
{
    public class MaintainItemViewModel
    {
        public List<SystemItems> SystemItemList { get; set; }

        public List<EquipmentInfo> EquipmentInfo { get; set; }
        public string EquipmentInfoList { get; set; }
    }

    public class SystemItems
    {
        public string SystemName { get; set; }
    }
}