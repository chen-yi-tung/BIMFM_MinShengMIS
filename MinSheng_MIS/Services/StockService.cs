using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Linq.Dynamic.Core;
using System.Data.Entity.Migrations;
using MinSheng_MIS.Surfaces;

namespace MinSheng_MIS.Services
{
    public class StockService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public StockService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 新增庫存
        public JsonResService<string> Stock_Create(ComputationalStockCreateModel datas)
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            JObject jo_res = new JObject();
            #endregion

            try
            {
                #region 資料檢查
                string ErrorMessage = ComputationalStockAnnotation(datas);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = ErrorMessage;
                    return res;
                }
                #endregion

                #region 資料新增
                var data = new ComputationalStock();
                var lastSISN = _db.ComputationalStock.OrderByDescending(x => x.SISN).FirstOrDefault()?.SISN ?? "0000000";
                data.SISN = ComFunc.CreateNextID("%{7}", lastSISN);
                data.StockTypeSN = datas.StockTypeSN;
                data.StockName = datas.StockName;
                data.Unit = datas.Unit;
                data.StockAmount = 0;
                data.MinStockAmount = datas.MinStockAmount;
                if (data.StockAmount < data.MinStockAmount)
                    data.StockStatus = "2";
                else
                    data.StockStatus = "1";
                _db.ComputationalStock.AddOrUpdate(data);
                _db.SaveChanges();
                #endregion
                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion

        #region ComputationalStock資料驗證
        private string ComputationalStockAnnotation(IComputationalStock data)
        {
            string ErrorMessage = "";
            //庫存類別編號
            if (string.IsNullOrEmpty(data.StockTypeSN.ToString()))
            {
                ErrorMessage = "庫存類別不可為空";
                return ErrorMessage;
            }
            else if (_db.StockType.Find(data.StockTypeSN) == null)
            {
                ErrorMessage = "無此庫存類別";
                return ErrorMessage;
            }
            //庫存品項
            if (string.IsNullOrEmpty(data.StockName))
            {
                ErrorMessage = "庫存品項不可為空";
                return ErrorMessage;
            }
            else if(data.StockName.Length > 50)
            {
                ErrorMessage = "庫存品項需介於1 ~ 50字之間！";
                return ErrorMessage;
            }
            //單位
            if (string.IsNullOrEmpty(data.Unit))
            {
                ErrorMessage = "單位不可為空";
                return ErrorMessage;
            }
            else if (data.Unit.Length > 10)
            {
                ErrorMessage = "單位需介於1 ~ 10字之間！";
                return ErrorMessage;
            }
            //庫存警戒值
            if (string.IsNullOrEmpty(data.MinStockAmount.ToString()))
            {
                ErrorMessage = "庫存警戒值不可為空";
                return ErrorMessage;
            }
            return ErrorMessage;
        }
        #endregion

        #region 庫存詳情
        public JsonResService<ComputationalStockDetailModel> Stock_Details(string sisn)
        {
            #region 變數
            JsonResService<ComputationalStockDetailModel> res = new JsonResService<ComputationalStockDetailModel>();
            JObject jo_res = new JObject();
            var dic_stocktype = Surface.StockStatus();
            #endregion

            try
            {
                #region 資料檢查
                var data = _db.ComputationalStock.Find(sisn);
                if(data == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此庫存品項";
                    return res;
                }
                #endregion

                #region 資料
                ComputationalStockDetailModel datas = new ComputationalStockDetailModel();
                datas.StockType = _db.StockType.Find(data.StockTypeSN).StockTypeName.ToString();
                datas.StockName = data.StockName;
                datas.StockStauts = dic_stocktype[data.StockStatus];
                datas.StockAmount = (float)data.StockAmount;
                datas.Unit = data.Unit;
                datas.MinStockAmount = (float)data.MinStockAmount;
                res.Datas = datas;
                #endregion

                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion

        #region 新增一般入庫
        public JsonResService<string> NormalStockIn_Create(NomalComputationalStockInModel datas,string sarsn,string registrar,string filename)
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            JObject jo_res = new JObject();
            #endregion

            try
            {
                #region 資料檢查
                string ErrorMessage = NomalStockInRecordAnnotation(datas);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = ErrorMessage;
                    return res;
                }
                #endregion

                #region 資料新增
                //變更計算型庫存數量
                var stock = _db.ComputationalStock.Find(datas.SISN);
                stock.StockAmount += datas.NumberOfChanges;
                if (stock.StockAmount < stock.MinStockAmount)
                    stock.StockStatus = "2";//低於警戒值
                else
                    stock.StockStatus = "1";//充足
                _db.ComputationalStock.AddOrUpdate(stock);
                _db.SaveChanges();

                //新增入庫紀錄
                var data = new StockChangesRecord();
                data.SARSN = sarsn;
                data.SISN = datas.SISN;
                data.ChangeType = "2";//入庫
                data.ChangeWay = "1";//一般
                data.NumberOfChanges = (double)datas.NumberOfChanges;
                data.CurrentInventory = stock.StockAmount;
                data.Registrar = registrar;
                data.ChangeTime = DateTime.Now;
                data.PurchaseOrder = filename;
                data.Memo = datas.Memo;
                _db.StockChangesRecord.AddOrUpdate(data);
                _db.SaveChanges();
                #endregion
                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion

        #region NomalStockInRecord資料驗證
        private string NomalStockInRecordAnnotation(INomalComputationalStockIn data)
        {
            string ErrorMessage = "";
            //庫存項目編號
            if (string.IsNullOrEmpty(data.SISN.ToString()))
            {
                ErrorMessage = "庫存項目不可為空";
                return ErrorMessage;
            }
            else if (_db.ComputationalStock.Find(data.SISN) == null)
            {
                ErrorMessage = "無此庫存項目";
                return ErrorMessage;
            }
            //入庫數量
            if (string.IsNullOrEmpty(data.NumberOfChanges.ToString()))
            {
                ErrorMessage = "入庫數量不可為空";
                return ErrorMessage;
            }
            else
            {
                if (data.NumberOfChanges <= 0)
                {
                    ErrorMessage = "入庫數量不可小於0";
                    return ErrorMessage;
                }
            }
            //備註
            if (!string.IsNullOrEmpty(data.Memo))
            {
                if (data.Memo.Length > 250)
                {
                    ErrorMessage = "備註需介於0 ~ 250字之間！";
                    return ErrorMessage;
                }
            }       
            return ErrorMessage;
        }
        #endregion
        #region 新增一般出庫
        public JsonResService<string> NormalStockOut_Create(NomalComputationalStockOutModel datas, string registrar)
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            JObject jo_res = new JObject();
            #endregion

            try
            {
                #region 資料檢查
                string ErrorMessage = NomalStockOutRecordAnnotation(datas);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = ErrorMessage;
                    return res;
                }
                #endregion

                #region 資料新增
                //變更計算型庫存數量
                var stock = _db.ComputationalStock.Find(datas.SISN);
                stock.StockAmount -= datas.NumberOfChanges;
                if (stock.StockAmount < stock.MinStockAmount)
                    stock.StockStatus = "2";//低於警戒值
                else
                    stock.StockStatus = "1";//充足
                _db.ComputationalStock.AddOrUpdate(stock);
                _db.SaveChanges();

                //新增出庫紀錄
                var data = new StockChangesRecord();
                var lastSARSN = _db.StockChangesRecord.OrderByDescending(x => x.SARSN).FirstOrDefault()?.SARSN ?? (DateTime.Today.ToString("yyyyMMddHHmm") + "000");
                data.SARSN = ComFunc.CreateNextID("!{yyMMddHHmm}%{3}", lastSARSN);
                data.SISN = datas.SISN;
                data.ChangeType = "1";//出庫
                data.ChangeWay = "1";//一般
                data.NumberOfChanges = (double)datas.NumberOfChanges;
                data.CurrentInventory = stock.StockAmount;
                data.Registrar = registrar;
                data.ChangeTime = DateTime.Now;
                data.Recipient = datas.Recipient;
                data.Memo = datas.Memo;
                _db.StockChangesRecord.AddOrUpdate(data);
                _db.SaveChanges();
                #endregion
                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion
        #region NomalStockOutRecord資料驗證
        private string NomalStockOutRecordAnnotation(INomalComputationalStockOut data)
        {
            string ErrorMessage = "";
            //庫存項目編號
            if (string.IsNullOrEmpty(data.SISN.ToString()))
            {
                ErrorMessage = "庫存項目不可為空";
                return ErrorMessage;
            }
            else if (_db.ComputationalStock.Find(data.SISN) == null)
            {
                ErrorMessage = "無此庫存項目";
                return ErrorMessage;
            }
            //出庫數量
            if (string.IsNullOrEmpty(data.NumberOfChanges.ToString()))
            {
                ErrorMessage = "出庫數量不可為空";
                return ErrorMessage;
            }
            else //可出庫數量 = 當前總數量 - 當前rfid庫存數量
            {
                var currrentRFIDstocks = _db.StockChangesRecord.Where(x => x.SISN == data.SISN).Select(x => x.SARSN).ToList();
                var enablestockout = _db.ComputationalStock.Find(data.SISN).StockAmount - _db.RFID.Where(x => currrentRFIDstocks.Contains(x.SARSN)).Count();
                if(data.NumberOfChanges > enablestockout)
                {
                    ErrorMessage = "出庫失敗，當前可出庫數量為:"+ enablestockout+"。請重新確認出庫數量！";
                    return ErrorMessage;
                }
            }
            //備註
            if (!string.IsNullOrEmpty(data.Memo))
            {
                if (data.Memo.Length > 250)
                {
                    ErrorMessage = "備註需介於0 ~ 250字之間！";
                    return ErrorMessage;
                }
            }
            return ErrorMessage;
        }
        #endregion
    }
}