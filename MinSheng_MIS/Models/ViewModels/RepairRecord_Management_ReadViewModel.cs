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
using System.Data.Entity.Validation;
using MinSheng_MIS.Services;
using System.Web.WebPages;
using System.Diagnostics;

namespace MinSheng_MIS.Models.ViewModels
{
    public class RepairRecord_Management_ReadViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 巡檢維修紀錄-詳情格式
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
        #endregion

        #region 巡檢維修紀錄-詳情
        public string GetJsonForRead(string IPRSN)
        {
            try
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
                    equipmentReportItem.Date = item.Date.ToString("yyyy/MM/dd HH:mm:ss");

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
                inspectionPlan.PlanDate = InspecPlan.PlanDate.ToString("yyyy/MM/dd");
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
                inspectionPlanRepair.RepairDate = InsepecPlanRe.RepairDate?.ToString("yyyy/MM/dd");

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
                    RSInfo.SupplementaryDate = Data.SupplementaryDate.ToString("yyyy/MM/dd");
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
                    RAInfo.AuditDate = RAI.AuditDate.ToString("yyyy/MM/dd");
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
                    inspection.PlanDate = InsPlan.PlanDate.ToString("yyyy/MM/dd");

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



                    var dicS = Surfaces.Surface.InspectionPlanRepairState();
                    InspectionPlanRepair planRepair = new InspectionPlanRepair(); //維修資訊
                    planRepair.RepairState = dicS[item.RepairState];
                    planRepair.RepairContent = item.RepairContent;

                    var Name2 = db.AspNetUsers.Where(x => x.UserName == item.RepairUserID).Select(x => x.MyName).FirstOrDefault();
                    planRepair.MyName = Name2;
                    planRepair.RepairDate = item.RepairDate?.ToString("yyyy/MM/dd HH:mm:ss");

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
                        RSI.SupplementaryDate = item2.SupplementaryDate.ToString("yyyy/MM/dd");
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
                        var dicAR = Surfaces.Surface.AuditResult();
                        var AName2 = db.AspNetUsers.Where(x => x.UserName == item3.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                        RAI.MyName = AName2;
                        RAI.AuditDate = item3.AuditDate.ToString("yyyy/MM/dd");
                        RAI.AuditResult = dicAR[item3.AuditResult];
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
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
            
        }
        #endregion

        #region 提取暫存資料格式
        public class RepairAuditBufferData
        {
            public string AuditUserName { get; set; }
            public string AuditUserID { get; set; }
            public string AuditMemo { get; set; }
            public List<string> ImgPath { get; set; }
            public string AuditResult { get; set; }
            public string PRASN { get; set; }
            public string IPRSN { get; set; }
            public string AuditDate { get; set; }

        }
        #endregion

        #region 提取審核暫存資料
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
                var Name = db.AspNetUsers.Where(x => x.UserName == RepairAuIn.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                var Pic = db.RepairAuditImage.Where(x => x.PRASN == RepairAuIn.PRASN).Select(x => x.ImgPath); //可能會有很多張圖
                List<string> ImgList = new List<string>();
                foreach (string path in Pic)
                {
                    ImgList.Add(path);
                }

                var dic_AuditResult = Surface.AuditResult();
                RepairAuditBufferData ReAu = new RepairAuditBufferData()
                {
                    AuditUserName = Name,
                    AuditUserID = RepairAuIn.AuditUserID,
                    AuditMemo = RepairAuIn.AuditMemo,
                    ImgPath = ImgList,
                    AuditResult = dic_AuditResult[RepairAuIn.AuditResult],
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
                return JsonConvert.SerializeObject("");
            }
        }
        #endregion

        #region 提交審核資料後設備狀態調整
        /// <summary>
        /// 在提交審核資料時:
        /// 檢查設備EquipmentReportForm中有沒有其他未完成的項目，如果沒有的話則去EquipmentInfo把該設備(ESN)的Estate改為1
        /// </summary>
        /// <returns></returns>
        public bool CheckEquipState(string ESN)
        {
            try
            {
                var ERF = db.EquipmentReportForm.Where(x => x.ESN == ESN).Where(x => x.ReportState != "7");
                if (ERF == null)
                {
                    var EI = db.EquipmentInfo.Find(ESN);
                    EI.EState = "1";
                    db.EquipmentInfo.AddOrUpdate(EI);
                }
                return true;
            }
            catch { return false; }
        }
        #endregion

        #region 巡檢維修紀錄_審核_提交
        public string CreateAuditData(System.Web.Mvc.FormCollection formCollection, HttpServerUtilityBase Sev, List<HttpPostedFileBase> fileList)
        {
            JsonResponseViewModel Jresult = new JsonResponseViewModel();

            try
            {
                #region 變數宣告
                string iprsn = formCollection["IPRSN"].ToString();
                string prasn = "";
                bool IsBu = formCollection["IsBuffer"] == "0" ? false : true;
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
                #endregion

                #region 主表更新
                var RAI_SourceTable = db.RepairAuditInfo.Where(x => x.IPRSN == iprsn).Where(x => x.IsBuffer == true).FirstOrDefault(); //檢查有沒有暫存資料

                if (RAI_SourceTable != null) //有暫存
                {
                    RAI_SourceTable.AuditUserID = formCollection["AuditUserID"].ToString();
                    RAI_SourceTable.AuditDate = DateTime.Now;
                    RAI_SourceTable.AuditMemo = formCollection["AuditMemo"].IsNullOrWhiteSpace() ? "" : formCollection["AuditMemo"].ToString();
                    RAI_SourceTable.AuditResult = formCollection["AuditResult"].ToString();
                    RAI_SourceTable.IsBuffer = IsBu;

                    prasn = RAI_SourceTable.PRASN;
                    db.RepairAuditInfo.AddOrUpdate(RAI_SourceTable);
                    db.SaveChanges();
                }
                else
                {
                    #region 組新PRASN
                    var GetPRASN = db.RepairAuditInfo.Where(x => x.IPRSN == iprsn).OrderByDescending(x => x.PRASN).Select(x => x.PRASN).FirstOrDefault();
                    //規則 :   PRASN = IPRSN + _序號
                    //取出最新的一筆資料，算出下一筆要新增的序號
                    string NewPRASN = ""; //新增的序號
                    if (GetPRASN == null) //如果沒有任何資料
                    {
                        NewPRASN = iprsn + "_01";
                    }
                    else //有資料
                    {
                        int subIndex = iprsn.Length; //抓取IPRSN長度
                        int Nowindex = Int32.Parse(GetPRASN.Substring(subIndex + 1)); //擷取現在最新資料後面數字部分
                        Nowindex++;
                        string Newindex = Nowindex.ToString();
                        if (Newindex.Length == 1) //如果是個位數前面要補0
                        {
                            Newindex = "0" + Newindex;
                        }
                        NewPRASN = iprsn + "_" + Newindex; //組起來新的PRSN
                    }
                    prasn = NewPRASN;
                    #endregion

                    var RAI = new Models.RepairAuditInfo()
                    {
                        PRASN = prasn,
                        IPRSN = iprsn,
                        AuditUserID = formCollection["AuditUserID"].ToString(),
                        AuditDate = DateTime.Now,
                        AuditMemo = formCollection["AuditMemo"].IsNullOrWhiteSpace() ? "" : formCollection["AuditMemo"].ToString(),
                        AuditResult = formCollection["AuditResult"].ToString(),
                        IsBuffer = IsBu
                    };
                    db.RepairAuditInfo.AddOrUpdate(RAI);
                    db.SaveChanges();
                }
                
                #endregion

                if (!IsBu) //暫存不要改狀態
                {
                    #region 狀態調整
                    var InsPlanRe = db.InspectionPlanRepair.Find(iprsn);
                    InsPlanRe.RepairState = RepairState_IPR;
                    db.InspectionPlanRepair.AddOrUpdate(InsPlanRe);
                    db.SaveChanges();

                    var EquipReport = db.EquipmentReportForm.Find(InsPlanRe.RSN);
                    EquipReport.ReportState = RepairState_ERF;
                    db.EquipmentReportForm.AddOrUpdate(EquipReport);
                    db.SaveChanges();
                    #endregion

                    #region 設備狀態檢查
                    if (formCollection["AuditResult"].ToString() == "1") //審核狀態為審核通過時，要檢查該設備，並做設備狀態調整
                    {
                        var rsn = db.InspectionPlanRepair.Find(iprsn);
                        if (!CheckEquipState(rsn.RSN))
                        {
                            Jresult.ResponseCode = 500;
                            Jresult.ResponseMessage = "檢查設備狀態時出錯!";
                            return JsonConvert.SerializeObject(Jresult);
                        }
                    }
                    #endregion
                }

                #region 圖片處理
                List<string> ImagePath_Server = new List<string>();
                if (!ComFunc.UpdateFile(fileList, Sev, ref ImagePath_Server, prasn))
                {
                    Jresult.ResponseCode = 500;
                    Jresult.ResponseMessage = "上傳過程出錯!";
                    return JsonConvert.SerializeObject(Jresult);
                }

                var DelImage_Source = db.RepairAuditImage.Where(x => x.PRASN == prasn);
                if (DelImage_Source != null)
                {
                    db.RepairAuditImage.RemoveRange(DelImage_Source);
                    db.SaveChanges();
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
                #endregion

                Jresult.ResponseCode = 200;
                Jresult.ResponseMessage = "提交成功!";
                return JsonConvert.SerializeObject(Jresult);
            }
            catch (Exception ex)
            {
                Jresult.ResponseCode = 500;
                Jresult.ResponseMessage = ex.Message;
                return JsonConvert.SerializeObject(Jresult);
            }
        }
        #endregion

        #region 巡檢維修紀錄_補件_取得補件資料_格式
        public class SuppleData
        {
            public string RepairState { get; set; }
            public string MyName { get; set; }
            public string RepairDate { get; set; }
            public string RepairContent { get; set; }
            public List<string> ImgPath { get; set; }
            //public string PRSN { get; set; }
            public string IPRSN { get; set; }
            //public string SupplementaryDate { get; set; }
        }
        #endregion

        #region 巡檢維修紀錄_補件_取得補件資料
        public string GetSupplementEditData(string IPRSN) //取得"補件"下方編輯區資料，提供給前端做顯示
        {
            try
            {
                var IPR = db.InspectionPlanRepair.Find(IPRSN);
                var Name = db.AspNetUsers.Where(x => x.UserName == IPR.RepairUserID).Select(x => x.MyName).FirstOrDefault();
                var RSI = db.RepairSupplementaryInfo.Where(x => x.IPRSN == IPRSN).FirstOrDefault();
                var RCI = db.RepairCompletionImage.Where(x => x.IPRSN == IPRSN).Select(x => x.ImgPath);

                List<string> allPath = new List<string>();
                foreach (var path in RCI)
                {
                    allPath.Add(path);
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
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
            
        }
        #endregion

        #region 巡檢維修紀錄_補件_更新補件資料
        /// <summary>
        /// IPRSN type = text
        /// RepairContent type = text
        /// SupplementaryUserID type = text
        /// SupplementaryContent type = text
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Sev"></param>
        /// <param name="imgList"></param>
        /// <param name="fileList"></param>
        /// <returns></returns>
        public string UpdateSuppleData(System.Web.Mvc.FormCollection form, HttpServerUtilityBase Sev, List<HttpPostedFileBase> imgList, List<HttpPostedFileBase> fileList) //提交補件資料，要更新和新建資料Repair Supplementary Info
        {
            JsonResponseViewModel Jresult = new JsonResponseViewModel();    
            try
            {
                #region 巡檢計畫含維修 Inspection Plan Repair 資料更新
                string iprsn = form["IPRSN"].ToString();
                var IPR = db.InspectionPlanRepair.Find(iprsn);
                IPR.RepairState = "3";
                IPR.RepairContent = form["RepairContent"].IsNullOrWhiteSpace() ? "" : form["RepairContent"].ToString();
                db.InspectionPlanRepair.AddOrUpdate(IPR);
                var RCI = db.RepairCompletionImage.Where(x => x.IPRSN == iprsn);
                if (RCI != null)
                {
                    db.RepairCompletionImage.RemoveRange(RCI); //先移除所有維修照片再新增
                }
                db.SaveChanges();
                #endregion

                #region 維修圖片
                List<string> ImagePath_Server = new List<string>();
                if (!ComFunc.UpdateFile(imgList, Sev, ref ImagePath_Server, iprsn))
                {
                    Jresult.ResponseCode = 500;
                    Jresult.ResponseMessage = "圖片上傳過程出錯!";
                    return JsonConvert.SerializeObject(Jresult);
                }

                foreach (string item in ImagePath_Server)
                {
                    //db存
                    RepairCompletionImage RC = new RepairCompletionImage()
                    {
                        IPRSN = iprsn,
                        ImgPath = item
                    };
                    db.RepairCompletionImage.Add(RC);
                    db.SaveChanges();
                }
                #endregion

                #region 變更 設備報修單Equipment Report 狀態
                var EP = db.EquipmentReportForm.Find(IPR.RSN);
                EP.ReportState = "4";
                db.EquipmentReportForm.AddOrUpdate(EP);
                db.SaveChanges();
                #endregion

                #region 組新的PRSN
                var GetPRSN = db.RepairSupplementaryInfo.Where(x => x.PRSN.Contains(iprsn)).OrderByDescending(x => x.PRSN).Select(x => x.PRSN).FirstOrDefault();
                //規則 :   PRSN = IPRSN + _序號
                //取出最新的一筆資料，算出下一筆要新增的序號
                string NewPRSN = ""; //新增的序號
                if (GetPRSN == null) //如果沒有任何資料
                {
                    NewPRSN = iprsn + "_01";
                }
                else //有資料
                {
                    int subIndex = iprsn.Length; //抓取IPRSN長度
                    int Nowindex = Int32.Parse(GetPRSN.Substring(subIndex + 1)); //擷取現在最新資料後面數字部分
                    Nowindex++;
                    string Newindex = Nowindex.ToString();
                    if (Newindex.Length == 1) //如果是個位數前面要補0
                    {
                        Newindex = "0" + Newindex;
                    }
                    NewPRSN = iprsn + "_" + Newindex; //組起來新的PRSN
                }
                #endregion

                #region 新增補件資料 RepairSupplementaryInfo
                var RSI = new Models.RepairSupplementaryInfo()
                {
                    PRSN = NewPRSN,
                    IPRSN = iprsn,
                    SupplementaryUserID = form["SupplementaryUserID"].ToString(),
                    SupplementaryDate = DateTime.Now,
                    SupplementaryContent = form["SupplementaryContent"].ToString()
                };
                db.RepairSupplementaryInfo.Add(RSI);
                db.SaveChanges();
                #endregion

                #region 補件檔案
                List<string> FilesPath = new List<string>();
                if (!ComFunc.UpdateFile(fileList, Sev, ref FilesPath, NewPRSN))
                {
                    Jresult.ResponseCode = 500;
                    Jresult.ResponseMessage = "檔案上傳過程出錯!";
                    return JsonConvert.SerializeObject(Jresult);
                }

                foreach (var item in FilesPath)
                {
                    RepairSupplementaryFile RS = new RepairSupplementaryFile()
                    {
                        FilePath = item,
                        PRSN = NewPRSN
                    };
                    db.RepairSupplementaryFile.Add(RS);
                    db.SaveChanges();
                }
                #endregion

                Jresult.ResponseCode = 200;
                Jresult.ResponseMessage = "提交成功!";
                return JsonConvert.SerializeObject(Jresult);
            }
            catch (Exception ex)
            {
                Jresult.ResponseCode = 500;
                Jresult.ResponseMessage = ex.Message;
                return JsonConvert.SerializeObject(Jresult);
            }
        }
#endregion
    }
}