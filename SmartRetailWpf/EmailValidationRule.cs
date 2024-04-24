using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SmartRetailWpf
{
    public class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                string email = value as string;

                // Проверка почтового адреса с помощью регулярного выражения
                var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
                if (!emailRegex.IsMatch(email))
                {
                    return new ValidationResult(false, "Непподерживаемый адрес эл. почты");
                }

                return ValidationResult.ValidResult;
            }
            catch
            {
                return new ValidationResult(false, "Недопустимое значение");
            }
        }
    }
}
