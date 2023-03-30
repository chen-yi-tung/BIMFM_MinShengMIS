using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using static MinSheng_MIS.Models.ViewModels.MaintainRecord_Management_ViewModel;

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
            var dicShift = Surfaces.Surface.Shift();
            inspectionPlan.Shift = dicShift[IP.Shift];
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
                Namelist = Namelist.Remove((Namelist.Length - 1), 1);
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
                var dicAR = Surfaces.Surface.AuditResult();
                MaintainAuditInfo maintainAuIn = new MaintainAuditInfo()
                { 
                    MyName = AuName,
                    AuditDate = item.AuditDate.ToString("yyyy/MM/dd"),
                    AuditResult = dicAR[item.AuditResult] ,
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

        public class AuditBufferData
        { 
            public string AuditMemo { get; set; }
            public List<string> ImgPath { get; set; }
            public string AuditResult { get; set; }
        }

        public string GetBufferData(string IPMSN)
        {
            var MAI = db.MaintainAuditInfo.Where(x => x.IPMSN == IPMSN).Where(x => x.IsBuffer == true).FirstOrDefault();
            if (MAI != null)
            {
                var MAImg = db.MaintainAuditImage.Where(x => x.PMASN == MAI.PMASN).Select(x => x.ImgPath);
                List<string> ImgList = new List<string>();
                foreach (string path in MAImg)
                {
                    ImgList.Add(path);
                }
                var dic = Surfaces.Surface.AuditResult();
                AuditBufferData auditData = new AuditBufferData()
                { 
                    AuditMemo = MAI.AuditMemo,
                    ImgPath = ImgList,
                    AuditResult = dic[MAI.AuditResult]
                };
                string result = JsonConvert.SerializeObject(auditData);
                return result;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// IPMSN type = text
        /// IsBuffer type = text 判斷使用者點擊[暫存]或是[儲存]
        /// img1、2、... type = file  可多筆
        /// AuditUserID type = text 
        /// AuditMemo type = text
        /// AuditResult type = text
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Sev"></param>
        /// <param name="imgList"></param>
        /// <returns></returns>
        public string AuditSubmit(System.Web.Mvc.FormCollection form, HttpServerUtilityBase Sev, List<HttpPostedFileBase> imgList)
        {
            string ipmsn = form["IPMSN"].ToString();
            RepairRecord_Management_ReadViewModel RRMVM = new RepairRecord_Management_ReadViewModel();
           
            bool IsBu = false; 
            switch (form["IsBuffer"].ToString()) //判斷是否暫存資料
            {
                case "0":
                    IsBu = false;
                    break;
                case "1":
                    IsBu = true;
                    break;
            }

            var MAI = db.MaintainAuditInfo.Where(x => x.IPMSN == ipmsn).FirstOrDefault(); //先看有沒有暫存資料

            if (MAI != null) 
            {
                MAI.AuditUserID = form["AuditUserID"].ToString().Trim();
                MAI.AuditMemo = form["AuditMemo"].ToString().Trim();
                MAI.AuditResult = form["AuditResult"].ToString();
                MAI.IsBuffer = IsBu;
                MAI.AuditDate = DateTime.Now;
                MAI.IPMSN = ipmsn;

                db.MaintainAuditInfo.AddOrUpdate(MAI);
                db.SaveChanges();
            }
            else
            {
                Models.MaintainAuditInfo MainAuIn = new Models.MaintainAuditInfo()
                {
                    AuditUserID = form["AuditUserID"].ToString().Trim(),
                    AuditMemo = form["AuditMemo"].ToString().Trim(),
                    AuditResult = form["AuditResult"].ToString(),
                    IsBuffer = IsBu,
                    AuditDate = DateTime.Now,
                    IPMSN = ipmsn,
                    PMASN = ipmsn + "_01"
                };

                db.MaintainAuditInfo.Add(MainAuIn);
                db.SaveChanges();
            }

            string IPM_AuditResult = "";
            string IPM_State = "";
            string EMFI_State = "";
            string EMI_IsCreate = "";

            switch (form["AuditResult"].ToString())
            {
                case "1":
                    IPM_AuditResult = "1";
                    IPM_State = "6";
                    EMFI_State= "7";
                    EMI_IsCreate = "0";
                    break;
                case "2":
                    IPM_AuditResult = "2";
                    IPM_State = "7";
                    EMFI_State = "8";
                    break;
                case "3":
                    IPM_AuditResult = "3";
                    IPM_State = "5";
                    EMFI_State = "6";
                    break;
            }

            var IPM = db.InspectionPlanMaintain.Find(ipmsn);
            IPM.MaintainState = IPM_State;
            db.InspectionPlanMaintain.AddOrUpdate(IPM);
            var EMFI = db.EquipmentMaintainFormItem.Find(IPM.EMFISN);
            EMFI.FormItemState = EMFI_State;
            if (EMI_IsCreate == "0")
            {
                var EMI = db.EquipmentMaintainItem.Find(EMFI.EMISN);
                EMI.IsCreate = false;
            }
            db.SaveChanges();

            //儲存照片
            List<string> ImgsPath = new List<string>();
            foreach (var item in imgList) 
            {
                string result = RRMVM.UploadFile(item, Sev);
                if (result != "")
                {
                    ImgsPath.Add(result);
                }
                else
                {
                    return "檔案上傳過程出錯!";
                }
            }
            //db存照片路徑
            string pmasn = db.MaintainAuditInfo.Where(x => x.IPMSN == ipmsn).Select(x => x.PMASN).FirstOrDefault();
            db.MaintainAuditImage.RemoveRange(db.MaintainAuditImage.Where(x => x.PMASN == pmasn));
            db.SaveChanges();
            foreach (string path in ImgsPath)
            {
                MaintainAuditImage MainAuImg = new MaintainAuditImage() 
                {
                    PMASN = pmasn,
                    ImgPath = path
                };
                db.MaintainAuditImage.Add(MainAuImg);
                db.SaveChanges();
            }
            return "提交成功!";
        }

        public class SuppleData
        { 
            public string MaintainState { get; set; }
            public string MyName { get; set; }
            public string MaintainDate { get; set; }
            public string MaintainContent { get; set; }
            public List<string> ImgPath { get; set; }
        }

        /// <summary>
        /// 取得補件資料
        /// </summary>
        /// <param name="IPMSN"></param>
        /// <returns></returns>
        public string Supplement_GetData(string IPMSN)
        {
            var IPM = db.InspectionPlanMaintain.Find(IPMSN);
            var Mname = db.AspNetUsers.Where(x => x.UserName == IPM.MaintainUserID).Select(x => x.MyName).FirstOrDefault();
            var dic = Surfaces.Surface.InspectionPlanMaintainState();
            var MCI = db.MaintainCompletionImage.Where(x => x.IPMSN == IPMSN);
            List<string> PathList = new List<string>();
            foreach (var item in MCI)
            {
                PathList.Add(item.ImgPath);
            }
            SuppleData sd = new SuppleData()
            { 
                MaintainState = dic[IPM.MaintainState],
                MyName = Mname,
                MaintainDate = IPM.MaintainDate.ToString("yyyy/MM/dd HH:mm:ss"),
                MaintainContent = IPM.MaintainContent,
                ImgPath = PathList
            };
            string result = JsonConvert.SerializeObject(sd);
            return result;
        }

        /// <summary>
        /// formdata提交格式
        /// file1  type = file  僅一筆
        /// img1   type = file  可多筆
        /// IPMSN  type = text
        /// MaintainContent  type = text
        /// SupplementaryUserID  type = text
        /// SupplementaryContent  type = text
        /// </summary>
        /// <param name="formCollection"></param>
        /// <param name="Sev"></param>
        /// <param name="imgList"></param>
        /// <param name="fileList"></param>
        /// <returns></returns>
        public string Supplement_Submit(System.Web.Mvc.FormCollection formCollection, HttpServerUtilityBase Sev, List<HttpPostedFileBase> imgList, List<HttpPostedFileBase> fileList)
        {
            try
            {
                RepairRecord_Management_ReadViewModel RRMVM = new RepairRecord_Management_ReadViewModel();
                string ipmsn = formCollection["IPMSN"].ToString();
                var IPM = db.InspectionPlanMaintain.Find(ipmsn);
                IPM.MaintainContent = formCollection["MaintainContent"].ToString();
                IPM.MaintainState = "3";
                db.InspectionPlanMaintain.AddOrUpdate(IPM);
                var EMFI = db.EquipmentMaintainFormItem.Find(IPM.EMFISN);
                EMFI.FormItemState = "4";
                db.EquipmentMaintainFormItem.AddOrUpdate(EMFI);
                db.MaintainCompletionImage.RemoveRange(db.MaintainCompletionImage.Where(x => x.IPMSN == ipmsn)); //移除該IPMSN下所有照片，會在下方重新加入照片
                db.SaveChanges();

                //上傳照片
                List<string> IPList = new List<string>();
                foreach (var item in imgList)
                {
                    string result = RRMVM.UploadImg(item, Sev);
                    if (result != "")
                    {
                        IPList.Add(result);
                    }
                    else
                    {
                        return "上傳圖片過程錯誤!";
                    }
                }
                //儲存照片
                foreach (string path in IPList)
                {
                    MaintainCompletionImage MCI = new MaintainCompletionImage()
                    {
                        IPMSN = ipmsn,
                        ImgPath = path
                    };
                    db.MaintainCompletionImage.Add(MCI);
                    db.SaveChanges();
                }

                var MainSupIn = db.MaintainSupplementaryInfo.Where(x => x.PMSN.Contains(ipmsn)).OrderByDescending(x => x.PMSN).Select(x => x.PMSN).FirstOrDefault();
                //PMSN = IPMSN + 序號
                string newPMSN = "";
                if (MainSupIn.Count() == 0)
                {
                    newPMSN = ipmsn + "_01";
                }
                else
                {
                    int subIndex = ipmsn.Length;
                    int nowIndex = Int32.Parse(MainSupIn.Substring(subIndex + 1));
                    nowIndex++;
                    string newIndex = nowIndex.ToString();
                    if (newIndex.Length == 1)
                    {
                        newIndex = "0" + newIndex;
                    }
                    newPMSN = ipmsn + "_" + newIndex;
                }
                Models.MaintainSupplementaryInfo MSI = new Models.MaintainSupplementaryInfo()
                {
                    SupplementaryUserID = formCollection["SupplementaryUserID"].ToString(),
                    SupplementaryContent = formCollection["SupplementaryContent"].ToString(),
                    PMSN = newPMSN,
                    IPMSN = ipmsn,
                    SupplementaryDate = DateTime.Now
                };
                db.MaintainSupplementaryInfo.Add(MSI);
                db.SaveChanges();

                //補件檔案上傳
                List<string> filesPath = new List<string>();
                foreach (var item in fileList)
                {
                    string result = RRMVM.UploadFile(item, Sev);
                    if (result != "")
                    {
                        filesPath.Add(result);
                    }
                    else 
                    {
                        return "檔案上傳過程出錯!";
                    }
                }
                foreach (var item in filesPath)
                {
                    Models.MaintainSupplementaryFile MS = new Models.MaintainSupplementaryFile()
                    {
                        PMSN = newPMSN,
                        FilePath = item
                    };
                    db.MaintainSupplementaryFile.Add(MS);
                    db.SaveChanges();
                }

                return "提交成功!";
            }
            catch(Exception ex) 
            {
                return ex.Message;
            }
        }
    }
}