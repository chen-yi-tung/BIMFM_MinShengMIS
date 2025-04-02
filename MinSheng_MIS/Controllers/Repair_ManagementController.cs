using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Services.Helpers;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Repair_ManagementController : Controller
    {
        #region 頁面
        #region 報修管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 報修單
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 報修單 審核
        public ActionResult Review()
        {
            string userName = HttpContext.User.Identity.Name;
            using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
            {
                ViewBag.MyName = db.AspNetUsers.Where(a => a.UserName == userName).Select(a => a.MyName).FirstOrDefault();
            }
            return View();
        }
        #endregion

        #region 報修單 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion
        #endregion

        #region API
        #region 報修管理 DataGrid
        [HttpPost]
        public ActionResult ManagementDataGrid(FormCollection form)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                DatagridService ds = new DatagridService();
                jo["Datas"] = ds.RepairManagementDataGrid(form);
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 新增 報修單
        [HttpPost]
        public ActionResult Create(Repair_ManagementWebCreateViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.CreateFromWeb(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修單 派工
        [HttpPost]
        public ActionResult Assignment(Repair_ManagementAssignmentViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.Assignment(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修單 詳情
        [HttpPost]
        public ActionResult Detail(string rsn)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    jo["Datas"] = ds.Detail(rsn);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修單 審核
        [HttpPost]
        public ActionResult Audit(Repair_ManagementAuditViewModel item)
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.Audit(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json;charset=utf-8");
        }
        #endregion

        #region 報修管理 匯出
        public ActionResult ExportToExcel(FormCollection form)
        {
            try
            {
                JObject jo = new JObject();
                DatagridService ds = new DatagridService();
                form.Add("rows", int.MaxValue.ToString());
                jo = ds.RepairManagementDataGrid(form);
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("報修管理");

                    worksheet.Cells["A1"].Value = "報修單狀態";
                    worksheet.Cells["B1"].Value = "報修單號";
                    worksheet.Cells["C1"].Value = "報修等級";
                    worksheet.Cells["D1"].Value = "報修時間";
                    worksheet.Cells["E1"].Value = "報修內容";
                    worksheet.Cells["F1"].Value = "棟別";
                    worksheet.Cells["G1"].Value = "樓層";
                    worksheet.Cells["H1"].Value = "設備名稱";
                    worksheet.Cells["I1"].Value = "設備編號";
                    worksheet.Cells["J1"].Value = "執行人員";

                    int row = 2;
                    foreach (var item in jo["rows"])
                    {
                        worksheet.Cells["A" + row].Value = item["ReportState"].ToString();
                        worksheet.Cells["B" + row].Value = item["RSN"].ToString();
                        worksheet.Cells["C" + row].Value = item["ReportLevel"].ToString();
                        worksheet.Cells["D" + row].Value = item["ReportTime"].ToString();
                        worksheet.Cells["E" + row].Value = item["ReportContent"].ToString();
                        worksheet.Cells["F" + row].Value = item["Area"].ToString();
                        worksheet.Cells["G" + row].Value = item["FloorName"].ToString();
                        worksheet.Cells["H" + row].Value = item["EName"].ToString();
                        worksheet.Cells["I" + row].Value = item["NO"].ToString();
                        worksheet.Cells["J" + row].Value = item["RepairMyName"].ToString();
                        row++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", $"attachment; filename=報修管理{DateTime.Now.Ticks}.xlsx");
                    Response.BinaryWrite(package.GetAsByteArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
            }
            return null;
        }
        #endregion
        #endregion
    }
}