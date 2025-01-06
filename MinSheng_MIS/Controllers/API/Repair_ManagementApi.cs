using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace MinSheng_MIS.Controllers.API
{
    #region 報修
    public class GetEquipmentByRFIDController : ApiController
    {
        public JObject Post([FromBody] List<RFID> rfids)
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
                    List<string> rfidInternalCodes = new List<string>();
                    foreach (var rfid in rfids)
                    {
                        rfidInternalCodes.Add(rfid.RFIDInternalCode);
                    }
                    jo["Datas"] = ds.GetEquipmentByRFID(rfidInternalCodes);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class EquipmentDetailController : ApiController
    {
        public JObject Post([FromBody] EquipmentInfo equipment)
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
                    jo["Datas"] = ds.EquipmentDetail(equipment.ESN);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairCreateController : ApiController
    {
        public JObject Post()
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                var form = HttpContext.Current.Request.Form;

                // 建立模型並綁定表單資料
                var item = new Repair_ManagementAddOrUpdateViewModel
                {
                    ESN = form["ESN"],
                    ReportLevel = form["ReportLevel"],
                    ReportContent = form["ReportContent"],
                    UserName = HttpContext.Current.User.Identity.Name
                };
                item.ReportImg = HttpContext.Current.Request.Files[0];
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.AddOrUpdateFromApp(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class GetEquipmentBySearchController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementAppSearchEquipmentViewModel item)
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
                    jo["Datas"] = ds.GetEquipmentBySearch(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairListController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementRepairListFilterViewModel item)
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
                    if (item == null) item = new Repair_ManagementRepairListFilterViewModel();
                    item.UserName = HttpContext.Current.User.Identity.Name;
                    jo["Datas"] = ds.RepairList(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairDetailController : ApiController
    {
        public JObject Post([FromBody] EquipmentReportForm item)
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
                    jo["Datas"] = ds.AppDetail(item.RSN);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairEditController : ApiController
    {
        public JObject Post()
        {
            JObject jo = new JObject()
            {
                { "State", "Success" },
                { "ErrorMessage", "" },
                { "Datas", "" }
            };
            try
            {
                var form = HttpContext.Current.Request.Form;

                // 建立模型並綁定表單資料
                var item = new Repair_ManagementAddOrUpdateViewModel
                {
                    RSN = form["RSN"],
                    ReportLevel = form["ReportLevel"],
                    ReportContent = form["ReportContent"],
                    UserName = HttpContext.Current.User.Identity.Name
                };
                if (HttpContext.Current.Request.Files.Count > 0)
                    item.ReportImg = HttpContext.Current.Request.Files[0];
                using (Repair_ManagementService ds = new Repair_ManagementService())
                {
                    ds.AddOrUpdateFromApp(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairDeleteController : ApiController
    {
        public JObject Post([FromBody] EquipmentReportForm item)
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
                    ds.AppDelete(item.RSN);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairRecordController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementRepairRecordViewModel item)
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
                    jo["Datas"] = ds.RepairRecord(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion

    #region 維修
    public class RepairWorkListController : ApiController
    {
        public JObject Post([FromBody] Repair_ManagementRepairWorkSortViewModel item)
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
                    item.UserName = HttpContext.Current.User.Identity.Name;
                    jo["Datas"] = ds.RepairWorkList(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }

    public class RepairWorkFillinController : ApiController
    {
        public JObject Post()
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
                    var form = HttpContext.Current.Request.Form;

                    // 建立模型並綁定表單資料
                    var item = new Repair_ManagementRepairFillinViewModel
                    {
                        RSN = form["RSN"],
                        RepairContent = form["RepairContent"],
                    };
                    if (HttpContext.Current.Request.Files.Count > 0)
                        item.RepairImg = HttpContext.Current.Request.Files[0];
                    ds.RepairWorkFillin(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion


    #region 保養借放(設備保養紀錄)
    public class MaintenanceRecordController : ApiController
    {
        public JObject Post([FromBody] MaitenanceRecordViewModel item)
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
                    jo["Datas"] = ds.MaintenanceRecord(item);
                }
            }
            catch (Exception ex)
            {
                jo["State"] = "Failed";
                jo["ErrorMessage"] = ex.Message;
            }
            return jo;
        }
    }
    #endregion
}