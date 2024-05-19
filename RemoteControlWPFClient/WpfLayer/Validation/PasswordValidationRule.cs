using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RemoteControlWPFClient.WpfLayer.Validation
{
    public class PasswordValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value.ToString().Length < 6)
            {
                return new ValidationResult(false,"Пароль должен содержать больше 6 символов");
            }

            if (!Regex.IsMatch(value.ToString(), @"[a-zA-Z0-9]+$"))
            {
                return new ValidationResult(false, "Пароль должен содержать только латинские буквы и цифры");
            }

            if (!value.ToString().Any(char.IsDigit))
            {
                return new ValidationResult(false, "Пароль должен содержать цифры");
            }

            if (!value.ToString().Any(char.IsLetter))
            {
                return new ValidationResult(false, "Пароль должен содержать буквы");
            }

            if (!value.ToString().Any(char.IsUpper) && !value.ToString().Any(char.IsLower))
            {
                return new ValidationResult(false, "Пароль должен содержать буквы разного регистра");
            }

            return ValidationResult.ValidResult;
        }
    }
}
