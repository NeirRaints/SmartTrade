using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace SmartRetailWpf
{
    internal class InputValidation
    {
        // Ограничение ввода только для букв и цифр
        internal bool ValidateInputOnlyLettersAndNumbers(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(char.IsLetterOrDigit);
        }

        // Ограничение для ввода только букв и цифр без кириллицы
        internal bool ValidateInputOnlyLatinLettersAndNumbers(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(ch => char.IsLetterOrDigit(ch) && !IsCyrillic(ch));
        }

        // Ограничение для ввода только цифр с символами
        internal bool ValidateInputOnlyNumbersWithSymbols(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(ch => char.IsDigit(ch) || char.IsSymbol(ch));
        }

        // Ограничение для ввода только букв
        internal bool ValidateInputOnlyLetters(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(char.IsLetter);
        }

        // Ограничение для ввода только цифр
        internal bool ValidateInputOnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(char.IsDigit);
        }

        // Ограничение для ввода только символов
        internal bool ValidateInputOnlySymbols(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(char.IsSymbol);
        }

        // Ограничение для ввотда только букв с кириллицей
        internal bool ValidateOnlyCyrillicLetters(object sender, TextCompositionEventArgs e)
        {
            return e.Text.All(ch => char.IsLetter(ch) && IsCyrillic(ch));
        }

        // Проверка является ли ведёный текст кириллическим
        internal bool IsCyrillic(char ch)
        {
            return Regex.IsMatch(ch.ToString(), @"[а-яА-ЯёЁ]");
        }
    }
}
