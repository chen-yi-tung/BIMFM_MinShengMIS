using Microsoft.Ajax.Utilities;
using Microsoft.Owin;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Web.Mvc;
using System.Data.Entity.Migrations;
using System.IO;

namespace MinSheng_MIS.Models.ViewModels
{
    public class RepairRecord_Management_ReadViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        private class EquipmentReportItem
        {
            public string ReportState { get; set; }
            public string RSN { get; set; }
            public string Date { get; set; }
            public string ReportLevel { get; set; }
            public string MyName { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string PorpertyCode { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string ESN_Button { get; set; }
            public string ReportContent { get; set; }
            public List<string> ImgPath { get; set; }
        }

        private class InspectionPlan
        {
            public string IPSN { get; set; }
            public string IPName { get; set; }
            public string PlanDate { get; set; }
            public string PlanState { get; set; }
            public string Shift { get; set; }
            public string MyName { get; set; }
        }

        private class InspectionPlanList
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRepair InspectionPlanRepair { get; set; }
            public List<RepairSupplementaryInfo> RepairSupplementaryInfo { get; set; }
            public List<RepairAuditInfo> RepairAuditInfo { get; set; }
        }

        private class InspectionPlanRepair
        {
            public string RepairState { get; set; }
            public string MyName { get; set; }
            public string RepairContent { get; set; }
            public string RepairDate { get; set; }
            public List<string> ImgPath { get; set; }
        }

        private class RepairAuditInfo
        {
            public string MyName { get; set; }
            public string AuditDate { get; set; }
            public string AuditResult { get; set; }
            public string AuditMemo { get; set; }
            public List<string> ImgPath { get; set; }
        }

        private class RepairSupplementaryInfo
        {
            public string MyName { get; set; }
            public string SupplementaryDate { get; set; }
            public string SupplementaryContent { get; set; }
            public List<string> FilePath { get; set; }
        }

        private class Root
        {
            public EquipmentReportItem EquipmentReportItem { get; set; }
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRepair InspectionPlanRepair { get; set; }
            public List<RepairSupplementaryInfo> RepairSupplementaryInfo { get; set; }
            public List<RepairAuditInfo> RepairAuditInfo { get; set; }
            public List<InspectionPlanList> InspectionPlanList { get; set; }
        }

        public string GetJsonForRead(string IPRSN) //巡檢維修紀錄-詳情
        {

            Root root = new Root();

            #region 處理報修資料 EquipmentReportItem

            EquipmentReportItem equipmentReportItem = new EquipmentReportItem();

            var InsepecPlanRe = db.InspectionPlanRepair.Where(x => x.IPRSN == IPRSN).FirstOrDefault();
            var table1 = db.EquipmentReportForm.Where(x => x.RSN == InsepecPlanRe.RSN).ToList();
            foreach (var item in table1)
            {
                var ReportStatedics = Surface.EquipmentReportFormState();
                equipmentReportItem.ReportState = ReportStatedics[item.ReportState.Trim()];
                equipmentReportItem.RSN = item.RSN.Trim();
                equipmentReportItem.Date = item.Date?.ToString("yyyy/M/d HH:mm:ss");

                var ReportLeveldics = Surface.ReportLevel();
                equipmentReportItem.ReportLevel = ReportLeveldics[item.ReportLevel.Trim()];

                var myname = db.AspNetUsers.Where(x => x.UserName == item.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                equipmentReportItem.MyName = myname;

                var EquipInfoRow = db.EquipmentInfo.Where(x => x.ESN == item.ESN).FirstOrDefault();
                equipmentReportItem.Area = EquipInfoRow.Area;
                equipmentReportItem.Floor = EquipInfoRow.Floor;
                equipmentReportItem.PorpertyCode = EquipInfoRow.PropertyCode;

                equipmentReportItem.ESN = item.ESN;
                equipmentReportItem.EName = EquipInfoRow.EName;
                equipmentReportItem.ReportContent = item.ReportContent;
                var ImagePathList = db.ReportImage.Where(x => x.RSN == item.RSN).Select(x => x.ImgPath).ToList();
                equipmentReportItem.ImgPath = ImagePathList;
                break;
            }
            #endregion

            #region 計劃資訊InspectionPlan

            InspectionPlan inspectionPlan = new InspectionPlan();

            var InspecPlan = db.InspectionPlan.Where(x => x.IPSN == InsepecPlanRe.IPSN).FirstOrDefault();

            inspectionPlan.IPSN = InspecPlan.IPSN;
            inspectionPlan.IPName = InspecPlan.IPName;
            inspectionPlan.PlanDate = InspecPlan.PlanDate.ToString("yyyy/M/d");
            var PSdics = Surface.InspectionPlanState();
            inspectionPlan.PlanState = PSdics[InspecPlan.PlanState];
            var Shiftdics = Surface.Shift();
            inspectionPlan.Shift = Shiftdics[InspecPlan.Shift];
            var IPUserudlist = db.InspectionPlanMember.Where(x => x.IPSN == InspecPlan.IPSN).Select(x => x.UserID).ToList();
            var INSPNameList = "";
            int a = 0;
            foreach (var id in IPUserudlist)
            {
                var myname = db.AspNetUsers.Where(x => x.UserName == id).Select(x => x.MyName).FirstOrDefault();
                if (myname != null)
                {
                    if (a == 0)
                        INSPNameList += myname;
                    else
                        INSPNameList += $"、{myname}";
                }
                a++;
            }
            a = 0;
            inspectionPlan.MyName = INSPNameList;
            #endregion

            #region 維修資料

            InspectionPlanRepair inspectionPlanRepair = new InspectionPlanRepair();

            var IPRdics = Surface.InspectionPlanRepairState();
            inspectionPlanRepair.RepairState = IPRdics[InsepecPlanRe.RepairState];
            inspectionPlanRepair.RepairContent = InsepecPlanRe.RepairContent;

            var IPRname = db.AspNetUsers.Where(x => x.UserName == InsepecPlanRe.RepairUserID).Select(x => x.MyName).FirstOrDefault() as string;
            inspectionPlanRepair.MyName = IPRname;
            inspectionPlanRepair.RepairDate = InsepecPlanRe.RepairDate?.ToString("yyyy/M/d");

            var RPImgPathlist = db.RepairCompletionImage.Where(x => x.IPRSN == InsepecPlanRe.IPRSN).Select(x => x.ImgPath).ToList();
            inspectionPlanRepair.ImgPath = RPImgPathlist;
            #endregion

            #region RepairSupplementaryInfo補件資料

            var repairSupplementaryInfolist = new List<RepairSupplementaryInfo>(); //補件資料List

            var RSInfoData = db.RepairSupplementaryInfo.Where(x => x.IPRSN == IPRSN).ToList();
            foreach (var Data in RSInfoData)
            {
                RepairSupplementaryInfo RSInfo = new RepairSupplementaryInfo(); //補件資料

                var Name = db.AspNetUsers.Where(x => x.UserName == Data.SupplementaryUserID).FirstOrDefault();
                RSInfo.MyName = Name.MyName;
                RSInfo.SupplementaryDate = Data.SupplementaryDate.ToString("yyyy/M/d");
                RSInfo.SupplementaryContent = Data.SupplementaryContent;
                var FileP = db.RepairSupplementaryFile.Where(x => x.PRSN == Data.PRSN).Select(x => x.FilePath).ToList();
                RSInfo.FilePath = FileP;
                repairSupplementaryInfolist.Add(RSInfo);
            }
            #endregion

            #region RepairAuditInfo審核資料

            var RepairAuditInfolist = new List<RepairAuditInfo>(); //審核資料List

            var RepairAuIn = db.RepairAuditInfo.Where(x => x.IPRSN == IPRSN).Where(x => x.IsBuffer == false).ToList();
            foreach (var RAI in RepairAuIn)
            {
                RepairAuditInfo RAInfo = new RepairAuditInfo(); //審核資料

                var AName = db.AspNetUsers.Where(x => x.UserName == RAI.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                RAInfo.MyName = AName;
                RAInfo.AuditDate = RAI.AuditDate.ToString("yyyy/M/d");
                var dic = Surface.AuditResult();
                RAInfo.AuditResult = dic[RAI.AuditResult];
                RAInfo.AuditMemo = RAI.AuditMemo;
                var ImgP = db.RepairAuditImage.Where(x => x.PRASN == RAI.PRASN).Select(x => x.ImgPath).ToList();
                RAInfo.ImgPath = ImgP;
                RepairAuditInfolist.Add(RAInfo);
            }
            #endregion

            #region InspectionPlanList報修單相關維修紀錄
            var InspectionPlanList = new List<InspectionPlanList>();

            var restRSNinInspPlanRe = db.InspectionPlanRepair.Where(x => x.RSN.Contains(InsepecPlanRe.RSN)).Where(x => x.IPRSN != IPRSN).ToList();
            foreach (var item in restRSNinInspPlanRe)
            {
                InspectionPlanList inspectionPlans = new InspectionPlanList();

                InspectionPlan inspection = new InspectionPlan(); //計劃資訊
                var InsPlan = db.InspectionPlan.Where(x => x.IPSN == item.IPSN).FirstOrDefault();
                inspection.IPSN = InsPlan.IPSN;
                inspection.IPName = InsPlan.IPName;
                inspection.PlanDate = InsPlan.PlanDate.ToString("yyyy/M/d");

                inspection.PlanState = PSdics[InsPlan.PlanState];
                inspection.Shift = Shiftdics[InsPlan.Shift];
                var UID = db.InspectionPlanMember.Where(x => x.IPSN == InsPlan.IPSN).Select(x => x.UserID).ToList();
                var INSPNlist = "";
                int aa = 0;
                foreach (var id in UID)
                {
                    var myname1 = db.AspNetUsers.Where(x => x.UserName == id).Select(x => x.MyName).FirstOrDefault();
                    if (myname1 != null)
                    {
                        if (aa == 0)
                            INSPNlist += myname1;
                        else
                            INSPNlist += $"、{myname1}";
                    }
                    aa++;
                }
                aa = 0;
                inspection.MyName = INSPNlist;
                inspectionPlans.InspectionPlan = inspection;




                InspectionPlanRepair planRepair = new InspectionPlanRepair(); //維修資訊
                planRepair.RepairState = item.RepairState;
                planRepair.RepairContent = item.RepairContent;

                var Name2 = db.AspNetUsers.Where(x => x.UserName == item.RepairUserID).Select(x => x.MyName).FirstOrDefault();
                planRepair.MyName = Name2;
                planRepair.RepairDate = item.RepairDate?.ToString("yyyy/M/d HH:mm:ss");

                var RPImg2 = db.RepairCompletionImage.Where(x => x.IPRSN == item.IPRSN).Select(x => x.ImgPath).ToList();
                planRepair.ImgPath = RPImg2;
                inspectionPlans.InspectionPlanRepair = planRepair;





                List<RepairSupplementaryInfo> ListRSI = new List<RepairSupplementaryInfo>(); //補件資料
                var RSInfoData2 = db.RepairSupplementaryInfo.Where(x => x.IPRSN == IPRSN).ToList();
                foreach (var item2 in RSInfoData2)
                {
                    RepairSupplementaryInfo RSI = new RepairSupplementaryInfo();

                    var Name3 = db.AspNetUsers.Where(x => x.UserName == item2.SupplementaryUserID).FirstOrDefault();
                    RSI.MyName = Name3.MyName;
                    RSI.SupplementaryDate = item2.SupplementaryDate.ToString("yyyy/M/d");
                    RSI.SupplementaryContent = item2.SupplementaryContent;
                    var FileP2 = db.RepairSupplementaryFile.Where(x => x.PRSN == item2.PRSN).Select(x => x.FilePath).ToList();
                    RSI.FilePath = FileP2;
                    ListRSI.Add(RSI);
                }
                inspectionPlans.RepairSupplementaryInfo = ListRSI;




                List<RepairAuditInfo> ListRAI = new List<RepairAuditInfo>(); //審核資料

                var RepairAuIn2 = db.RepairAuditInfo.Where(x => x.IPRSN == item.IPRSN).Where(x => x.IsBuffer == false).ToList();
                foreach (var item3 in RepairAuIn2)
                {
                    RepairAuditInfo RAI = new RepairAuditInfo(); //審核資料

                    var AName2 = db.AspNetUsers.Where(x => x.UserName == item3.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                    RAI.MyName = AName2;
                    RAI.AuditDate = item3.AuditDate.ToString("yyyy/M/d");
                    RAI.AuditResult = item3.AuditResult;
                    RAI.AuditMemo = item3.AuditMemo;
                    var ImgP2 = db.RepairAuditImage.Where(x => x.PRASN == item3.PRASN).Select(x => x.ImgPath).ToList();
                    RAI.ImgPath = ImgP2;
                    ListRAI.Add(RAI);
                }
                inspectionPlans.RepairAuditInfo = ListRAI;


                InspectionPlanList.Add(inspectionPlans);
            }
            #endregion

            root.EquipmentReportItem = equipmentReportItem;
            root.InspectionPlan = inspectionPlan;
            root.InspectionPlanRepair = inspectionPlanRepair;
            root.RepairSupplementaryInfo = repairSupplementaryInfolist;
            root.RepairAuditInfo = RepairAuditInfolist;
            root.InspectionPlanList = InspectionPlanList;

            string result = JsonConvert.SerializeObject(root);
            return result;
        }

        public class AllRepairAudit
        {
            public string AuditUserName { get; set; }
            public string AuditUserID { get; set; }
            public string AuditMemo { get; set; }
            public string ImgPath { get; set; }
            public string AuditResult { get; set; }
            public string PRASN { get; set; }
            public string IPRSN { get; set; }
            public string AuditDate { get; set; }
            
        }

        /// <summary>
        /// 檢查有沒有草稿(IsBuffer = 1)，有的話就帶回資料，沒有的話就不帶
        /// </summary>
        /// <returns></returns>
        public string AuditCheckBuffer(string IPRSN)
        { 
            var RepairAuIn = db.RepairAuditInfo.Where(x => x.IPRSN == IPRSN).Where(x => x.IsBuffer == true).FirstOrDefault();
            if (RepairAuIn != null)
            {
                //有草稿要回傳
                //要在確認回傳格式
                var Name = db.AspNetUsers.Where(x => x.UserName == RepairAuIn.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                var Pic = db.RepairAuditImage.Where(x => x.PRASN == RepairAuIn.PRASN).Select(x => x.ImgPath); //可能會有很多張圖
                string PicResult = "";  
                foreach (var item in Pic)
                {
                    PicResult += item + ",";
                }
                if (PicResult.Length > 0)
                {
                    PicResult.Remove(PicResult.Length - 1); //移除最後一個'，'
                }

                var dic = Surface.AuditResult();
                AllRepairAudit ReAu = new AllRepairAudit()
                {
                    AuditUserName = Name,
                    AuditUserID = RepairAuIn.AuditUserID,
                    AuditMemo = RepairAuIn.AuditMemo,
                    ImgPath = PicResult,  //如果多張圖片的話， 用'，'分開
                    AuditResult = dic[RepairAuIn.AuditResult],
                    PRASN = RepairAuIn.PRASN,
                    IPRSN = IPRSN,
                    AuditDate = RepairAuIn.AuditDate.ToString("yyyy/MM/dd HH:mm:ss")
                };
                string result = JsonConvert.SerializeObject(ReAu);
                return result;
            }
            else
            {
                //沒有草稿
                return "";
            }
        }
        /// <summary>
        /// 上傳照片，並回傳Server端儲存路徑
        /// </summary>
        /// <param name="Path"> client端的圖片路徑 </param>
        /// <returns></returns>
        public string UploadImg(HttpPostedFileBase file, HttpServerUtilityBase Sev) 
        {
            try
            {
                string fileSavedPath = "~/AllImage/";
                if (!Directory.Exists(Sev.MapPath(fileSavedPath)))
                {
                    Directory.CreateDirectory(Sev.MapPath(fileSavedPath));
                }
                
                string newFileName = string.Concat(
                DateTime.Now.ToString("yyyy-MM-ddHH-mm-ss-ff"),
                Path.GetExtension(file.FileName).ToLower()); //這段可以把檔案名稱改為當下時間

                string fullFilePath = Path.Combine(Sev.MapPath(fileSavedPath), newFileName);
                file.SaveAs(fullFilePath);
                return fileSavedPath + newFileName;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 上傳檔案，並回傳Server端儲存路徑
        /// </summary>
        /// <param name="Path"> client端的檔案路徑 </param>
        /// <returns></returns>
        public string UploadFile(HttpPostedFileBase file, HttpServerUtilityBase Sev, string prasn)
        {
            try
            {
                string fileSavedPath = "~/AllFile/";
                if (!Directory.Exists(Sev.MapPath(fileSavedPath)))
                {
                    Directory.CreateDirectory(Sev.MapPath(fileSavedPath));
                }

                string newFileName = string.Concat(
                DateTime.Now.ToString("yyyy-MM-ddHH-mm-ss-ff"),
                Path.GetExtension(file.FileName).ToLower()); //這段可以把檔案名稱改為當下時間

                string fullFilePath = Path.Combine(Sev.MapPath(fileSavedPath), newFileName);
                file.SaveAs(fullFilePath);
                return fileSavedPath + newFileName;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string CreateAuditData(System.Web.Mvc.FormCollection formCollection, HttpServerUtilityBase Sev, List<HttpPostedFileBase> fileList)
        {
            string iprsn = formCollection["IPRSN"].ToString();
            string prasn = formCollection["prasn"].ToString();
            string RepairState_IPR = ""; //InspectionPlanRepair
            string RepairState_ERF = ""; //EquipReportForm

            switch (formCollection["AuditResult"].ToString()) //判斷審核結果
            {
                case "1":
                    RepairState_IPR = "6";
                    RepairState_ERF = "7";
                    break;
                case "2":
                    RepairState_IPR = "7";
                    RepairState_ERF = "8";
                    break; 
                case "3":
                    RepairState_IPR = "5";
                    RepairState_ERF = "6";
                    break;
            }

            if (formCollection["isBuffer"] == "0") //先判斷是按哪一個按鈕，新增
            {
                var RAI = new Models.RepairAuditInfo()
                {
                    PRASN = prasn, 
                    IPRSN = iprsn, 
                    AuditUserID = formCollection["AuditUserID"].ToString(),
                    AuditDate = DateTime.Now,
                    AuditMemo = formCollection["AuditMemo"].ToString(),
                    AuditResult = formCollection["AuditResult"].ToString(),
                    IsBuffer = false
                };
                db.RepairAuditInfo.AddOrUpdate(RAI);
            }
            else //暫存
            {
                var RAI = new Models.RepairAuditInfo()
                {
                    PRASN = prasn, 
                    IPRSN = iprsn, 
                    AuditUserID = formCollection["AuditUserID"].ToString(),
                    AuditDate = DateTime.Now,
                    AuditMemo = formCollection["AuditMemo"].ToString(),
                    AuditResult = formCollection["AuditResult"].ToString(),
                    IsBuffer = true
                };
                db.RepairAuditInfo.AddOrUpdate(RAI);
            }
            db.SaveChanges();

            var InsPlanRe = db.InspectionPlanRepair.Find(iprsn);
            InsPlanRe.RepairState = RepairState_IPR;
            db.InspectionPlanRepair.AddOrUpdate(InsPlanRe);
            db.SaveChanges();

            var IPRepair = db.InspectionPlanRepair.Where(x => x.IPRSN == iprsn).FirstOrDefault();
            var EquipReport = db.EquipmentReportForm.Find(IPRepair.RSN);
            EquipReport.ReportState = RepairState_ERF;
            db.EquipmentReportForm.AddOrUpdate(EquipReport);
            db.SaveChanges();

            //以下處理圖片
            List<string> ImagePath_Server = new List<string>();

            foreach (var item in fileList)
            {
                //上傳照片 都傳完之後才做新增
                string result = UploadImg(item, Sev); //做上傳動作，並回傳儲存路徑
                if (result != "")
                {
                    ImagePath_Server.Add(result);
                }
                else
                {
                    return "上傳過程出錯!"; //新增出錯!!
                }
            }

            foreach (var item in ImagePath_Server)
            {
                var Image = new Models.RepairAuditImage()
                {
                    ImgPath = item, //圖片路徑
                    PRASN = prasn
                };
                db.RepairAuditImage.Add(Image);
                db.SaveChanges();
            }

            return "成功!";
        }

        public class SuppleData
        { 
            public string RepairState { get; set; }
            public string MyName { get; set; }
            public string RepairDate { get; set; }
            public string RepairContent { get; set; }
            public string ImgPath { get; set; }
            //public string PRSN { get; set; }
            public string IPRSN { get; set; }
            //public string SupplementaryDate { get; set; }
        }

        public string GetSupplementEditData(string IPRSN) //取得"補件"下方編輯區資料，提供給前端做顯示
        {
            var IPR = db.InspectionPlanRepair.Find(IPRSN);
            var Name = db.AspNetUsers.Where(x => x.UserName == IPR.RepairUserID).Select(x => x.MyName).FirstOrDefault();
            var RSI = db.RepairSupplementaryInfo.Where(x => x.IPRSN == IPRSN).FirstOrDefault();
            var RCI = db.RepairCompletionImage.Where(x => x.IPRSN == IPRSN).Select(x => x.ImgPath);

            string allPath = "";
            foreach (var path in RCI)
            {
                allPath += path + ",";
            }
            if (allPath.Length > 0)
            {
                allPath.Remove(allPath.Length - 1); //移除最後一個'，'
            }
            var dic = Surface.InspectionPlanRepairState();
            var SD = new SuppleData() //下面補件資料的所有資料
            {
                RepairState = dic[IPR.RepairState],
                MyName = Name,
                RepairDate = IPR.RepairDate?.ToString("yyyy/MM/dd HH:mm:ss"),
                RepairContent = IPR.RepairContent,
                ImgPath = allPath,
                IPRSN = IPRSN
            };

            string result = JsonConvert.SerializeObject(SD);
            return result;
        }

        public string UpdateSuppleData(System.Web.Mvc.FormCollection form, HttpServerUtilityBase Sev, List<HttpPostedFileBase> imgList,List<HttpPostedFileBase> fileList) //提交補件資料，要更新和新建資料Repair Supplementary Info
        {
            try
            {
                var IPR = db.InspectionPlanRepair.Find(form["IPRSN"].ToString());
                IPR.RepairState = "3";
                IPR.RepairContent = form["RepairContent"].ToString();
                db.InspectionPlanRepair.AddOrUpdate(IPR);
                db.RepairCompletionImage.RemoveRange(db.RepairCompletionImage.Where(x => x.IPRSN == form["IPRSN"].ToString())); //先移除所有維修照片再新增
                db.SaveChanges();
                List<string> ImagePath_Server = new List<string>();
                foreach (var item in imgList)
                {
                    string result = UploadImg(item, Sev); //做上傳動作，並回傳儲存路徑
                    if (result != "")
                    {
                        ImagePath_Server.Add(result);
                    }
                    else
                    {
                        return "上傳過程出錯!"; //新增出錯!!
                    }
                }
                foreach (var item in ImagePath_Server)
                {
                    //db存
                }
                var EP = db.EquipmentReportForm.Find(IPR.RSN);
                EP.ReportState = "4";
                db.EquipmentReportForm.AddOrUpdate(EP);
                db.SaveChanges();

                //補件檔案


                var GetPRSN = db.RepairSupplementaryInfo.Where(x => x.PRSN.Contains(form["IPRSN"].ToString())).OrderByDescending(x => x.PRSN).Select(x => x.PRSN).FirstOrDefault();
                //規則 :   PRSN = IPRSN + _序號
                //取出最新的一筆資料，算出下一筆要新增的序號
                string NewPRSN = ""; //新增的序號
                if (GetPRSN.Count() == 0) //如果沒有任何資料
                {
                    NewPRSN = form["IPRSN"].ToString() + "_01";
                }
                else //有資料
                {
                    int subIndex = form["IPRSN"].ToString().Length; //抓取IPRSN長度
                    int Nowindex = Int32.Parse(GetPRSN.Substring(subIndex + 1)); //擷取現在最新資料後面數字部分
                    Nowindex++;
                    string Newindex = Nowindex.ToString();
                    if (Newindex.Length == 1) //如果是個位數前面要補0
                    {
                        Newindex = "0" + Newindex;
                    }
                    NewPRSN = form["IPRSN"].ToString() + Newindex; //組起來新的PRSN
                }
                var RSI = new Models.RepairSupplementaryInfo()
                {
                    PRSN = NewPRSN,
                    IPRSN = form["IPRSN"].ToString(),
                    SupplementaryUserID = form["SupplementaryUserID"].ToString(),
                    SupplementaryDate = DateTime.Now,
                    SupplementaryContent = form["SupplementaryContent"].ToString()
                };
                db.RepairSupplementaryInfo.Add(RSI);
                return "提交成功!";
            }
            catch (Exception ex) 
            {
                return ex.Message;
            }
        }
    }
}