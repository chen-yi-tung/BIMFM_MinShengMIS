using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ComputationalStockCreateModel: IComputationalStock
    {
        [Required]
        public int StockTypeSN { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        public string StockName { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        public string Unit { get; set; }
        public float MinStockAmount { get; set; }
    }
    public class ComputationalStockDetailModel 
    {
        public string StockType { get; set; } //類別
        public string StockName { get; set; } //品項名稱
        public string StockStauts { get; set; } //狀態
        public float StockAmount { get; set; } //數量
        public string Unit { get; set; } //單位
        public float MinStockAmount { get; set; } //警戒值
    }
    //-----Interface & Abstract class
    #region ComputationalStock 計算型庫存
    public interface IComputationalStock
    {
        int StockTypeSN { get; set; }
        string StockName { get; set; }
        string Unit { get; set; }
        float MinStockAmount { get; set; }
    }
    #endregion
    #region ComputationalStockDatail 計算型庫存詳情
    public interface IComputationalStockDatail
    {
        int StockType { get; set; } //類別
        string StockName { get; set; } //品項名稱
        string StockStauts { get; set; } //狀態
        float StockAmount { get; set; } //數量
        string Unit { get; set; } //單位
        float MinStockAmount { get; set; } //警戒值
    }
    #endregion
    //public class Stock_ViewModel
    //{
    //    public string SISN { get; set; }
    //    public string StockType { get; set; }
    //    public string StockName { get; set; }
    //    public double StockAmount { get; set; }
    //    public double AvailableStockAmount { get; set; }
    //    public string Unit { get; set; }
    //    public double MinStockAmount { get; set; }
    //    public string ExpiryDate { get; set; }
    //    public List<StockItem> StockItem { get; set; } = new List<StockItem>();
    //}

    //public class StockItem
    //{
    //    public string SSN { get; set; }
    //    public string Brand { get; set; }
    //    public string Model { get; set; }
    //    public string StockInDateTime { get; set; }
    //    public string ExpiryDate { get; set; }
    //    public string Location { get; set; }
    //    public string SIRSN { get; set; }
    //    public double RemainingAmount { get; set; }
    //    public string StockInMyName { get; set; }
    //}

    //public class SetWarning
    //{
    //    [Required]
    //    public string SISN { get; set; }
    //    [Required]
    //    public DateTime? ExpiryDate { get; set; }
    //    [Required]
    //    public double MinStockAmount { get; set; }
    //}
}