using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Instrumentation;
using static MinSheng_MIS.Models.ViewModels.PathSampleViewModel;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ReadInspectionPlanPathData
    {
        public class postPathData
        {
            public string PlanDate { get; set; }
            public string ASN { get; set; }
            public string FSN { get; set; }
            public string PathTitle { get; set; }
        }
        public class postResPathData
        {
            public PathSample PathSample { get; set; }
            public List<string> PathSampleOrder { get; set; }
            public List<PathSampleRecord> PathSampleRecord { get; set; }
        }

        public class PlanPathInput
        {
            public string PlanDate { get; set; }
            public string ASN { get; set; }
            public string FSN { get; set; }
            public string PathTitle { get; set; }
            public List<string> MaintainEquipment { get; set; }
            public List<string> RepairEquipment { get; set; }
        }
        public class PlanPathOutput
        {
            public PathSample PathSample { get; set; }
            public List<string> PathSampleOrder { get; set; }
            public List<PathSampleRecord> PathSampleRecord { get; set; }
            public List<MaintainEquipment> MaintainEquipment { get; set; }
            public List<RepairEquipment> RepairEquipment { get; set; }
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
            public List<Beacon> Beacon { get; set; }
        }
        public class PathSampleRecord
        {
            public decimal LocationX { get; set; }
            public decimal LocationY { get; set; }
        }
        public class Beacon
        {
            public int dbId { get; set; }
            public string deviceType { get; set; }
            public string deviceName { get; set; }
        }

        public class MaintainEquipment
        {
            public string ESN { get; set; }
            public int DBID { get; set; }
            public Position Position { get; set; }
        }

        public class RepairEquipment
        {
            public string ESN { get; set; }
            public int DBID { get; set; }
            public Position Position { get; set; }
        }
        public class Position
        {
            public decimal LocationX { get; set; }
            public decimal LocationY { get; set; }
        }
    }
}