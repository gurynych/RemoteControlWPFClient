using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RemoteControlWPFClient.MVVM.Validation
{
    public class LoginValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value.ToString().Length < 3)
            {
                return new ValidationResult(false, "Логин должен содержать больше 3 символов");
            }

            if (!Regex.IsMatch(value.ToString(), @"[a-zA-Z0-9]+$"))
            {
                return new ValidationResult(false, "Логин должен содержать только латинские буквы и цифры");
            }

            return ValidationResult.ValidResult;
        }
    }
}
