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
            public PathSample PathSample { get; set; }
            public List<string> PathSampleOrder { get; set; }
            public List<PathSampleRecord> PathSampleRecord { get; set; }
        }
        public class PathSample
        {
            public string PSSN { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string ASN { get; set; }
            public string FSN { get; set; }
            public string PathTitle { get; set; }
            public string BIMPath { get; set; }
            public List<BIMDevices> BIMDevices { get; set; }
            public List<BIMDevices> Beacon { get; set; }
        }
        public class PathSampleRecord
        {
            public decimal LocationX { get; set; }
            public decimal LocationY { get; set; }
        }
        public class BIMDevices{
            public int dbId { get; set; }
            public string deviceType { get; set; }
            public string deviceName { get; set; }
        }
    }
}