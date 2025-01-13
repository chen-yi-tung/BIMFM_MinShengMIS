using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Attributes
{
    public class FileSizeLimit : ValidationAttribute
    {
        private readonly int _maxSize;

        public FileSizeLimit(int maxSize)
        {
            _maxSize = maxSize* 1024 * 1024;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is HttpPostedFileBase file && file?.ContentLength > _maxSize)
            {
                // 使用 Display Name（若未設定，則使用屬性名稱）
                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                return new ValidationResult($"{displayName} 檔案大小超過 {_maxSize} MB！");
            }

            return ValidationResult.Success;
        }
    }
}