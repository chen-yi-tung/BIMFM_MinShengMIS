using System.ComponentModel.DataAnnotations;

public class RFIDInternalCodesValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var codes = value as string[];
        if (codes == null || codes.Length == 0)
        {
            return new ValidationResult("至少需掃描一個RFID");
        }

        if (codes.Length > 100)
        {
            return new ValidationResult("一次入庫不可掃描超過100個RFID");
        }

        foreach (var code in codes)
        {
            if (code.Length > 150)
            {
                return new ValidationResult("超過150碼之RFID內碼不存在");
            }
        }

        return ValidationResult.Success;
    }
}