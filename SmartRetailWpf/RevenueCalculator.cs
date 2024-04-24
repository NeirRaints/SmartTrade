using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SmartRetailWpf
{
    internal class RevenueCalculator
    {
        // Вывести годовую выручку с учётом шаблона
        public static string CalculateAndFormatAnnualRevenue(List<тЗаказы> orders)
        {
            decimal annualRevenue = CalculateAnnualRevenue(orders);
            return FormatRevenue(annualRevenue);
        }

        // Вывести недельную выручку с учётом шаблона
        public static string CalculateAndFormatWeeklyRevenue(List<тЗаказы> orders, DateTime selectedDate)
        {
            decimal weeklyRevenue = CalculateWeeklyRevenue(orders, selectedDate);
            return FormatRevenue(weeklyRevenue);
        }

        // Вычислить годовую выручку
        internal static decimal CalculateAnnualRevenue(List<тЗаказы> orders)
        {
            decimal annualRevenue = 0;
            foreach (var order in orders)
            {
                annualRevenue += GetTotalOrderRevenue(order);
            }
            return annualRevenue;
        }

        // Вычислить месячную выручку
        internal static decimal CalculateMonthlyRevenue(List<тЗаказы> orders)
        {
            DateTime today = DateTime.Today;
            DateTime oneMonthAgo = today.AddMonths(-1);

            decimal monthlyRevenue = 0;
            foreach (var order in orders)
            {
                if (order.Дата_покупки >= oneMonthAgo && order.Дата_покупки <= today)
                {
                    monthlyRevenue += GetTotalOrderRevenue(order);
                }
            }
            return monthlyRevenue;
        }

        // Вычислить недельную выручку
        internal static decimal CalculateWeeklyRevenue(List<тЗаказы> orders, DateTime selectedDate)
        {
            DateTime selectedDateStartOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek + 1);
            DateTime selectedWeekEndDate = selectedDateStartOfWeek.AddDays(7);

            decimal weeklyRevenue = 0;
            foreach (var order in orders)
            {
                if (order.Дата_покупки >= selectedDateStartOfWeek && order.Дата_покупки <= selectedWeekEndDate)
                {
                    weeklyRevenue += GetTotalOrderRevenue(order);
                }
            }
            return weeklyRevenue;
        }

        // Получить выручку по каждой продаже
        private static decimal GetTotalOrderRevenue(тЗаказы order)
        {
            using (var entity = new SmartRetailEntities())
            {
                var product = entity.тТовары.FirstOrDefault(p => p.КодТовара == order.КодТовара);
                if (product != null)
                {
                    decimal totalRevenue = (decimal)(product.Цена * order.Количество);
                    return totalRevenue;
                }
                return 0;
            }
        }

        // Сгруппировать заказы по неделям и вычислить выручку за каждую неделю
        public static Dictionary<string, decimal> GetWeeklyRevenueByWeeks(List<тЗаказы> orders)
        {
            var weeklyRevenue = new Dictionary<string, decimal>();

            foreach (var order in orders)
            {
                var weekNumber = GetWeekNumber(order.Дата_покупки);
                if (!weeklyRevenue.ContainsKey(weekNumber))
                {
                    weeklyRevenue[weekNumber] = 0;
                }
                weeklyRevenue[weekNumber] += GetTotalOrderRevenue(order);
            }

            return weeklyRevenue;
        }

        // Получить номер недели для заданной даты
        private static string GetWeekNumber(DateTime date)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            return ci.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString();
        }

        // Задать шаблон для выводимого числа при достижении определённого максимума
        public static string FormatRevenue(decimal revenue)
        {
            if (revenue >= 1000000)
            {
                return (revenue / 1000000).ToString("0.0 млн.");
            }
            else if (revenue >= 100000)
            {
                return (revenue / 1000).ToString("0 тыс.");
            }
            else
            {
                return revenue.ToString("C");
            }
        }
    }
}
