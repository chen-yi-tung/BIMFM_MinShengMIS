using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;

namespace MinSheng_MIS.Models.ViewModels
{
    public class InspectionPlan_ManagementViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();


        #region 巡檢計畫-詳情 DataGrid
        public string InspectationPlan_Read_Data(string IPSN)
        {
            var dic_InPlanState = Surfaces.Surface.InspectionPlanState();
            var dic_Shift = Surfaces.Surface.Shift();
            var dic_EMFIState = Surfaces.Surface.EquipmentMaintainFormItemState();
            var dic_EState = Surfaces.Surface.EState();
            var dic_ERFState = Surfaces.Surface.EquipmentReportFormState();
            var dic_RL = Surfaces.Surface.ReportLevel();

            #region 巡檢計畫
            var IP_SourceTable = db.InspectionPlan.Find(IPSN);
            var IPM_UID = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID);
            List<string> IP_NameList = new List<string>();
            foreach (var item in IPM_UID)
            {
                var IP_Name = db.AspNetUsers.Where(x => x.UserName == item).Select(x => x.MyName).FirstOrDefault();
                IP_NameList.Add(IP_Name);
            }
            var IP_MaintainName = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.MaintainUserID).Select(x => x.MyName).FirstOrDefault();
            var IP_RepairName = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.RepairUserID).Select(x => x.MyName).FirstOrDefault();
            #endregion

            #region 定期保養設備
            JArray ME = new JArray();

            var IPM_SourceTable = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);

            foreach (var IPM_item in IPM_SourceTable)
            {
                var EMFI_SourceTable = db.EquipmentMaintainFormItem.Find(IPM_item.EMFISN);
                var EMI_SourceTable = db.EquipmentMaintainItem.Find(EMFI_SourceTable.EMISN);
                var EI_SourceTable = db.EquipmentInfo.Find(EMI_SourceTable.ESN);
                var MI_SourceTable = db.MaintainItem.Find(EMI_SourceTable.MISN);
                JObject ME_Row = new JObject()
                {
                    { "StockState", EMFI_SourceTable.StockState? "有":"無" },
                    { "FormItemState", dic_EMFIState[EMFI_SourceTable.FormItemState] },
                    { "EMFISN", IPM_item.EMFISN },
                    { "Period", EMFI_SourceTable.Period },
                    { "Unit", EMFI_SourceTable.Unit },
                    { "LastTime", EMFI_SourceTable.LastTime.ToString("yyyy/MM/dd") },
                    { "Date", EMFI_SourceTable.Date.ToString("yyyy/MM/dd") },
                    { "EState", dic_EState[EI_SourceTable.EState] },
                    { "Area", EI_SourceTable.Area },
                    { "Floor", EI_SourceTable.Floor },
                    { "ESN", EMI_SourceTable.ESN },
                    { "EName", EI_SourceTable.EName },
                    { "MIName", MI_SourceTable.MIName }
                };
                ME.Add(ME_Row);
            }
            #endregion

            #region 維修設備
            JArray RE = new JArray();

            var IPR_SourceTable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);

            foreach (var IPR_item in IPR_SourceTable)
            {
                var ERF_SourceTable = db.EquipmentReportForm.Find(IPR_item.RSN);
                var EI_SourceTable = db.EquipmentInfo.Find(ERF_SourceTable.ESN);
                var ERF_Name = db.AspNetUsers.Where(x => x.UserName == ERF_SourceTable.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                JObject RE_Row = new JObject()
                {
                    { "StockState", ERF_SourceTable.StockState?"有":"無" },
                    { "ReportState", dic_ERFState[ERF_SourceTable.ReportState] },
                    { "ESN", ERF_SourceTable.ESN },
                    { "RSN", IPR_item.RSN },
                    { "ReportLevel", dic_RL[ERF_SourceTable.ReportLevel] },
                    { "Date", ERF_SourceTable.Date.ToString("yyyy/MM/dd HH:mm:ss") },
                    { "ReportContent", ERF_SourceTable.ReportContent },
                    { "MyName", ERF_Name },
                    { "EState", dic_EState[EI_SourceTable.EState] },
                    { "Area", EI_SourceTable.Area },
                    { "Floor", EI_SourceTable.Floor },
                    { "EName", EI_SourceTable.EName }
                };
                RE.Add(RE_Row);
            }
            #endregion

            #region 巡檢路線規劃
            JArray IPP = new JArray();

            var IPP_SourceTable = db.InspectionPlanPath.Where(x => x.IPSN == IPSN);

            foreach (var IPP_item in IPP_SourceTable)
            {
                JArray PathSampleOrder_ja = new JArray(); //路徑
                JArray PathSampleRecord_ja = new JArray(); //座標
                JArray MaintainEquipment_ja = new JArray(); //沒用到
                JArray RepairEquipment_ja = new JArray(); //沒用到
                JArray BothEquipment_ja = new JArray(); //沒用到

                #region 路徑標題
                var FI_SourceTable = db.Floor_Info.Find(IPP_item.FSN);
                var AI_SourceTable = db.AreaInfo.Find(FI_SourceTable.ASN);

                JObject PathSample_jo = new JObject()
                {
                    {"PSSN", IPP_item.PSN },
                    {"Area", AI_SourceTable.Area },
                    {"Floor", FI_SourceTable.FloorName },
                    {"ASN", FI_SourceTable.ASN },
                    {"FSN", IPP_item.FSN },
                    {"PathTitle", IPP_item.PathTitle },
                    {"BIMPath", FI_SourceTable.BIMPath },
                    {"Beacon","" }
                }; //路徑資訊
                #endregion

                #region 路線順序
                var IPFP_SourceTable = db.InspectionPlanFloorPath.Where(x => x.PSN == IPP_item.PSN).OrderBy(x => x.FPSN);

                foreach (var IPFP_item in IPFP_SourceTable)
                {
                    PathSampleOrder_ja.Add(IPFP_item.DeviceID.ToString());
                }
                #endregion

                #region 路線呈現
                var DIPP_SourceTable = db.DrawInspectionPlanPath.Where(x => x.PSN == IPP_item.PSN);

                foreach (var DIPP_item in DIPP_SourceTable)
                {
                    JObject XY_Path = new JObject()
                    {
                        { "LocationX", DIPP_item.LocationX },
                        { "LocationY", DIPP_item.LocationY }
                    };
                    PathSampleRecord_ja.Add(XY_Path);
                }
                #endregion

                JObject IPP_Row = new JObject()
                {
                    { "PathSample", PathSample_jo },
                    { "PathSampleOrder", PathSampleOrder_ja },
                    { "PathSampleRecord", PathSampleRecord_ja },
                    { "MaintainEquipment", MaintainEquipment_ja },
                    { "RepairEquipment", RepairEquipment_ja },
                    { "BothEquipment", BothEquipment_ja }
                };
                IPP.Add(IPP_Row);
            }
            #endregion

            JObject Main_jo = new JObject
            {
                { "IPSN", IPSN },
                { "IPName", IP_SourceTable.IPName },
                { "PlanCreateUserID", "" },
                { "PlanDate", IP_SourceTable.PlanDate.ToString("yyyy/MM/dd") },
                { "PlanState", dic_InPlanState[IP_SourceTable.PlanState] },
                { "Shift", dic_Shift[IP_SourceTable.Shift] },
                { "UserID", string.Join("、",IP_NameList) },
                { "MaintainUserID", IP_MaintainName },
                { "RepairUserID", IP_RepairName },
                { "MaintainEquipment", ME },
                { "RepairEquipment", RE },
                { "InspectionPlanPaths", IPP }
            };

            string reString = JsonConvert.SerializeObject(Main_jo);
            return reString;
        }
        #endregion

        #region 巡檢計畫-編輯 DataGrid
        public string InspectationPlan_Edit_Data(string IPSN)
        {
            var dic_InPlanState = Surfaces.Surface.InspectionPlanState();
            var dic_Shift = Surfaces.Surface.Shift();
            var dic_EMFIState = Surfaces.Surface.EquipmentMaintainFormItemState();
            var dic_EState = Surfaces.Surface.EState();
            var dic_ERFState = Surfaces.Surface.EquipmentReportFormState();
            var dic_RL = Surfaces.Surface.ReportLevel();

            #region 巡檢計畫
            var IP_SourceTable = db.InspectionPlan.Find(IPSN);
            //string IP_MaintainID = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.MaintainUserID).Select(x => x.MyName).FirstOrDefault();
            //string IP_RepairID = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.RepairUserID).Select(x => x.MyName).FirstOrDefault();
            //string IP_PlanCreateID = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.PlanCreateUserID).Select(x => x.MyName).FirstOrDefault();
            var IPM_SourceTable = db.InspectionPlanMember.Where(x => x.IPSN == IPSN);
            JArray InsName_ja = new JArray();

            foreach (var IPM_item in IPM_SourceTable)
            {
                InsName_ja.Add(IPM_item.UserID);
            }
            #endregion

            #region 定期保養項目
            var IPMaintain_SourceTable = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);
            JArray ME_ja = new JArray();

            foreach (var IPMaintain_item in IPMaintain_SourceTable)
            {
                var EMFI_SourceTable = db.EquipmentMaintainFormItem.Find(IPMaintain_item.EMFISN);
                var EMI_SourceTable = db.EquipmentMaintainItem.Find(EMFI_SourceTable.EMISN);
                var EI_SourceTable = db.EquipmentInfo.Find(EMI_SourceTable.ESN);
                var MI_SourceTable = db.MaintainItem.Find(EMI_SourceTable.MISN);
                JObject ME_jo = new JObject()
                {
                    {"StockState", EMFI_SourceTable.StockState? "有":"無"},
                    {"FormItemState", dic_EMFIState[EMFI_SourceTable.FormItemState] },
                    {"EMFISN", IPMaintain_item.EMFISN},
                    {"Period", EMFI_SourceTable.Period},
                    {"Unit", EMFI_SourceTable.Unit},
                    {"LastTime", EMFI_SourceTable.LastTime.ToString("yyyy/MM/dd")},
                    {"Date", EMFI_SourceTable.Date.ToString("yyyy/MM/dd")},
                    {"EState", dic_EState[EI_SourceTable.EState]},
                    {"Area", EI_SourceTable.Area},
                    {"Floor", EI_SourceTable.Floor},
                    {"ESN", EMI_SourceTable.ESN},
                    {"EName", EI_SourceTable.EName},
                    {"MIName", MI_SourceTable.MIName}
                };
                ME_ja.Add(ME_jo);
            }

            #endregion

            #region 維修設備
            var IPR_SourceTable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);
            JArray RE_ja = new JArray();
            foreach (var IPR_item in IPR_SourceTable)
            {
                var ERF_SourceTable = db.EquipmentReportForm.Find(IPR_item.RSN);
                var EI_SourceTable = db.EquipmentInfo.Find(ERF_SourceTable.ESN);
                var ERF_Name = db.AspNetUsers.Where(x => x.UserName == ERF_SourceTable.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                JObject RE_Row = new JObject()
                {
                    { "StockState", ERF_SourceTable.StockState?"有":"無" },
                    { "ReportState", dic_ERFState[ERF_SourceTable.ReportState] },
                    { "ESN", ERF_SourceTable.ESN },
                    { "RSN", IPR_item.RSN },
                    { "ReportLevel", dic_RL[ERF_SourceTable.ReportLevel] },
                    { "Date", ERF_SourceTable.Date.ToString("yyyy/MM/dd HH:mm:ss") },
                    { "ReportContent", ERF_SourceTable.ReportContent },
                    { "MyName", ERF_Name },
                    { "EState", dic_EState[EI_SourceTable.EState] },
                    { "Area", EI_SourceTable.Area },
                    { "Floor", EI_SourceTable.Floor },
                    { "EName", EI_SourceTable.EName }
                };
                RE_ja.Add(RE_Row);
            }
            #endregion

            #region 巡檢路線規劃
            JArray IP_ja = new JArray();

            var IPP_SourceTable = db.InspectionPlanPath.Where(x => x.IPSN == IPSN);

            foreach (var IPP_item in IPP_SourceTable)
            {
                JArray PathSampleOrder_ja = new JArray(); //路徑
                JArray PathSampleRecord_ja = new JArray(); //座標
                JArray MaintainEquipment_ja = new JArray(); //沒用到
                JArray RepairEquipment_ja = new JArray(); //沒用到
                JArray BothEquipment_ja = new JArray(); //沒用到

                #region 路徑標題
                var FI_SourceTable = db.Floor_Info.Find(IPP_item.FSN);
                var AI_SourceTable = db.AreaInfo.Find(FI_SourceTable.ASN);

                JObject PathSample_jo = new JObject()
                {
                    {"PSSN", IPP_item.PSN },
                    {"Area", AI_SourceTable.Area },
                    {"Floor", FI_SourceTable.FloorName },
                    {"ASN", FI_SourceTable.ASN },
                    {"FSN", IPP_item.FSN },
                    {"PathTitle", IPP_item.PathTitle },
                    {"BIMPath", FI_SourceTable.BIMPath },
                    {"Beacon","" }
                }; //路徑資訊
                #endregion

                #region 路線順序
                var IPFP_SourceTable = db.InspectionPlanFloorPath.Where(x => x.PSN == IPP_item.PSN).OrderBy(x => x.FPSN);

                foreach (var IPFP_item in IPFP_SourceTable)
                {
                    PathSampleOrder_ja.Add(IPFP_item.DeviceID.ToString());
                }
                #endregion

                #region 路線呈現
                var DIPP_SourceTable = db.DrawInspectionPlanPath.Where(x => x.PSN == IPP_item.PSN);

                foreach (var DIPP_item in DIPP_SourceTable)
                {
                    JObject XY_Path = new JObject()
                    {
                        { "LocationX", DIPP_item.LocationX },
                        { "LocationY", DIPP_item.LocationY }
                    };
                    PathSampleRecord_ja.Add(XY_Path);
                }
                #endregion

                JObject IPP_Row = new JObject()
                {
                    { "PathSample", PathSample_jo },
                    { "PathSampleOrder", PathSampleOrder_ja },
                    { "PathSampleRecord", PathSampleRecord_ja },
                    { "MaintainEquipment", MaintainEquipment_ja },
                    { "RepairEquipment", RepairEquipment_ja },
                    { "BothEquipment", BothEquipment_ja }
                };
                IP_ja.Add(IPP_Row);
            }
            #endregion

            JObject Main_jo = new JObject
            {
                {"IPSN", IPSN},
                {"IPName", IP_SourceTable.IPName },
                {"PlanCreateUserID", IP_SourceTable.PlanCreateUserID },
                {"PlanDate", IP_SourceTable.PlanDate.ToString("yyyy/MM/dd") },
                {"PlanState", dic_InPlanState[IP_SourceTable.PlanState] },
                {"Shift", IP_SourceTable.Shift },
                {"UserID", InsName_ja },
                {"MaintainUserID", IP_SourceTable.MaintainUserID },
                {"RepairUserID", IP_SourceTable.RepairUserID },
                {"MaintainAmount", IP_SourceTable.MaintainAmount },
                {"RepairAmount", IP_SourceTable.RepairAmount },
                {"MaintainEquipment", ME_ja },
                {"RepairEquipment", RE_ja },
                {"InspectionPlanPaths", IP_ja }
            };

            string reString = JsonConvert.SerializeObject(Main_jo);
            return reString;
        }
        #endregion

        #region 巡檢計畫-編輯 update
        public string InspectationPlan_Edit_Update(System.Web.Mvc.FormCollection form)
        {
            /*  前端回傳格式
            IPName: IPName,
            PlanCreateUserID: PlanCreateUserID,
            PlanDate: PlanDate,
            Shift: Shift,
            UserID: UserID,
            MaintainUserID: MaintainUserID,
            RepairUserID: RepairUserID,
            MaintainEquipment: MaintainEquipment,
            RepairEquipment: RepairEquipment,
            InspectionPlanPaths: InspectionPlanPaths
            */
            var IP_SourceTable = db.InspectionPlan.Find(form[""]); //我寫到這
            return "";
        }
        #endregion

        #region 巡檢計畫-紀錄 格式
        public class Root
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRecord InspectionPlanRecord { get; set; }
            public List<InspectionPlanMember> InspectionPlanMember { get; set; }
            public List<EquipmentMaintainRecord> EquipmentMaintainRecordList { get; set; }
            public List<EquipmentRepairRecord> EquipmentRepairRecordList { get; set; }
        }

        public class EquipmentMaintainRecord
        {
            public string IPMSN { get; set; }
            public string IPSN { get; set; }
            public string MaintainState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string ESN { get; set; }
            public string EState { get; set; }
            public string EName { get; set; }
            public string EMFISN { get; set; }
            public string MIName { get; set; }
            public string Period { get; set; }
            public string Unit { get; set; }
            public string LastTime { get; set; }
            public string NextTime { get; set; }
        }

        public class EquipmentRepairRecord
        {
            public string IPRSN { get; set; }
            public string IPSN { get; set; }
            public string RepairState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string RSN { get; set; }
            public string ReportLevel { get; set; }
            public string Date { get; set; }
            public string InformantUserID { get; set; }
            public string ReportContent { get; set; }
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

        public class InspectionPlanMember
        {
            public string MyName { get; set; }
            public string WatchID { get; set; }
            public string PMSN { get; set; }
        }

        public class InspectionPlanRecord
        {
            public string PlanState { get; set; }
            public string MyName { get; set; }
            public string DateOfFilling { get; set; }
            public string InspectionRecord { get; set; }
            public List<string> ImgPath { get; set; }
        }
        #endregion

        #region 巡檢計畫-紀錄 DataGrid
        public string GetJsonForRecord(string IPSN)
        {
            try
            {
                #region 巡檢計畫資訊 
                InspectionPlan inspectionPlan = new InspectionPlan();
                var InsPlan = db.InspectionPlan.Find(IPSN);
                var InsPlanMember = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID);
                List<string> NameList = new List<string>();
                string MyNameString = string.Empty;
                foreach (var item in InsPlanMember)
                {
                    var MyName = db.AspNetUsers.Where(x => x.UserName == item).Select(x => x.MyName).FirstOrDefault();
                    NameList.Add(MyName);
                }
                MyNameString = string.Join("、", NameList);

                inspectionPlan.IPSN = IPSN;
                inspectionPlan.IPName = InsPlan.IPName;
                inspectionPlan.PlanDate = InsPlan.PlanDate.ToString("yyyy/MM/dd");
                var dic_IPS = Surfaces.Surface.InspectionPlanState();
                inspectionPlan.PlanState = dic_IPS[InsPlan.PlanState];
                var dic_Shift = Surfaces.Surface.Shift();
                inspectionPlan.Shift = dic_Shift[InsPlan.Shift];
                inspectionPlan.MyName = MyNameString;
                #endregion

                #region 巡檢完工填報
                InspectionPlanRecord inspectionPlanRecord = new InspectionPlanRecord();
                inspectionPlanRecord.PlanState = dic_IPS[InsPlan.PlanState];
                string InfoName = db.AspNetUsers.Where(x => x.UserName == InsPlan.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                inspectionPlanRecord.MyName = InfoName;
                inspectionPlanRecord.DateOfFilling = InsPlan.DateOfFilling?.ToString("yyyy/MM/dd HH:mm:ss");
                inspectionPlanRecord.InspectionRecord = InsPlan.InspectionRecord;
                List<string> ImgList = new List<string>();
                var pathList = db.CompletionReportImage.Where(x => x.IPSN == IPSN).Select(x => x.ImgPath);
                foreach (var item in pathList)
                {
                    ImgList.Add(item);
                }
                inspectionPlanRecord.ImgPath = ImgList;
                #endregion

                #region 巡檢軌跡紀錄
                List<InspectionPlanMember> ListIP = new List<InspectionPlanMember>();
                var IPM = db.InspectionPlanMember.Where(x => x.IPSN == IPSN);
                foreach (var item in IPM)
                {
                    InspectionPlanMember inspectionPlanMember = new InspectionPlanMember();
                    var ipmName = db.AspNetUsers.Where(x => x.UserName == item.UserID).Select(x => x.MyName).FirstOrDefault();
                    inspectionPlanMember.MyName = ipmName;
                    inspectionPlanMember.WatchID = item.WatchID;
                    inspectionPlanMember.PMSN = item.PMSN;
                    ListIP.Add(inspectionPlanMember);
                }
                #endregion

                #region 設備保養紀錄
                List<EquipmentMaintainRecord> ListEMR = new List<EquipmentMaintainRecord>();
                var EMR = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);
                foreach (var item in EMR)
                {
                    EquipmentMaintainRecord equipmentMaintainRecord = new EquipmentMaintainRecord();
                    equipmentMaintainRecord.IPMSN = item.IPMSN;
                    equipmentMaintainRecord.IPSN = IPSN;
                    var dic_IPMS = Surfaces.Surface.InspectionPlanMaintainState();
                    equipmentMaintainRecord.MaintainState = dic_IPMS[item.MaintainState];
                    var EMFI = db.EquipmentMaintainFormItem.Find(item.EMFISN);
                    var EMI = db.EquipmentMaintainItem.Find(EMFI.EMISN);
                    var EI = db.EquipmentInfo.Find(EMI.ESN);
                    var MI = db.MaintainItem.Find(EMI.MISN);
                    equipmentMaintainRecord.Area = EI.Area;
                    equipmentMaintainRecord.Floor = EI.Floor;
                    equipmentMaintainRecord.ESN = EI.ESN;
                    var dic_EState = Surfaces.Surface.EState();
                    equipmentMaintainRecord.EState = dic_EState[EI.EState];
                    equipmentMaintainRecord.EName = EI.EName;
                    equipmentMaintainRecord.EMFISN = item.EMFISN;
                    equipmentMaintainRecord.MIName = MI.MIName;
                    equipmentMaintainRecord.Unit = EMFI.Unit;
                    equipmentMaintainRecord.Period = EMFI.Period.ToString();
                    equipmentMaintainRecord.LastTime = EMFI.LastTime.ToString("yyyy/MM/dd");
                    equipmentMaintainRecord.NextTime = EMFI.NextTime?.ToString("yyyy/MM/dd");
                    ListEMR.Add(equipmentMaintainRecord);
                }
                #endregion

                #region 設備維修紀錄
                List<EquipmentRepairRecord> ListERR = new List<EquipmentRepairRecord>();
                var IPR = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);
                foreach (var item in IPR)
                {
                    EquipmentRepairRecord equipmentRepairRecord = new EquipmentRepairRecord();
                    equipmentRepairRecord.IPRSN = item.IPRSN;
                    equipmentRepairRecord.IPSN = IPSN;
                    var dic_IPRS = Surfaces.Surface.InspectionPlanRepairState();
                    equipmentRepairRecord.RepairState = dic_IPRS[item.RepairState];
                    var ERF = db.EquipmentReportForm.Find(item.RSN);
                    var EI = db.EquipmentInfo.Find(ERF.ESN);
                    var dic_RL = Surfaces.Surface.ReportLevel();
                    equipmentRepairRecord.ReportLevel = dic_RL[ERF.ReportLevel];
                    equipmentRepairRecord.Area = EI.Area;
                    equipmentRepairRecord.Floor = EI.Floor;
                    equipmentRepairRecord.RSN = item.RSN;
                    equipmentRepairRecord.Date = ERF.Date.ToString("yyyy/MM/dd HH:mm:ss");
                    equipmentRepairRecord.ESN = ERF.ESN;
                    equipmentRepairRecord.EName = EI.EName;
                    var Name = db.AspNetUsers.Where(x => x.UserName == ERF.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                    equipmentRepairRecord.InformantUserID = Name;
                    equipmentRepairRecord.ReportContent = ERF.ReportContent;
                    ListERR.Add(equipmentRepairRecord);
                }
                #endregion

                Root root = new Root();
                root.InspectionPlan = inspectionPlan;
                root.InspectionPlanRecord = inspectionPlanRecord;
                root.InspectionPlanMember = ListIP;
                root.EquipmentMaintainRecordList = ListEMR;
                root.EquipmentRepairRecordList = ListERR;

                string result = JsonConvert.SerializeObject(root);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion
    }
}