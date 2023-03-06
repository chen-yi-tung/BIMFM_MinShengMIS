using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class NewMaintainItems
    {
        public string System { get; set; }
        public string SubSystem { get; set; }
        public string EName { get; set; }

        public List<MaintainItem> MaintainItem { get; set; }
    }

    public class MaintainItem
    {
        public string MIName { get; set; }
        public string Unit { get; set; }
        public int Period { get; set; }
        public string MaintainItemIsEnable { get; set; } = "1";

    }
}