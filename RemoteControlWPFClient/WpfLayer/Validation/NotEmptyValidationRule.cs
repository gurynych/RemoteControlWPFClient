﻿using System.Globalization;
using System.Windows.Controls;

namespace RemoteControlWPFClient.WpfLayer.Validation
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
            ? new ValidationResult(false, "Поле обязательно")
            : ValidationResult.ValidResult;
        }
    }
}
