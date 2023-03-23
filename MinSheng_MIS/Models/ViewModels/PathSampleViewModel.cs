using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class PathSampleViewModel
    {
        public class PathSampleInfo
        {
            public PathSample PathSample { get;set;}
            public List<string> PathSampleOrder { get; set; }
            public List<PathSampleRecord> PathSampleRecord { get; set; }
        }
        public class PathSample
        { 
            public string ASN { get; set; }
            public string FSN { get; set; }
            public string PathTitle { get; set; }
        }
        public class PathSampleRecord
        {
            public string LocationX { get; set; }
            public string LocationY { get; set; }
        }
    }
}