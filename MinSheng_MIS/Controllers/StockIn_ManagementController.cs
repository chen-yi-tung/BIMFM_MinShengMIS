using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class StockIn_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: StockIn_Management
        #region 入庫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增入庫
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateStockIn(List<SI_Info> si_info)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            DateTime now = DateTime.Now;
            ComputationalStock stock = null;
            var universalInfo = si_info.FirstOrDefault(); // 庫存(Stock)以外所需的data

            // 檢查庫存品項是否已存在(以StockType/StockName/Unit檢查)
            var computationalItem = await db.ComputationalStock.Where(x => x.StockType == universalInfo.StockType && x.StockName == universalInfo.StockName && x.Unit == universalInfo.Unit).FirstOrDefaultAsync();
            if (computationalItem == null) // 表示需新增計算型庫存品項
            {
                var c_count = await db.ComputationalStock.Where(x => x.StockType == universalInfo.StockType).CountAsync() + 1;  // 計算型庫存同庫存種類流水碼
                stock = new ComputationalStock
                {
                    SISN = universalInfo.StockType + c_count.ToString().PadLeft(3, '0'),
                    StockType = universalInfo.StockType,
                    StockName = universalInfo.StockName,
                    Unit = universalInfo.Unit,
                    StockAmount = si_info.Select(x => x.Amount).Sum(),
                    MinStockAmount = universalInfo.MinStockAmount,
                    ExpiryDate = si_info.OrderByDescending(x => x.ExpiryDate).Select(x => x.ExpiryDate).Distinct().FirstOrDefault()
                };
                db.ComputationalStock.Add(stock);
            }
            else // 表示需計算品項庫存量
            {
                computationalItem.StockAmount += si_info.Select(x => x.Amount).Sum();
                db.ComputationalStock.AddOrUpdate(stock);
            }

            // 新增庫存入庫紀錄
            var r_count = await db.StockInRecord.Where(x => DbFunctions.TruncateTime(x.StockInDateTime) == now.Date).CountAsync() + 1;  // 庫存入庫紀錄流水碼
            var record = new StockInRecord
            {
                SIRSN = "SI" + now.ToString("yyMMdd") + r_count.ToString().PadLeft(3, '0'),
                MName = universalInfo.MName,
                Brand = universalInfo.Brand,
                Model = universalInfo.Model,
                Size = universalInfo.Size,
                StockInDateTime = now,
                StockInUserName = User.Identity.Name
            };
            db.StockInRecord.Add(record);

            // 新增庫存
            var s_count = await db.Stock.Where(x => x.SISN == computationalItem.SISN).CountAsync() + 1;  // 庫存同庫存項目(計算型庫存)流水碼
            foreach (var item in si_info)
            {
                var obj = new Stock
                {
                    SSN = stock.SISN + s_count.ToString().PadLeft(4, '0'),
                    SIRSN = record.SIRSN,
                    SISN = stock.SISN,
                    Location = item.Location,
                    ExpiryDate = item.ExpiryDate,
                    Amount = item.Amount,
                    RemainingAmount = item.Amount,
                    RFIDInternalCode = null
                };
                db.Stock.Add(obj);
            }

            return Content("Succeed");
        }
        #endregion

        #region 入庫詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}