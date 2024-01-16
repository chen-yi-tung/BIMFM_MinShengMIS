using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class StockOut_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: StockOut_Management
        #region 出庫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增出庫
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateStockOut(SO_Info so_info)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            var request = await db.StoresRequisition.FirstOrDefaultAsync(x => x.SRSN == so_info.SRSN);
            if (request == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SRSN is Undefined.");
            else if (request.SRState == "1") return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot Stock Out!");
            #region 檢查出庫項目及數量是否符合領用申請
            IEnumerable<SO_Item> unmatchedItems = null, unmatchedAmount = null;
            string errorText = string.Empty;
            var SR_ItemsDics = request.StoresRequisitionItem?.Where(x => x.PickUpStatus == "3" || x.PickUpStatus == "4")?.GroupBy(x => x.SISN).ToDictionary(k => k.Key, v => v.Sum(a => a.Amount - a.TakeAmount));
            if (SR_ItemsDics == null) return Content($"<br>此領用單的項目皆已出庫!", "application/json; charset=utf-8");
            var SO_ItemsInSISNDics = so_info.StockOutItem?.GroupBy(x => x.SISN).ToDictionary(k => k.Key, v => v.Sum(a => a.OutAmount));
            if (SO_ItemsInSISNDics == null) return Content($"<br>庫存出庫項目至少一項!", "application/json; charset=utf-8");
            // 篩選非申請之庫存項目
            unmatchedItems = so_info.StockOutItem.Where(x => !SR_ItemsDics.ContainsKey(x.SISN));
            // 篩選數量不符之庫存項目
            if (unmatchedItems.Any()) unmatchedAmount = so_info.StockOutItem.Except(unmatchedItems ?? Enumerable.Empty<SO_Item>()).Where(x => SO_ItemsInSISNDics[x.SISN] > SR_ItemsDics[x.SISN]);
            else unmatchedAmount = so_info.StockOutItem.Where(x => SO_ItemsInSISNDics[x.SISN] > SR_ItemsDics[x.SISN]);
            // 有不符合的出庫項目
            if (unmatchedItems.Any() || unmatchedAmount.Any()) 
            {
                errorText += Helper.HandleErrorMessageList(GetUnmatchedTypeAndName(unmatchedItems), "未申請領用的庫存項目：");
                errorText += Helper.HandleErrorMessageList(GetUnmatchedTypeAndName(unmatchedAmount), "出庫數量超過可領取數量的庫存項目：");
                return Content(errorText, "application/json; charset=utf-8");
            }
            #endregion

            DateTime now = DateTime.Now;
            // 新增庫存出庫紀錄
            var r_count = await db.StockOutRecord.Where(x => DbFunctions.TruncateTime(x.StockOutDateTime) == now.Date).CountAsync() + 1;  // 庫存出庫紀錄流水碼
            var record = new StockOutRecord
            {
                SORSN = "O" + now.ToString("yyMMdd") + r_count.ToString().PadLeft(3, '0'),
                SRSN = so_info.SRSN,
                StockOutUserName = so_info.StockOutUserName,
                ReceiverUserName = so_info.ReceiverUserName,
                StockOutContent = so_info.StockOutContent,
                StockOutDateTime = now
            };
            db.StockOutRecord.Add(record);

            //// 計算型庫存變動的清單
            //var computationalStock = await db.ComputationalStock.Where(x => SO_ItemsInSISNDics.ContainsKey(x.SISN)).ToListAsync();
            //computationalStock.ForEach(x => x.StockAmount -= SO_ItemsInSISNDics[x.SISN]);
            // 新增庫存出庫項目及更新庫存數量
            var s_count = 1;  // 庫存出庫項目流水碼
            var groupedItems = so_info.StockOutItem.GroupBy(x => x.SSN).Select(g => new { SSN = g.Key, Amount = g.Sum(x => x.OutAmount) }); // 將同個物件掃多次RFID進行出庫的統整為單筆
            foreach (var item in groupedItems)
            {
                var obj = new StockOutItem
                {
                    SOISN = record.SORSN + s_count.ToString().PadLeft(3, '0'),
                    SORSN = record.SORSN,
                    SSN = item.SSN,
                    Amount = item.Amount
                };
                db.StockOutItem.Add(obj);
                var stock = await db.Stock.FindAsync(item.SSN);
                if (stock == null) return Content($"<br>RFID：{item.SSN} 不存在!", "application/json; charset=utf-8");
                else if (stock.RemainingAmount < item.Amount) return Content($"<br>RFID：{item.SSN} 數量不足!", "application/json; charset=utf-8");
                stock.RemainingAmount -= item.Amount;
                stock.ComputationalStock.StockAmount -= item.Amount;
                db.Stock.AddOrUpdate(stock);
                s_count++;
            }
            var updateItems = request.StoresRequisitionItem.Where(x => SO_ItemsInSISNDics.ContainsKey(x.SISN)).ToList();
            updateItems.ForEach(x =>
            {
                x.TakeAmount += SO_ItemsInSISNDics[x.SISN];
                if (x.TakeAmount < x.Amount) x.PickUpStatus = "4";
                else x.PickUpStatus = "5";
                db.StoresRequisitionItem.AddOrUpdate(x);
            });
            //// 更新計算型庫存數量
            //foreach (var item in computationalStock)
            //    db.ComputationalStock.AddOrUpdate(item);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Content("Succeed");
        }
        #endregion

        #region 出庫詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Read_Data(string id)
        {
            var record = await db.StockOutRecord.FirstOrDefaultAsync(x => x.SORSN == id);
            if (record == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SORSN is Undefined.");

            var UserDics = await db.AspNetUsers.Where(x => x.IsEnabled == true).ToDictionaryAsync(k => k.UserName, v => v.MyName);
            var TypeDics = Surface.StockType();
            var UnitDics = Surface.Unit();
            SO_ViewModel model = new SO_ViewModel
            {
                SORSN = record.SORSN,
                SRSN = record.SRSN,
                StockOutDateTime = record.StockOutDateTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                StockOutMyName = UserDics[record.StockOutUserName],
                ReceiverMyName = UserDics[record.ReceiverUserName],
                StockOutContent = record.StockOutContent,
                StockOutItem = record.StockOutItem.Select(x => new SO_Item_ViewModel
                {
                    SSN = x.SSN,
                    StockType = x.Stock.ComputationalStock != null ? TypeDics[x.Stock.ComputationalStock.StockType] : null,
                    StockName = x.Stock.ComputationalStock?.StockName,
                    MName = x.Stock.StockInRecord.MName,
                    Size = x.Stock.StockInRecord.Size,
                    Brand = x.Stock.StockInRecord.Brand,
                    Model = x.Stock.StockInRecord.Model,
                    OutAmount = x.Amount,
                    Unit = x.Stock.ComputationalStock != null ? UnitDics[x.Stock.ComputationalStock.Unit] : null
                }).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region 取得領取申請單資訊
        public ActionResult GetRequisitionInfo(string id)
        {
            //var stockInfo = db.ComputationalStock.Where(x => x.SISN == SISN).Select(x => new { x.StockType, x.StockName, x.Unit }).FirstOrDefault();
            //return Content(JsonConvert.SerializeObject(stockInfo), "application/json");
            return null;
        }
        #endregion

        #region 取得RFID資訊
        public ActionResult GetRFIDInfo(string id)
        {
            //var stockInfo = db.ComputationalStock.Where(x => x.SISN == SISN).Select(x => new { x.StockType, x.StockName, x.Unit }).FirstOrDefault();
            //return Content(JsonConvert.SerializeObject(stockInfo), "application/json");
            return null;
        }
        #endregion

        #region Helper
        private List<string> GetUnmatchedTypeAndName(IEnumerable<SO_Item> list)
        {
            var query = db.ComputationalStock.AsQueryable();
            List<string> result = new List<string>();
            foreach (var item in list)
            {
                var stock = query.Where(x => x.SISN == item.SISN).FirstOrDefault();
                if (stock != null)
                    result.Add($"{item.SSN} {stock.StockType} {stock.StockName}");
            }
            return result;
        }
        #endregion
    }
}