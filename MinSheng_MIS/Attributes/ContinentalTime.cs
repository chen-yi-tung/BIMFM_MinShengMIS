using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Attributes
{
    public class ContinentalTime : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success; // 可根據需求允許空值
            }

            var timeString = value.ToString();

            // 驗證格式是否為有效的 "HH:mm" 並自動檢查 24 小時制範圍
            if (!TimeSpan.TryParseExact(timeString, "hh\\:mm", CultureInfo.InvariantCulture, out _))
            {
                // 使用 Display Name（若未設定，則使用屬性名稱）
                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                return new ValidationResult($"{displayName} 格式非24小時制！");
            }

            return ValidationResult.Success;
        }
    }
}