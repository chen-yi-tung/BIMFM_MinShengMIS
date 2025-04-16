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
using System.Linq.Dynamic.Core;
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
            if (_db.EquipmentInfo.Any(e => e.ESN == item.ESN && e.EState == "3"))
                throw new Exception("此設備停用中");
            if (_db.EquipmentInfo.Any(e => e.ESN == item.ESN && e.EState == "2"))
                throw new Exception("此設備已經報修中");
            EquipmentReportForm newForm = new EquipmentReportForm();
            newForm.RSN = GetNextRSN();
            newForm.ESN = item.ESN;
            newForm.ReportTime = DateTime.Now;
            newForm.ReportLevel = item.ReportLevel;
            newForm.ReportContent = item.ReportContent;
            newForm.InformatUserID = HttpContext.Current.User.Identity.Name;
            newForm.ReportState = "1";
            newForm.ReportSource = "1";
            newForm.ReportImg = SaveImageFromHttpPostedFileBase($"{newForm.RSN}_Report", item.ReportImg);
            _db.EquipmentReportForm.Add(newForm);
            EquipmentInfo equipment = _db.EquipmentInfo.Find(newForm.ESN);
            equipment.EState = "2";
            SuspendMaintenance(newForm.ESN);
            _db.SaveChanges();
        }

        public void Assignment(Repair_ManagementAssignmentViewModel item)
        {
            foreach (var user in item.RepairUserName)
            {
                foreach (var rsn in item.RSN)
                {
                    var dbItem = _db.EquipmentReportForm.Find(rsn);
                    dbItem.Dispatcher = HttpContext.Current.User.Identity.Name;
                    dbItem.DispatcherTime = DateTime.Now;
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
            jo.Add("ReportTime", item.ReportTime.ToString("yyyy-MM-dd HH:mm"));
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
            jo.Add("RepairTime", item.RepairTime?.ToString("yyyy-MM-dd HH:mm"));
            jo.Add("RepairContent", item.RepairContent);
            jo.Add("RepairImg", item.RepairImg);
            jo.Add("AuditResult", item.AuditResult == null ? "" : (bool)item.AuditResult ? "通過" : "未通過");
            jo.Add("AuditId", item.AuditId);
            jo.Add("AuditTime", item.AuditTime?.ToString("yyyy-MM-dd HH:mm"));
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
                dbItem.ReportState = "4";
                EquipmentInfo equipment = _db.EquipmentInfo.Find(dbItem.ESN);
                if (equipment.EState == "2") equipment.EState = "1";
                ResumeMaintenance(dbItem.ESN);
            }
            else
            {
                dbItem.ReportState = "5";
            }
            _db.SaveChanges();
        }
        #endregion

        #region APP
        #region 報修
        public JArray GetEquipmentByRFID(List<string> rfids)
        {
            JArray ja = new JArray();
            var equipmentSet = _db.RFID.Where(r => rfids.Contains(r.RFIDInternalCode.Substring(4, r.RFIDInternalCode.Length - 8))).Select(r => r.EquipmentInfo).ToHashSet();
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
            jo.Add("EPhoto", $"Files/EquipmentInfo/{equipment.EPhoto}");

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
                if (_db.EquipmentInfo.Any(e => e.ESN == item.ESN && e.EState == "3"))
                    throw new Exception("此設備停用中");
                if (_db.EquipmentInfo.Any(e => e.ESN == item.ESN && e.EState == "2"))
                    throw new Exception("此設備已經報修中");
                newForm.RSN = GetNextRSN();
                newForm.ReportTime = DateTime.Now;
                newForm.ReportState = "1";
                newForm.ESN = item.ESN;
                SuspendMaintenance(newForm.ESN);
            }
            //編輯
            else
            {
                newForm = _db.EquipmentReportForm.Find(item.RSN);
            }
            newForm.ReportLevel = item.ReportLevel;
            newForm.ReportContent = item.ReportContent;
            newForm.InformatUserID = item.UserName;
            newForm.ReportSource = "1";
            if (item.ReportImg != null)
                newForm.ReportImg = SaveImageFromHttpPostedFile($"{newForm.RSN}_Report", item.ReportImg);
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

        public JArray RepairList(Repair_ManagementRepairListFilterViewModel item)
        {
            JArray ja = new JArray();
            var repairTable = _db.EquipmentReportForm.Where(r => r.ReportState == "1" && r.InformatUserID == item.UserName).AsQueryable();
            if (item.Date != null)
            {
                var dateEnd = item.Date?.AddDays(1);
                repairTable = repairTable.Where(r => item.Date <= r.ReportTime && r.ReportTime < dateEnd);
            }
            var repairList = repairTable.OrderByDescending(r => r.ReportTime).ToList();
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
            jo.Add("ESN", item.ESN);
            jo.Add("EName", item.EquipmentInfo.EName);
            jo.Add("NO", item.EquipmentInfo.NO);
            jo.Add("Area", item.EquipmentInfo.Floor_Info.AreaInfo.Area);
            jo.Add("FloorName", item.EquipmentInfo.Floor_Info.FloorName);
            jo.Add("RSN", item.RSN);
            jo.Add("ReportState", Surface.ReportState()[item.ReportState]);
            jo.Add("ReportLevel", item.ReportLevel);
            jo.Add("ReportContent", item.ReportContent);
            jo.Add("ReportImg", item.ReportImg);
            jo.Add("ReportTime", item.ReportTime);
            return jo;
        }

        public void AppDelete(string rsn)
        {
            var item = _db.EquipmentReportForm.Find(rsn);
            if (!_db.EquipmentReportForm.Any(e => e.ESN == item.ESN && e.ReportState != "4"))
            {
                EquipmentInfo equipment = _db.EquipmentInfo.Find(item.ESN);
                if (equipment.EState == "2") equipment.EState = "1";
            }
            var memberList = _db.Equipment_ReportFormMember.Where(e => e.RSN == item.RSN).ToList();
            _db.Equipment_ReportFormMember.RemoveRange(memberList);
            _db.EquipmentReportForm.Remove(item);
            _db.SaveChanges();
        }

        public JArray RepairRecord(Repair_ManagementRepairRecordViewModel item)
        {
            JArray ja = new JArray();
            var repairTable = _db.EquipmentReportForm.Where(e => e.ESN == item.ESN).AsQueryable();
            if (item.Order == "ASC") repairTable = repairTable.OrderBy(e => e.ReportTime);
            else repairTable = repairTable.OrderByDescending(e => e.ReportTime);
            var repairList = repairTable.ToList();
            foreach (var repair in repairList)
            {
                JObject itemObject = new JObject();
                itemObject.Add("ReportTime", repair.ReportTime.ToString("yyyy-MM-dd"));
                itemObject.Add("ReportContent", repair.ReportContent);
                itemObject.Add("ReportState", Surface.ReportState()[repair.ReportState]);
                itemObject.Add("RepairTime", repair.RepairTime?.ToString("yyyy-MM-dd"));
                itemObject.Add("RepairUserName", string.Join(",", _db.Equipment_ReportFormMember.Where(e => e.RSN == repair.RSN).Select(e => e.RepairUserName).ToList()));
                ja.Add(itemObject);
            }
            return ja;
        }
        #endregion

        #region 維修
        public JArray RepairWorkList(Repair_ManagementRepairWorkSortViewModel item)
        {
            JArray ja = new JArray();
            var repairTable = _db.EquipmentReportForm.Where(e => (e.ReportState == "2" || e.ReportState == "5") && e.Equipment_ReportFormMember.Any(m => m.RepairUserName == item.UserName)).AsQueryable();
            if (item.Order == "ASC")
            {
                repairTable = repairTable.OrderBy(r => r.DispatcherTime);
            }
            else
            {
                repairTable = repairTable.OrderByDescending(r => r.DispatcherTime);
            }
            var repairList = repairTable.ToList();
            foreach (var repair in repairList)
            {
                JObject itemObject = new JObject();
                itemObject.Add("RSN", repair.RSN);
                itemObject.Add("ReportState", Surface.ReportState()[repair.ReportState]);
                itemObject.Add("EName", repair.EquipmentInfo.EName);
                itemObject.Add("NO", repair.EquipmentInfo.NO);
                itemObject.Add("Area", repair.EquipmentInfo.Floor_Info.AreaInfo.Area);
                itemObject.Add("FloorName", repair.EquipmentInfo.Floor_Info.FloorName);
                itemObject.Add("ReportLevel", Surface.ReportLevel()[repair.ReportLevel]);
                JArray rfidJArray = new JArray();
                foreach (var rfid in _db.RFID.Where(r => r.ESN == repair.ESN))
                {
                    rfidJArray.Add(new JObject() { { "RFIDInternalCode", rfid.RFIDInternalCode } });
                }
                itemObject.Add("RFIDS", rfidJArray);
                ja.Add(itemObject);
            }
            return ja;
        }

        public void RepairWorkFillin(Repair_ManagementRepairFillinViewModel item)
        {
            var form = _db.EquipmentReportForm.Find(item.RSN);
            form.RepairContent = item.RepairContent;
            form.RepairtId = item.RepairtId;
            if (item.RepairImg != null)
                form.RepairImg = SaveImageFromHttpPostedFile($"{item.RSN}_Repair", item.RepairImg);
            form.RepairTime = DateTime.Now;
            form.ReportState = "3";
            _db.SaveChanges();
        }
        #endregion
        #endregion

        #region 保養借放(設備保養紀錄)
        public JArray MaintenanceRecord(MaitenanceRecordViewModel item)
        {
            JArray ja = new JArray();
            var maintenanceTable = _db.Equipment_MaintenanceForm.Where(e => e.ESN == item.ESN).AsQueryable();
            if (item.Order == "ASC") maintenanceTable = maintenanceTable.OrderBy(e => e.ReportTime);
            else maintenanceTable = maintenanceTable.OrderByDescending(e => e.ReportTime);
            var maintenanceList = maintenanceTable.ToList();
            foreach (var maintenance in maintenanceList)
            {
                JObject itemObject = new JObject();
                itemObject.Add("DispatcherTime", maintenance.DispatcherTime?.ToString("yyyy-MM-dd"));
                itemObject.Add("MaintainName", maintenance.MaintainName);
                itemObject.Add("Maintainer", string.Join(",", _db.Equipment_MaintenanceFormMember.Where(e => e.EMFSN == maintenance.EMFSN).Select(e => e.Maintainer).ToList()));
                itemObject.Add("Status", Surface.MaintainStatus()[maintenance.Status]);
                ja.Add(itemObject);
            }
            return ja;
        }
        #endregion

        #region 工具
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

        public string SaveImageFromHttpPostedFileBase(string fileName, HttpPostedFileBase img)
        {
            if (img != null && img.ContentLength > 0)
            {
                string extension = Path.GetExtension(img.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                    throw new Exception("圖片僅接受jpg、jpeg、png、pdf");
                if (!Directory.Exists(_server.MapPath("/Files/Repair_Management"))) Directory.CreateDirectory(_server.MapPath("/Files/Repair_Management"));
                string path = $"/Files/Repair_Management/{fileName}{extension}";
                string fullPath = _server.MapPath($"{path}");
                img.SaveAs(fullPath);
                return path;
            }
            return null;
        }

        public string SaveImageFromHttpPostedFile(string fileName, HttpPostedFile img)
        {
            if (img != null && img.ContentLength > 0)
            {
                string extension = Path.GetExtension(img.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                    throw new Exception("圖片僅接受jpg、jpeg、png、pdf");
                if (!Directory.Exists(_server.MapPath("/Files/Repair_Management"))) Directory.CreateDirectory(_server.MapPath("/Files/Repair_Management"));
                string path = $"/Files/Repair_Management/{fileName}{extension}";
                string fullPath = _server.MapPath($"{path}");
                img.SaveAs(fullPath);
                return path;
            }
            return null;
        }

        public void SuspendMaintenance(string esn)
        {
            var maintenanceList = _db.Equipment_MaintenanceForm.Where(e => (e.Status == "1" || e.Status == "2") && e.ESN == esn).ToList();
            var emfsnList = maintenanceList.Select(m => m.EMFSN).ToList();
            var memberList = _db.Equipment_MaintenanceFormMember.Where(e => emfsnList.Contains(e.EMFSN)).ToList();
            _db.Equipment_MaintenanceFormMember.RemoveRange(memberList);
            _db.Equipment_MaintenanceForm.RemoveRange(maintenanceList);
            var maintenanceItem = _db.Equipment_MaintainItemValue.Where(e => e.ESN == esn).ToList();
            foreach (var item in maintenanceItem)
            {
                item.IsCreateForm = false;
            }
            _db.SaveChanges();
        }

        public void ResumeMaintenance(string esn)
        {
            DateTime InOneMonth = DateTime.Today.AddMonths(1);
            var maintenanceItem = _db.Equipment_MaintainItemValue.Where(e => e.ESN == esn).ToList();
            foreach (var item in maintenanceItem)
            {
                if (item.NextMaintainDate != null && item.NextMaintainDate <= InOneMonth)
                {
                    Equipment_MaintenanceForm newForm = new Equipment_MaintenanceForm();
                    newForm.EMFSN = GetNextEMFSN(item.NextMaintainDate?.ToString("yyMMdd"));
                    newForm.ESN = esn;
                    newForm.MISSN = item.MISSN;
                    newForm.MaintainName = item.Template_MaintainItemSetting.MaintainName;
                    newForm.Period = item.Period;
                    newForm.lastMaintainDate = item.lastMaintainDate;
                    newForm.NextMaintainDate = (DateTime)item.NextMaintainDate;
                    newForm.Status = "1";
                    _db.Equipment_MaintenanceForm.Add(newForm);
                }
                item.IsCreateForm = true;
                _db.SaveChanges();
            }
        }

        public string GetNextEMFSN(string date)
        {
            string prefix = "M" + date;
            string emfsn = _db.Equipment_MaintenanceForm.Select(e => e.EMFSN).Where(e => e.StartsWith(prefix)).OrderByDescending(e => e).FirstOrDefault();
            if (emfsn != null) emfsn = prefix + (int.Parse(emfsn.Substring(prefix.Length)) + 1).ToString().PadLeft(6, '0');
            else emfsn = prefix + "000001";
            return emfsn;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        #endregion
    }
}