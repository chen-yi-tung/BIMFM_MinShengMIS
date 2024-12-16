using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{


    public class Repair_ManagementService : IDisposable
    {
        Bimfm_MinSheng_MISEntities _db = new Bimfm_MinSheng_MISEntities();
        HttpServerUtilityBase _server = new HttpServerUtilityWrapper(HttpContext.Current.Server);

        #region WEB
        public void CreateFromWeb(Repair_ManagementCreateViewModel item)
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
            if (item.ReportImg != null && item.ReportImg.ContentLength > 0)
            {
                string extension = Path.GetExtension(item.ReportImg.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                    throw new Exception("圖片僅接受jpg、jpeg、png、pdf");
                if (!Directory.Exists(_server.MapPath("/Files/Repair_Management"))) Directory.CreateDirectory(_server.MapPath("/Files/Repair_Management"));
                string path = $"/Files/Repair_Management/{newForm.RSN}{extension}";
                string fullPath = _server.MapPath($"{path}");
                item.ReportImg.SaveAs(fullPath);
                newForm.ReportImg = path;
            }
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
            JObject itemObject = new JObject();
            itemObject.Add("RSN", item.RSN);
            itemObject.Add("ReportState", Surface.ReportState()[item.ReportState]);
            itemObject.Add("ReportLevel", Surface.ReportLevel()[item.ReportLevel]);
            itemObject.Add("ReportTime", item.ReportTime.ToString("yyyy/MM/dd HH:mm"));
            itemObject.Add("ReportContent", item.ReportContent);
            itemObject.Add("ReportImg", item.ReportImg);
            itemObject.Add("RepairUserName", "");
            var memberlist = _db.Equipment_ReportFormMember.Where(e => e.RSN == item.RSN).ToList();
            foreach (var member in memberlist)
            {
                if (!string.IsNullOrEmpty(itemObject["RepairUserName"]?.ToString()))
                    itemObject["RepairUserName"] = $"{itemObject["RepairUserName"]}、{member.RepairUserName}";
                else
                    itemObject["RepairUserName"] = member.RepairUserName;
            }
            itemObject.Add("Location", $"{item.EquipmentInfo.Floor_Info.AreaInfo.Area} {item.EquipmentInfo.Floor_Info.FloorName}");
            itemObject.Add("No", item.EquipmentInfo.NO);
            itemObject.Add("EName", item.EquipmentInfo.EName);
            itemObject.Add("ESN", item.EquipmentInfo.ESN);
            itemObject.Add("RepairResult", item.RepairtId != null ? "完成" : "未完成");
            itemObject.Add("RepairtId", item.RepairtId);
            itemObject.Add("RepairTime", item.RepairTime?.ToString("yyyy/MM/dd HH:mm"));
            itemObject.Add("RepairContent", item.RepairContent);
            itemObject.Add("AuditResult", item.AuditResult == null ? "" : (bool)item.AuditResult ? "通過" : "未通過");
            itemObject.Add("AuditId", item.AuditId);
            itemObject.Add("AuditTime", item.AuditTime?.ToString("yyyy/MM/dd HH:mm"));
            itemObject.Add("AuditReason", item.AuditReason);
            return itemObject;
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

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}