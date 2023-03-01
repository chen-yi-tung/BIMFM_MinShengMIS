using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models
{
    public class MaintainItemQuery
    {
        public string MISN { get; set; }
        public string MIName { get; set; }
        public string System { get; set; }
        public string SubSystem { get; set; }
        public string EName { get; set; }
        public string Unit { get; set; }
        public string Period { get; set; }
        public string MaintainItemIsEnable { get; set; }
        public string QueryStr { get; set; }
    }
}