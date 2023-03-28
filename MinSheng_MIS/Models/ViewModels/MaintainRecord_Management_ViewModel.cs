using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class MaintainRecord_Management_ViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public class EquipmentMaintainFormItem
        {
            public string FormItemState { get; set; }
            public string EMFISN { get; set; }
            public string MIName { get; set; }
            public string Unit { get; set; }
            public string Period { get; set; }
            public string LastTime { get; set; }
            public string Date { get; set; }
            public string ESN { get; set; }
        }

        public class InspectionPlan
        {
            public string IPSN { get; set; }
            public string IPName { get; set; }
            public string PlanDate { get; set; }
            public string PlanState { get; set; }
            public string Shift { get; set; }
            public string MyName { get; set; }
        }

        public class InspectionPlanList
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanMaintain InspectionPlanMaintain { get; set; }
            public List<MaintainSupplementaryInfo> MaintainSupplementaryInfo { get; set; }
            public List<MaintainAuditInfo> MaintainAuditInfo { get; set; }
        }

        public class InspectionPlanMaintain
        {
            public string MaintainState { get; set; }
            public string MyName { get; set; }
            public string MaintainContent { get; set; }
            public string MaintainDate { get; set; }
            public List<string> ImgPath { get; set; }
        }

        public class MaintainAuditInfo
        {
            public string MyName { get; set; }
            public string AuditDate { get; set; }
            public string AuditResult { get; set; }
            public string AuditMemo { get; set; }
            public List<string> ImgPath { get; set; }
        }

        public class MaintainSupplementaryInfo
        {
            public string MyName { get; set; }
            public string SupplementaryDate { get; set; }
            public string SupplementaryContent { get; set; }
            public List<string> FilePath { get; set; }
        }

        public class Root
        {
            public EquipmentMaintainFormItem EquipmentMaintainFormItem { get; set; }
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanMaintain InspectionPlanMaintain { get; set; }
            public List<MaintainSupplementaryInfo> MaintainSupplementaryInfo { get; set; }
            public List<MaintainAuditInfo> MaintainAuditInfo { get; set; }
            public List<InspectionPlanList> InspectionPlanList { get; set; }
        }


        public string GetJsonForRead(string IPMSN)
        {
            Root root = new Root();
            var IPM = db.InspectionPlanMaintain.Find(IPMSN);
            var EMF = db.EquipmentMaintainFormItem.Find(IPM.EMFISN);
            var EMI = db.EquipmentMaintainItem.Find(EMF.EMISN);
            var MI = db.MaintainItem.Find(EMI.MISN);

            #region  設備保養單項目
            EquipmentMaintainFormItem equipmentMaintainFormItem = new EquipmentMaintainFormItem();
            var dicFIS = Surfaces.Surface.EquipmentMaintainFormItemState();
            equipmentMaintainFormItem.FormItemState = dicFIS[EMF.FormItemState];
            equipmentMaintainFormItem.EMFISN = IPM.EMFISN;
            equipmentMaintainFormItem.MIName = MI.MIName;
            equipmentMaintainFormItem.Unit = EMF.Unit;
            equipmentMaintainFormItem.Period = EMF.Period.ToString();
            equipmentMaintainFormItem.LastTime = EMF.LastTime.ToString("yyyy/MM/dd");
            equipmentMaintainFormItem.Date = EMF.Date.ToString("yyyy/MM/dd");
            equipmentMaintainFormItem.ESN = EMI.ESN;
            #endregion

            #region  計劃資訊
            InspectionPlan inspectionPlan = new InspectionPlan();
            var IP = db.InspectionPlan.Find(IPM.IPSN);
            inspectionPlan.IPSN = IP.IPSN;
            inspectionPlan.IPName = IP.IPName;
            inspectionPlan.PlanDate = IP.PlanDate.ToString("yyyy/MM/dd");
            var dicPS = Surfaces.Surface.InspectionPlanState();
            inspectionPlan.PlanState = dicPS[IP.PlanState];
            inspectionPlan.Shift = IP.Shift;
            var IDlist = db.InspectionPlanMember.Where(x => x.IPSN == IP.IPSN).Select(x => x.UserID);
            string Namelist = "";
            foreach (string ID in IDlist)
            {
                var mname = db.AspNetUsers.Where(x => x.UserName == ID).Select(x => x.MyName).FirstOrDefault();
                if (mname != null)
                {
                    Namelist += mname + "、";
                }
            }
            if (IDlist.Count() > 0)
            {
                Namelist.Remove(Namelist.Length - 1);
            }
            inspectionPlan.MyName = Namelist; 
            #endregion

            #region 保養填報
            InspectionPlanMaintain inspectionPlanMaintain = new InspectionPlanMaintain();
            var dicMS = Surfaces.Surface.InspectionPlanMaintainState();
            inspectionPlanMaintain.MaintainState = dicMS[IPM.MaintainState];
            var mName = db.AspNetUsers.Where(x => x.UserName == IPM.MaintainUserID).Select(x => x.MyName).FirstOrDefault();
            inspectionPlanMaintain.MyName = mName;
            inspectionPlanMaintain.MaintainContent = IPM.MaintainContent;
            inspectionPlanMaintain.MaintainDate = IPM.MaintainDate.ToString("yyyy/MM/dd HH:mm:ss");
            var ImgP = db.MaintainCompletionImage.Where(x => x.IPMSN == IPM.IPMSN).Select(x => x.ImgPath);
            List<string> Imglist = new List<string>();
            foreach (string Path in ImgP)
            {
                Imglist.Add(Path);
            }
            inspectionPlanMaintain.ImgPath = Imglist;
            #endregion

            #region 補件資料
            List<MaintainSupplementaryInfo> maintainSupplementaryInfoList = new List<MaintainSupplementaryInfo>();
            var MSI = db.MaintainSupplementaryInfo.Where(x => x.IPMSN == IPMSN);
            foreach (var item in MSI)
            {
                var SName = db.AspNetUsers.Where(x => x.UserName == item.SupplementaryUserID).Select(x => x.MyName).FirstOrDefault();
                
                List<string> FPlist = new List<string>();
                var AllSupFileP = db.MaintainSupplementaryFile.Where(x => x.PMSN == item.PMSN).Select(x => x.FilePath);
                foreach (string Path in AllSupFileP)
                {
                    FPlist.Add(Path);
                }

                MaintainSupplementaryInfo maintainSupIn = new MaintainSupplementaryInfo()
                { 
                    MyName = SName,
                    SupplementaryDate = item.SupplementaryDate.ToString("yyyy/MM/dd"),
                    SupplementaryContent = item.SupplementaryContent,
                    FilePath = FPlist
                };
                maintainSupplementaryInfoList.Add(maintainSupIn);
            }
            #endregion

            #region 審核資料
            List<MaintainAuditInfo> maintainAuditInfoList = new List<MaintainAuditInfo>();
            var MAI = db.MaintainAuditInfo.Where(x => x.IPMSN == IPMSN);
            foreach (var item in MAI)
            {
                var AuName = db.AspNetUsers.Where(x => x.UserName == item.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                
                List<string> IPlist = new List<string>();
                var AllImgP = db.MaintainAuditImage.Where(x => x.PMASN == item.PMASN).Select(x => x.ImgPath);
                foreach (string Path in AllImgP)
                {
                    IPlist.Add(Path);
                }

                MaintainAuditInfo maintainAuIn = new MaintainAuditInfo()
                { 
                    MyName = AuName,
                    AuditDate = item.AuditDate.ToString("yyyy/MM/dd"),
                    AuditResult = item.AuditResult,
                    AuditMemo = item.AuditMemo,
                    ImgPath = IPlist
                };
                maintainAuditInfoList.Add(maintainAuIn);
            }
            #endregion

            root.EquipmentMaintainFormItem = equipmentMaintainFormItem;
            root.InspectionPlan = inspectionPlan;
            root.InspectionPlanMaintain = inspectionPlanMaintain;
            root.MaintainSupplementaryInfo = maintainSupplementaryInfoList;
            root.MaintainAuditInfo = maintainAuditInfoList;
            //root.InspectionPlanList = ;

            string result = JsonConvert.SerializeObject(root);
            return result;
        }
    }
}