using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models
{
    public class MyCusResException : Exception
    {
        // 自定義異常的構造函數，可添加額外的參數
        public MyCusResException() : base("MyCustomException occurred")
        {
            // 初始化操作
        }

        // 自定義異常的構造函數，接受一個錯誤消息
        public MyCusResException(string message) : base(message)
        {
            // 初始化操作
        }

        // 自定義異常的構造函數，接受一個錯誤消息和一個內部異常
        public MyCusResException(string message, Exception innerException) : base(message, innerException)
        {
            // 初始化操作
        }

        // 添加自定義的屬性或方法，以提供更多信息
        //public string CustomProperty { get; set; }

        // 自定義方法
        //public void CustomMethod()
        //{
        //    // 執行自定義的操作
        //}
    }
}