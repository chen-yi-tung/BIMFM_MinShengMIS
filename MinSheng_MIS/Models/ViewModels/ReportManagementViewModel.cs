using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ReportManagementViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        private class InspectionPlanList
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRepair InspectionPlanRepair { get; set; }
            public List<RepairSupplementaryInfo> RepairSupplementaryInfo { get; set; }
            public List<RepairAuditInfo> RepairAuditInfo { get; set; }
        }

        private class InspectionPlan {
            public string IPSN { get; set; }
            public string IPName { get; set; }
            public string PlanDate { get; set; }
            public string PlanState { get; set; }
            public string Shift { get; set; }
            public string MyName { get; set; }
        }

        private class InspectionPlanRepair
        {
            public string RepairState { get; set; }
            public string RepairContent { get; set; }
            public string MyName { get; set; }
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
            public string RSN { get; set; }
            public string Date { get; set; }
            public string ReportLevel { get; set; }
            public string MyName { get; set; }
            public string ReportState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string PropertyCode { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string ReportContent { get; set; }
            public List<string> ImgPath { get; set; }
            public List<InspectionPlanList> InspectionPlanList { get; set; }
        }
        public string GetJsonForRead(string RSN)
        {
            Root root = new Root();
            var table = db.EquipmentReportForm.Where(x => x.RSN == RSN).ToList();
            if (table.Count > 0) 
            {
                root.RSN = RSN;

                root.Date = table[0].Date?.ToString("yyyy/M/d HH:mm:ss");

                var levelDic = Surface.EquipmentReportFormState();
                root.ReportLevel = levelDic[table[0].ReportLevel];

                var userid = table[0].InformatUserID;
                var Reportuser = db.AspNetUsers.Where(x => x.UserName== userid).FirstOrDefault();
                root.MyName = Reportuser.MyName;

                var stateDic = Surface.EquipmentReportFormState();
                root.ReportState = stateDic[table[0].ReportState];

                var ESN = table[0].ESN;
                var equipmentinfo = db.EquipmentInfo.Where(x=>x.ESN == ESN).FirstOrDefault();
                root.Area = equipmentinfo.Area;

                root.Floor = equipmentinfo.Floor;

                root.PropertyCode = equipmentinfo.PropertyCode;

                root.ESN = table[0].ESN;

                root.EName = equipmentinfo.EName;

                root.ReportContent = table[0].ReportContent;

                var ReportImgList = db.ReportImage.Where(x=>x.RSN == RSN).Select(x=>x.ImgPath).ToList();
                root.ImgPath = ReportImgList;

                var inspectionplanlists = new List<InspectionPlanList>();
                //找出所有IPSN
                var IPSNlist = db.InspectionPlanRepair.Where(x => x.RSN == RSN).OrderBy(x => x.IPSN).Select(x => x.IPSN).ToList();
                foreach (var IPSN in IPSNlist)
                {
                    var inspectionplanlist = new InspectionPlanList();
                    #region 計劃資訊

                    var inspectionplantable = db.InspectionPlan.Find(IPSN);

                    var inspectionplan = new InspectionPlan();
                    inspectionplan.IPSN = inspectionplantable.IPSN;
                    inspectionplan.IPName = inspectionplantable.IPName;
                    inspectionplan.PlanDate = inspectionplantable.PlanDate.ToString("yyyy/M/d");
                    var PSdics = Surface.InspectionPlanState();
                    inspectionplan.PlanState = PSdics[inspectionplantable.PlanState];
                    inspectionplan.Shift = inspectionplantable.Shift;
                    var IPUserudlist = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID).ToList();
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
                    inspectionplan.MyName = INSPNameList;
                    inspectionplanlist.InspectionPlan = inspectionplan;
                    #endregion

                    #region 維修資料
                    var inspectionPlanRepair = new InspectionPlanRepair();
                    var inspectionPlanRepairtable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN && x.RSN == RSN).FirstOrDefault();

                    var IPRdics = Surface.InspectionPlanRepairState();
                    inspectionPlanRepair.RepairState = IPRdics[inspectionPlanRepairtable.RepairState];
                    inspectionPlanRepair.RepairContent = inspectionPlanRepairtable.RepairContent;

                    var IPRname = db.AspNetUsers.Where(x => x.UserName == inspectionPlanRepairtable.RepairUserID).Select(x => x.MyName).FirstOrDefault() as string;
                    inspectionPlanRepair.MyName = IPRname;
                    inspectionPlanRepair.RepairDate = inspectionPlanRepairtable.RepairDate?.ToString("yyyy/M/d");

                    var RPImgPathlist = db.RepairCompletionImage.Where(x => x.IPRSN == inspectionPlanRepairtable.IPRSN).Select(x => x.ImgPath).ToList();
                    inspectionPlanRepair.ImgPath = RPImgPathlist;

                    inspectionplanlist.InspectionPlanRepair = inspectionPlanRepair;

                    #endregion

                    #region 補件資料

                    var RSInfolist = new List<RepairSupplementaryInfo>();


                    var repairSupplementaryInfoTable = db.RepairSupplementaryInfo.Where(x => x.IPRSN == inspectionPlanRepairtable.IPRSN).OrderBy(x => x.PRSN).ToList();
                    foreach (var item in repairSupplementaryInfoTable)
                    {
                        var RSInfo = new RepairSupplementaryInfo();

                        var RSIname = db.AspNetUsers.Where(x => x.UserName == item.SupplementaryUserID).Select(x => x.MyName).FirstOrDefault() as string;

                        RSInfo.MyName = RSIname;
                        RSInfo.SupplementaryDate = item.SupplementaryDate.ToString("yyyy/M/d");
                        RSInfo.SupplementaryContent = item.SupplementaryContent;

                        var filepathlist = db.RepairSupplementaryFile.Where(x => x.PRSN == item.PRSN).Select(x => x.FilePath).ToList();

                        RSInfo.FilePath = filepathlist;

                        RSInfolist.Add(RSInfo);
                    }

                    inspectionplanlist.RepairSupplementaryInfo = RSInfolist;
                    #endregion

                    #region 審核資料

                    var RAInfolist = new List<RepairAuditInfo>();

                    var repairAuditInfoTable = db.RepairAuditInfo.Where(x => x.IPRSN == inspectionPlanRepairtable.IPRSN).OrderBy(x => x.PRASN).ToList();

                    foreach (var item in repairAuditInfoTable)
                    {
                        var RAInfo = new RepairAuditInfo();
                        var RAIname = db.AspNetUsers.Where(x => x.UserName == item.AuditUserID).Select(x => x.MyName).FirstOrDefault() as string;
                        RAInfo.MyName = RAIname;
                        RAInfo.AuditDate = item.AuditDate.ToString("yyyy/M/d");
                        var RAdics = Surface.AuditResult();
                        RAInfo.AuditResult = RAdics[item.AuditResult];
                        RAInfo.AuditMemo = item.AuditMemo.ToString();

                        var ImgList = db.RepairAuditImage.Where(x => x.PRASN == item.PRASN).Select(x => x.ImgPath).ToList();
                        RAInfo.ImgPath = ImgList;

                        RAInfolist.Add(RAInfo);
                    }

                    inspectionplanlist.RepairAuditInfo = RAInfolist;
                    #endregion

                    inspectionplanlists.Add(inspectionplanlist);
                }

                #region 塞多筆本維修單相關維修紀錄
                //#region 先藉由RSN去Inspection Plan Repair 找所有的IPSN
                //var aaatable = (from x1 in db.InspectionPlanRepair  //主表 巡檢計畫含維修
                //                join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN
                //                join x3 in db.RepairSupplementaryInfo on x1.IPRSN equals x3.IPRSN//巡檢計畫 Inspection Plan
                //                select new {x1.RSN, x1.IPSN}).Where(x => x.RSN == RSN).OrderBy(x => x.IPSN).AsQueryable().Count();
                //var slavetable = (from x1 in db.InspectionPlanRepair  //主表 巡檢計畫含維修
                //                  join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN //巡檢計畫 Inspection Plan
                //                  join x3 in db.RepairSupplementaryInfo on x1.IPRSN equals x3.IPRSN //維修補件資料 Repair Supplementary Info
                //                  join x4 in db.RepairAuditInfo on x1.IPRSN equals x4.IPRSN //維修審核資料 Repair Audit Info
                //                  join x5 in db.InspectionPlanMember on x1.IPSN equals x5.IPSN //查詢巡檢計畫人員名單(id)
                //                  join x6 in db.AspNetUsers on x5.UserID equals x6.UserName //藉由Inspection Plan id查詢人員真名
                //                  join x7 in db.AspNetUsers on x1.RepairUserID equals x7.UserName //藉由主表id查詢人員真名
                //                  join x8 in db.RepairCompletionImage on x1.RSN equals x8.ImgPath //藉由主表查詢所有維修照片 多個
                //                  join x9 in db.AspNetUsers on x3.SupplementaryUserID equals x9.UserName //藉由補件資料 id查詢補件人員真名
                //                  join x10 in db.RepairSupplementaryFile on x3.PRSN equals x10.PRSN //藉由補件資料查詢出所有檔案路徑 多個
                //                  join x11 in db.RepairAuditImage on x4.PRASN equals x11.PRASN //藉由維修審核資料找出相關照片路徑 多個
                //                  join x12 in db.AspNetUsers on x4.AuditUserID equals x12.UserName //藉由維修審核資料找出相關人員 真名
                //                  select new {
                //                      x1.RSN,
                //                      #region 巡檢計畫
                //                      x2.IPSN, //計畫編號
                //                      x2.IPName,//計畫名稱
                //                      x2.PlanDate, //計畫日期
                //                      x2.PlanState, //計畫執行狀態 需比對編碼
                //                      x2.Shift, //巡檢班別
                //                      InspectionName = x6.MyName, // 巡檢人員多筆 須以頓號連接 上傳時
                //                      #endregion
                //                      #region 維修資料
                //                      x1.RepairState, //本次維修狀態 需比對編碼
                //                      x1.RepairContent, //維修備註
                //                      ReportName = x7.MyName, //填報人員(人名)
                //                      x1.RepairDate, //填報時間
                //                      RepairImgpath = x8.ImgPath, //維修圖片路徑 多張
                //                      #endregion
                //                      #region 補件資料 多筆
                //                      x3.IPRSN,
                //                      x3.PRSN,
                //                      SupplementaryName = x9.MyName, //補件人員(人名)
                //                      x3.SupplementaryDate, //補件日期
                //                      x3.SupplementaryContent, //補件說明
                //                      x10.FilePath, //檔案路徑 多個
                //                      #endregion
                //                      #region 審核資料 多筆
                //                      x4.PRASN,
                //                      AuditName =x12.MyName, //審核人員(人名)
                //                      x4.AuditDate, //審核日期
                //                      x4.AuditResult, //審核結果 需比對編碼
                //                      x4.AuditMemo, //審核意見
                //                      AuditImgPath = x11.ImgPath //審核照片 多張
                //                      #endregion
                //                  }).Where(x=>x.RSN == RSN).OrderBy(x=>x.IPSN).AsQueryable();
                //int pikachu = slavetable.Count();
                //foreach (var item in slavetable)
                //{
                //    var inspectionPlanList = new InspectionPlanList();

                //    var inspectionPlan = new InspectionPlan();
                //    inspectionPlan.IPSN = item.IPSN;
                //    inspectionPlan.IPName= item.IPName;
                //    inspectionPlan.PlanDate = item.PlanDate.ToString("yyyy/M/d");
                //    inspectionPlan.Shift= item.Shift;
                //    string InspectionNames = "";
                //    int a = 0;
                //    foreach (var item2 in slavetable.Where(x => x.IPSN == item.IPSN)) {
                //        if (a == 0)
                //            InspectionNames += item2.IPSN;
                //        else
                //            InspectionNames += "、";
                //            InspectionNames+= item2.IPSN;
                //        a++;
                //    }
                //    a = 0;
                //    inspectionPlan.MyName = item.InspectionName;

                //    var inspectionPlanRepair = new InspectionPlanRepair();
                //    var repairdic = Surface.InspectionPlanRepairState();
                //    inspectionPlanRepair.RepairState = repairdic[item.RepairState];
                //    inspectionPlanRepair.RepairContent = item.RepairContent;
                //    inspectionPlanRepair.MyName= item.InspectionName;
                //    var IPRImgList = new List<string>();
                //    foreach (var item2 in slavetable.Where(x => x.RSN == RSN))
                //    {
                //        imgpathlist.Add(item2.RepairImgpath);
                //    }
                //    inspectionPlanRepair.ImgPath = IPRImgList;


                //    var repairSupplementaryInfoList = new List<RepairSupplementaryInfo>();
                //    foreach (var item2 in slavetable.Where(x => x.IPRSN == item.IPRSN).AsQueryable())
                //    {
                //        var repairSupplementaryInfo = new RepairSupplementaryInfo();
                //        repairSupplementaryInfo.MyName = item2.SupplementaryName;
                //        repairSupplementaryInfo.SupplementaryDate = item2.SupplementaryDate.ToString("yyyy/M/d");
                //        repairSupplementaryInfo.SupplementaryContent = item2.SupplementaryContent;
                //        var SMFlist= new List<string>();
                //        foreach (var item3 in slavetable.Where(x=>x.PRSN == x.PRSN && x.IPRSN == item2.IPRSN)) {
                //            SMFlist.Add(item3.FilePath);
                //        }
                //        repairSupplementaryInfo.FilePath = SMFlist;
                //    }

                //    var auditDics = Surface.AuditResult();
                //    var repairAuditInfoList = new List<RepairAuditInfo>();
                //    foreach (var item2 in slavetable.Where(x => x.PRASN == item.PRASN)) {
                //        var repairAuditInfo = new RepairAuditInfo();
                //        repairAuditInfo.MyName = item2.AuditName;
                //        repairAuditInfo.AuditDate = item2.AuditDate.ToString("yyyy/M/d");
                //        repairAuditInfo.AuditResult = auditDics[item2.AuditResult];
                //        repairAuditInfo.AuditMemo = item2.AuditMemo;    
                //        var AMGlist = new List<string>();

                //        foreach (var item3 in slavetable.Where(x => x.PRASN == item2.PRASN)){
                //            AMGlist.Add(item3.AuditImgPath);
                //        }
                //        repairAuditInfo.ImgPath = AMGlist;
                //    }
                //    int rows = slavetable.Where(x => x.IPSN == item.IPSN).Count();
                //    slavetable = slavetable.Skip(rows);
                //    inspectionplanlists.Add(inspectionPlanList);
                //}
                //#endregion

                #endregion
                root.InspectionPlanList = inspectionplanlists;
            }
            
            string result = JsonConvert.SerializeObject(root);
            return result;
        }
    }
}