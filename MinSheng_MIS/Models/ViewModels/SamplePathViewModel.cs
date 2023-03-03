using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class SamplePathViewModel
    {
        public string PathTitle { get; set; }
        public string Area { get; set; }
        public string Floor { get; set; }

        //查詢字串
        public string QueryStr { get; set; }
    }
}