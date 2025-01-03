using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace MinSheng_MIS.Controllers
{
    public class StockRFID_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public StockRFID_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
        }

        //RFID入庫
        //POST: StockRFID_Management/CreateRFIDStockIn
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRFIDStockIn(SIRFID_ViewModel StockItem)
        {
            JsonResService<string> result = new JsonResService<string>();

            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    #region 檢查是否有重複 RFIDInternalCode 或 RFIDExternalCode 並確認是否有此SISN品項名稱 

                    // 檢查 RFIDInternalCode 是否已經存在
                    var StockInRFID = _db.RFID.Where(x => x.RFIDInternalCode == StockItem.RFIDInternalCode).FirstOrDefault();
                    if (StockInRFID != null)
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = $"已有此RFID内碼，不可重複入庫";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }

                    // 檢查 RFIDExternalCode 是否重複
                    var checkRFIDExternalCode = _db.RFID.Where(x => x.RFIDExternalCode == StockItem.RFIDExternalCode).FirstOrDefault();
                    if (checkRFIDExternalCode != null)
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = $"已有此RFID外碼，請重新編碼";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }

                    // 用SISN查詢庫存記錄
                    var InStock = _db.ComputationalStock.Where(c => c.SISN == StockItem.StockName).FirstOrDefault();
                    if (InStock == null)
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = $"查無此庫存記錄";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }

                    // 檢查PurchaseOrder是否超過一個檔案
                    if (StockItem.PurchaseOrder != null && StockItem.PurchaseOrder.Length > 1)
                    {
                            result.AccessState = ResState.Failed;
                            result.ErrorMessage = $"只能上傳一個檔案";
                            return Content(JsonConvert.SerializeObject(result), "application/json");

                    }

                    // 儲存檔案
                    string purchaseOrderFileName = null;
                    if (StockItem.PurchaseOrder != null && StockItem.PurchaseOrder.Length == 1)
                    {
                        purchaseOrderFileName = await SavePurchaseOrderFileAsync(StockItem.PurchaseOrder[0]);
                    }
                    #endregion

                    #region 新增一筆 ComputationalStock
                    // 通過前述檢查，新增這個SISN的庫存
                    InStock.StockAmount += 1;
                    InStock.StockStatus = InStock.StockAmount < InStock.MinStockAmount ? "2" : "1";
                    _db.ComputationalStock.AddOrUpdate(InStock);
                    await _db.SaveChangesAsync();
                    #endregion

                    #region 新增一筆 StockChangesRecord
                    var newCount = 1;

                    var stockChangesRecord = new StockChangesRecord
                    {
                        SARSN = await GenerateNewSARSN(),
                        SISN = StockItem.StockName,
                        ChangeType = "2", // 入庫
                        ChangeWay = "2", // RFID
                        NumberOfChanges = 1,
                        CurrentInventory = InStock.StockAmount,
                        Registrar = User.Identity.Name,
                        ChangeTime = DateTime.Now,
                        PurchaseOrder = purchaseOrderFileName
                    };

                    _db.StockChangesRecord.Add(stockChangesRecord);
                    await _db.SaveChangesAsync();
                    #endregion

                    #region 新增一筆 RFID   
                    _db.RFID.Add(new RFID
                    {
                        RFIDInternalCode = StockItem.RFIDInternalCode,
                        RFIDExternalCode = StockItem.RFIDExternalCode,
                        SARSN = stockChangesRecord.SARSN
                    });

                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    #endregion

                    result.AccessState = ResState.Success;
                    result.ErrorMessage = "";
                    result.Datas = "新增入庫成功";
                    return Content(JsonConvert.SerializeObject(result), "application/json");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.AccessState = ResState.Failed;
                    result.ErrorMessage = $"操作失敗: {ex.Message}";
                    return Content(JsonConvert.SerializeObject(result), "application/json");
                }
            }
        }


        //RFID出庫品項細節
        [HttpGet]
        public ActionResult StockOutDetail(string RFIDInternalCode)
        {
            JsonResService<JObject> result = new JsonResService<JObject>();
            JObject jo_item = new JObject();

            #region 檢查RFIDInternalCode 長度 是否提供
            if (string.IsNullOrEmpty(RFIDInternalCode))
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"請提供RFID內碼";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }

            if (RFIDInternalCode.Length > 24)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"RFID內碼應不超過24碼";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            #endregion

            #region RFIDInternalCodes檢查是否存在、是否重複
            var rfidDetail = _db.RFID.Where(x => x.RFIDInternalCode == RFIDInternalCode).FirstOrDefault();
            if (rfidDetail == null)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"RFID內碼 {RFIDInternalCode} 不存在";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            #endregion

            #region 塞入細節資料
            var scrDetail = _db.StockChangesRecord.Where(x => x.SARSN == rfidDetail.SARSN).FirstOrDefault();
            var csDetail = _db.ComputationalStock.Where(x => x.SISN == scrDetail.SISN).FirstOrDefault();
            var stockTSN = _db.StockType.Where(x => x.StockTypeSN == csDetail.StockTypeSN).FirstOrDefault();

            jo_item.Add("RFIDInternalCode", rfidDetail.RFIDInternalCode);
            jo_item.Add("RFIDExternalCode", rfidDetail.RFIDExternalCode);
            jo_item.Add("StockType", stockTSN.StockTypeName);
            jo_item.Add("StockName", csDetail.StockName);
            jo_item.Add("Unit", csDetail.Unit);
            #endregion

            result.Datas = jo_item;
            result.AccessState = ResState.Success;
            result.ErrorMessage = "";
            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        //RFID出庫
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> RFIDStockOut(SORFID_ViewModel soViewModel)
        {
            JsonResService<string> result = new JsonResService<string>();

            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    #region 檢查是否每個 RFIDInternalCodes 都有相關記錄、是否有重複
                    var seenCodes = new HashSet<string>(); // HashSet無法儲存重複值
                    var ErrorMessageList = new List<string>();

                    foreach (var code in soViewModel.RFIDInternalCodes)
                    {
                        var StockInRFID = await _db.RFID.Where(x => x.RFIDInternalCode == code).FirstOrDefaultAsync();
                        if (StockInRFID == null)
                        {
                            ErrorMessageList.Add($"RFID内碼 {code} 不存在");
                        }

                        if (!seenCodes.Add(code))
                        {
                            ErrorMessageList.Add($"RFID内碼 {code} 重複");
                        }

                        if (ErrorMessageList.Count != 0)
                        {
                            result.AccessState = ResState.Failed;
                            result.ErrorMessage = string.Join(",", ErrorMessageList);
                            return Content(JsonConvert.SerializeObject(result), "application/json");
                        }
                    }
                    #endregion

                    #region db.RFID 找出掃描到的RFIDInternalCodes的RFID資料
                    var RFIDs = new List<RFID>();

                    foreach (var code in soViewModel.RFIDInternalCodes)
                    {
                        var rfid = await _db.RFID.FirstOrDefaultAsync(x => x.RFIDInternalCode == code);
                        if (rfid != null)
                        {
                            RFIDs.Add(rfid);
                        }
                    }
                    #endregion

                    #region 找出RFIDInternalCodes的SARSNs
                    var SARSNs = RFIDs.Select(scr => scr.SARSN).ToList();
                    var stockChangesRecords = await _db.StockChangesRecord
                        .Where(scr => SARSNs.Contains(scr.SARSN))
                        .ToListAsync();
                    #endregion

                    #region 將找出的SARSNs的SISN做分組（每個SISN有Count個），並找出stockChangesRecords
                    var sisnCounts = stockChangesRecords
                        .GroupBy(scr => scr.SISN)
                        .Select(group => new SISNCount
                        {
                            SISN = group.Key,
                            Count = group.Count()
                        })
                        .ToList();
                    #endregion

                    #region 從SISN找出ComputationalStock
                    var sisnValues = sisnCounts.Select(sc => sc.SISN).ToList();
                    var computationalStocks = await _db.ComputationalStock
                        .Where(cs => sisnValues.Contains(cs.SISN))
                        .ToListAsync();

                    if (sisnCounts.Count == 0 || computationalStocks.Count == 0)
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = "找不到相關資料";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }
                    #endregion

                    #region 更新ComputationalStock、新增StockChangesRecord
                    var newSARSNs = await GenerateNewSARSNs(sisnCounts.Count);

                    for (int i = 0; i < sisnCounts.Count; i++)
                    {
                        var sisnCount = sisnCounts[i];
                        var computationalStock = computationalStocks.FirstOrDefault(cs => cs.SISN == sisnCount.SISN);
                        if (computationalStock != null)
                        {
                            var stockChangesRecord = new StockChangesRecord
                            {
                                SARSN = newSARSNs[i],
                                SISN = sisnCount.SISN,
                                ChangeType = "1", // 出庫
                                ChangeWay = "2", // RFID
                                NumberOfChanges = sisnCount.Count,
                                CurrentInventory = computationalStock.StockAmount - sisnCount.Count,
                                Registrar = User.Identity.Name,
                                ChangeTime = DateTime.Now,
                                Recipient = soViewModel.Recipient
                            };

                            _db.StockChangesRecord.Add(stockChangesRecord);
                        }
                    }
                    
                    foreach (var cs in computationalStocks)
                    {
                        cs.StockAmount -= sisnCounts.Where(sc => sc.SISN == cs.SISN).Select(sc => sc.Count).FirstOrDefault();
                        if (cs.StockAmount < 0)
                        {
                            transaction.Rollback();
                            var StockName = cs.StockName;
                            result.AccessState = ResState.Failed;
                            result.ErrorMessage = $"操作失敗: {StockName}庫存量不足";
                            return Content(JsonConvert.SerializeObject(result), "application/json");
                        }
                        cs.StockStatus = cs.StockAmount < cs.MinStockAmount ? "2" : "1";
                        _db.ComputationalStock.AddOrUpdate(cs);
                    }
                    #endregion

                    #region 刪除RFID
                    foreach (var code in soViewModel.RFIDInternalCodes)
                    {
                        var StockInRFID = await _db.RFID.Where(x => x.RFIDInternalCode == code).FirstOrDefaultAsync();
                        if (StockInRFID != null)
                        {
                            _db.RFID.Remove(StockInRFID);
                        }
                    }
                    #endregion

                    #region DB SaveChanges try-catch
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        var errorMessages = dbEx.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);
                        var fullErrorMessage = string.Join("; ", errorMessages);
                        var exceptionMessage = string.Concat(dbEx.Message, " The validation errors are: ", fullErrorMessage);

                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = $"操作失敗: {exceptionMessage}";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }
                    catch (DbUpdateException dbUpEx)
                    {
                        var innerExceptionMessage = dbUpEx.InnerException?.InnerException?.Message ?? dbUpEx.InnerException?.Message ?? dbUpEx.Message;
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = $"操作失敗: {innerExceptionMessage}";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }
                    catch (Exception ex)
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = $"操作失敗: {ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : string.Empty)}";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }
                    #endregion

                    transaction.Commit();
                    result.AccessState = ResState.Success;
                    result.ErrorMessage = "";
                    result.Datas = "出庫成功";
                    return Content(JsonConvert.SerializeObject(result), "application/json");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.AccessState = ResState.Failed;
                    result.ErrorMessage = $"操作失敗: {ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : string.Empty)}";
                    return Content(JsonConvert.SerializeObject(result), "application/json");
                }
            }
        }



        private async Task<List<string>> GenerateNewSARSNs(int count)
        {
            string currentDateFormatted = DateTime.Now.ToString("yyMMddHHmm");
            List<string> newSARSNs = new List<string>();
            int retryCount = 0;
            const int maxRetries = 5;

            for (int i = 0; i < count; i++)
            {
                string newSARSN;
                do
                {
                    var lastSARSN = await _db.StockChangesRecord
                        .Where(x => x.SARSN.StartsWith(currentDateFormatted))
                        .OrderByDescending(x => x.SARSN)
                        .Select(x => x.SARSN)
                        .FirstOrDefaultAsync();

                    newSARSN = lastSARSN != null
                        ? currentDateFormatted + (int.Parse(lastSARSN.Substring(10)) + 1 + i).ToString("D3")
                        : currentDateFormatted + (i + 1).ToString("D3");

                    retryCount++;
                } while (await _db.StockChangesRecord.AnyAsync(x => x.SARSN == newSARSN) && retryCount < maxRetries);

                if (retryCount == maxRetries)
                {
                    throw new InvalidOperationException("Failed to generate a unique SARSN after multiple attempts.");
                }

                newSARSNs.Add(newSARSN);
            }

            return newSARSNs;
        }

        private async Task<string> GenerateNewSARSN()
        {
            var sarsns = await GenerateNewSARSNs(1);
            return sarsns.First();
        }

        private async Task<string> SavePurchaseOrderFileAsync(HttpPostedFileBase purchaseOrder)
        {
            if (purchaseOrder == null) return string.Empty;

            string extension = Path.GetExtension(purchaseOrder.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
            {
                throw new InvalidOperationException("圖片僅接受jpg、jpeg、png、pdf！");
            }

            string folderPath = Server.MapPath("~/Files/PurchaseOrder");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filename = await GenerateNewSARSN() + extension;
            string fileFullPath = Path.Combine(folderPath, filename);
            purchaseOrder.SaveAs(fileFullPath);

            return filename;
        }
    }
}