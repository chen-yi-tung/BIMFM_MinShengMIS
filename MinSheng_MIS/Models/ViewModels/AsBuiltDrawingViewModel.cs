using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class AsBuiltDrawingViewModel
    {
        public int ASN { get; set; }
        public string FSN { get; set; }
        public int DSystemID { get; set; }
        public string DSubSystemID { get; set; }
        public string ImgNum { get; set; }
        public string ImgName { get; set; }
        public string ImgVersion { get; set; }
        public HttpPostedFileBase ImgPath { get; set; }
    }
}