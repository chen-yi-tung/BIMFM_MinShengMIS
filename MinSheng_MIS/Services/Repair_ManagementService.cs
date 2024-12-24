using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using static MinSheng_MIS.Models.ViewModels.ReadInspectionPlanPathData;

namespace MinSheng_MIS.Services
{
    public class Repair_ManagementService : IDisposable
    {
        Bimfm_MinSheng_MISEntities _db = new Bimfm_MinSheng_MISEntities();
        HttpServerUtilityBase _server = new HttpServerUtilityWrapper(HttpContext.Current.Server);

        #region WEB
        public void CreateFromWeb(Repair_ManagementWebCreateViewModel item)
        {
            EquipmentReportForm newForm = new EquipmentReportForm();
            newForm.RSN = GetNextRSN();
            newForm.ESN = item.ESN;
            newForm.ReportTime = DateTime.Now;
            newForm.ReportLevel = item.ReportLevel;
            newForm.ReportContent = item.ReportContent;
            newForm.InformatUserID = HttpContext.Current.User.Identity.Name;
            newForm.ReportState = "1";
            newForm.ReportSource = "1";
            newForm.ReportImg = SaveImageFromHttpPostedFileBase(newForm.RSN, item.ReportImg);
            _db.EquipmentReportForm.Add(newForm);
            EquipmentInfo equipment = _db.EquipmentInfo.Find(newForm.ESN);
            equipment.EState = "2";
            _db.SaveChanges();
        }

        public void Assignment(Repair_ManagementAssignmentViewModel item)
        {
            foreach (var user in item.RepairUserName)
            {
                foreach (var rsn in item.RSN)
                {
                    var dbItem = _db.EquipmentReportForm.Find(rsn);
                    dbItem.DueDate = item.DueDate;
                    dbItem.ReportState = "2";
                    Equipment_ReportFormMember newAssignment = new Equipment_ReportFormMember();
                    newAssignment.ERFMSN = GetNextERFMSN(rsn);
                    newAssignment.RSN = rsn;
                    newAssignment.RepairUserName = user;
                    _db.Equipment_ReportFormMember.Add(newAssignment);
                    _db.SaveChanges();
                }
            }
        }

        public JObject Detail(string rsn)
        {
            var item = _db.EquipmentReportForm.Find(rsn);
            JObject jo = new JObject();
            jo.Add("RSN", item.RSN);
            jo.Add("ReportState", Surface.ReportState()[item.ReportState]);
            jo.Add("ReportLevel", Surface.ReportLevel()[item.ReportLevel]);
            jo.Add("ReportTime", item.ReportTime.ToString("yyyy/MM/dd HH:mm"));
            jo.Add("ReportContent", item.ReportContent);
            jo.Add("ReportImg", item.ReportImg);
            jo.Add("RepairUserName", "");
            var memberlist = _db.Equipment_ReportFormMember.Where(e => e.RSN == item.RSN).ToList();
            foreach (var member in memberlist)
            {
                if (!string.IsNullOrEmpty(jo["RepairUserName"]?.ToString()))
                    jo["RepairUserName"] = $"{jo["RepairUserName"]}、{member.RepairUserName}";
                else
                    jo["RepairUserName"] = member.RepairUserName;
            }
            jo.Add("Location", $"{item.EquipmentInfo.Floor_Info.AreaInfo.Area} {item.EquipmentInfo.Floor_Info.FloorName}");
            jo.Add("NO", item.EquipmentInfo.NO);
            jo.Add("EName", item.EquipmentInfo.EName);
            jo.Add("ESN", item.EquipmentInfo.ESN);
            jo.Add("RepairResult", item.RepairtId != null ? "完成" : "未完成");
            jo.Add("RepairtId", item.RepairtId);
            jo.Add("RepairTime", item.RepairTime?.ToString("yyyy/MM/dd HH:mm"));
            jo.Add("RepairContent", item.RepairContent);
            jo.Add("RepairImg", item.RepairImg);
            jo.Add("AuditResult", item.AuditResult == null ? "" : (bool)item.AuditResult ? "通過" : "未通過");
            jo.Add("AuditId", item.AuditId);
            jo.Add("AuditTime", item.AuditTime?.ToString("yyyy/MM/dd HH:mm"));
            jo.Add("AuditReason", item.AuditReason);
            return jo;
        }

        public void Audit(Repair_ManagementAuditViewModel item)
        {
            var dbItem = _db.EquipmentReportForm.Find(item.RSN);
            dbItem.AuditId = HttpContext.Current.User.Identity.Name;
            dbItem.AuditTime = DateTime.Now;
            dbItem.AuditReason = item.AuditReason;
            dbItem.AuditResult = item.AuditResult;
            if (item.AuditResult)
            {
                EquipmentInfo equipment = _db.EquipmentInfo.Find(dbItem.ESN);
                equipment.EState = "1";
            }
            _db.SaveChanges();
        }
        #endregion

        #region APP
        public JArray GetEquipmentByRFID(List<string> rfids)
        {
            JArray ja = new JArray();
            var equipmentSet = _db.RFID.Where(r => rfids.Contains(r.RFIDInternalCode)).Select(r => r.EquipmentInfo).ToHashSet();
            foreach (var equipment in equipmentSet)
            {
                JObject itemObject = new JObject();
                itemObject.Add("ESN", equipment.ESN);
                itemObject.Add("EName", equipment.EName);
                itemObject.Add("EState", Surface.EState()[equipment.EState]);
                itemObject.Add("NO", equipment.NO);
                itemObject.Add("Location", $"{equipment.Floor_Info.AreaInfo.Area} {equipment.Floor_Info.FloorName}");
                ja.Add(itemObject);
            }
            return ja;
        }

        public JObject EquipmentDetail(string esn)
        {
            JObject jo = new JObject();
            var equipment = _db.EquipmentInfo.Find(esn);
            jo.Add("EName", equipment.EName);
            jo.Add("NO", equipment.NO);
            jo.Add("Model", equipment.Model);
            jo.Add("Area", equipment.Floor_Info.AreaInfo.Area);
            jo.Add("FloorName", equipment.Floor_Info.FloorName);
            jo.Add("OperatingVoltage", equipment.OperatingVoltage);
            jo.Add("OtherInfo", equipment.OtherInfo);

            jo.Add("Brand", equipment.Brand);
            jo.Add("Vendor", equipment.Vendor);
            jo.Add("ContactPhone", equipment.ContactPhone);
            jo.Add("InstallDate", equipment.InstallDate);

            jo.Add("Memo", equipment.Memo);

            var maintenanceFrom = _db.Equipment_MaintenanceForm.FirstOrDefault(e => e.ESN == esn);
            if (maintenanceFrom != null)
            {
                jo.Add("MaintainName", maintenanceFrom.MaintainName);
                jo.Add("Period", Surface.MaintainPeriod()[maintenanceFrom.Period]);
            }
            return jo;
        }

        public void AddOrUpdateFromApp(Repair_ManagementAddOrUpdateViewModel item)
        {
            EquipmentReportForm newForm = new EquipmentReportForm();
            //新增
            if (item.RSN == null)
            {
                newForm.RSN = GetNextRSN();
                newForm.ReportTime = DateTime.Now;
                newForm.ReportState = "1";
            }
            //編輯
            else
            {
                newForm.RSN = item.RSN;
            }
            newForm.ESN = item.ESN;
            newForm.ReportLevel = item.ReportLevel;
            newForm.ReportContent = item.ReportContent;
            newForm.InformatUserID = item.UserName;
            newForm.ReportSource = "1";
            if (item.ReportImg != null)
                newForm.ReportImg = SaveImageFromHttpPostedFile(newForm.RSN, item.ReportImg);
            _db.EquipmentReportForm.AddOrUpdate(newForm);
            EquipmentInfo equipment = _db.EquipmentInfo.Find(newForm.ESN);
            equipment.EState = "2";
            _db.SaveChanges();
        }

        public JArray GetEquipmentBySearch(Repair_ManagementAppSearchEquipmentViewModel item)
        {
            JArray ja = new JArray();
            var equipmentTable = _db.EquipmentInfo.AsQueryable();
            if (item.ASN != null) equipmentTable = equipmentTable.Where(e => e.Floor_Info.ASN == item.ASN);
            if (!string.IsNullOrEmpty(item.FSN)) equipmentTable = equipmentTable.Where(e => e.FSN == item.FSN);
            if (!string.IsNullOrEmpty(item.EName)) equipmentTable = equipmentTable.Where(e => e.EName == item.EName);
            if (!string.IsNullOrEmpty(item.NO)) equipmentTable = equipmentTable.Where(e => e.NO == item.NO);
            var equipmentList = equipmentTable.ToList();
            foreach (var equipment in equipmentList)
            {
                JObject itemObject = new JObject();
                itemObject.Add("ESN", equipment.ESN);
                itemObject.Add("EName", equipment.EName);
                itemObject.Add("EState", Surface.EState()[equipment.EState]);
                itemObject.Add("NO", equipment.NO);
                itemObject.Add("Location", $"{equipment.Floor_Info.AreaInfo.Area} {equipment.Floor_Info.FloorName}");
                ja.Add(itemObject);
            }
            return ja;
        }

        public JArray GetRepairList()
        {
            JArray ja = new JArray();
            var repairList = _db.EquipmentReportForm.ToList();
            foreach (var repair in repairList)
            {
                JObject itemObject = new JObject();
                itemObject.Add("RSN", repair.RSN);
                itemObject.Add("ReportState", Surface.ReportState()[repair.ReportState]);
                itemObject.Add("EName", repair.EquipmentInfo.EName);
                itemObject.Add("NO", repair.EquipmentInfo.NO);
                itemObject.Add("ReportTime", repair.ReportTime.ToString("yyyy-MM-dd"));
                itemObject.Add("Location", $"{repair.EquipmentInfo.Floor_Info.AreaInfo.Area} {repair.EquipmentInfo.Floor_Info.FloorName}");
                itemObject.Add("ReportLevel", Surface.ReportLevel()[repair.ReportLevel]);
                ja.Add(itemObject);
            }
            return ja;
        }

        public JObject AppDetail(string rsn)
        {
            var item = _db.EquipmentReportForm.Find(rsn);
            JObject jo = new JObject();
            jo.Add("EName", item.EquipmentInfo.EName);
            jo.Add("NO", item.EquipmentInfo.NO);
            jo.Add("Area", item.EquipmentInfo.Floor_Info.AreaInfo.Area);
            jo.Add("FloorName", item.EquipmentInfo.Floor_Info.FloorName);
            jo.Add("RSN", item.RSN);
            jo.Add("ReportState", Surface.ReportState()[item.ReportState]);
            jo.Add("ReportLevel", item.ReportLevel);
            jo.Add("ReportContent", item.ReportContent);
            jo.Add("ReportImg", item.ReportImg);
            return jo;
        }

        public void AppDelete(string rsn)
        {
            var item = _db.EquipmentReportForm.Find(rsn);
            if (!_db.EquipmentReportForm.Any(e => e.ESN == item.ESN && e.ReportState != "4"))
            {
                EquipmentInfo equipment = _db.EquipmentInfo.Find(item.ESN);
                equipment.EState = "1";
            }
            var memberList = _db.Equipment_ReportFormMember.Where(e => e.RSN == item.RSN).ToList();
            _db.Equipment_ReportFormMember.RemoveRange(memberList);
            _db.EquipmentReportForm.Remove(item);
            _db.SaveChanges();
        }
        #endregion

        public string GetNextRSN()
        {
            string prefix = "R" + DateTime.Now.ToString("yyMMdd");
            string rsn = _db.EquipmentReportForm.Select(e => e.RSN).Where(r => r.StartsWith(prefix)).OrderByDescending(r => r).FirstOrDefault();
            if (rsn != null) rsn = prefix + (int.Parse(rsn.Substring(prefix.Length)) + 1).ToString().PadLeft(6, '0');
            else rsn = prefix + "000001";
            return rsn;
        }

        public string GetNextERFMSN(string rsn)
        {
            string erfmsn = _db.Equipment_ReportFormMember.Select(e => e.ERFMSN).Where(e => e.StartsWith(rsn)).OrderByDescending(e => e).FirstOrDefault();
            if (erfmsn != null) erfmsn = rsn + (int.Parse(erfmsn.Substring(rsn.Length)) + 1).ToString().PadLeft(2, '0');
            else erfmsn = rsn + "01";
            return erfmsn;
        }

        public string SaveImageFromHttpPostedFileBase(string rsn, HttpPostedFileBase img)
        {
            if (img != null && img.ContentLength > 0)
            {
                string extension = Path.GetExtension(img.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                    throw new Exception("圖片僅接受jpg、jpeg、png、pdf");
                if (!Directory.Exists(_server.MapPath("/Files/Repair_Management"))) Directory.CreateDirectory(_server.MapPath("/Files/Repair_Management"));
                string path = $"/Files/Repair_Management/{rsn}{extension}";
                string fullPath = _server.MapPath($"{path}");
                img.SaveAs(fullPath);
                return path;
            }
            return null;
        }

        public string SaveImageFromHttpPostedFile(string rsn, HttpPostedFile img)
        {
            if (img != null && img.ContentLength > 0)
            {
                string extension = Path.GetExtension(img.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                    throw new Exception("圖片僅接受jpg、jpeg、png、pdf");
                if (!Directory.Exists(_server.MapPath("/Files/Repair_Management"))) Directory.CreateDirectory(_server.MapPath("/Files/Repair_Management"));
                string path = $"/Files/Repair_Management/{rsn}{extension}";
                string fullPath = _server.MapPath($"{path}");
                img.SaveAs(fullPath);
                return path;
            }
            return null;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}