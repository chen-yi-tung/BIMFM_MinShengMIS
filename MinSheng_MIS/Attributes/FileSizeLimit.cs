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
            if (value is HttpPostedFileBase file && file.ContentLength > _maxSize)
                return new ValidationResult($"檔案大小不得超過 {_maxSize} MB");

            return ValidationResult.Success;
        }
    }
}