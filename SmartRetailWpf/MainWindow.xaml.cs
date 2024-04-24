using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace SmartRetailWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InputValidation Validation = new InputValidation();

        public MainWindow()
        {
            InitializeComponent();
            
            LoadOrdersData();
            LoadOrdersProductsData();
            LoadClientsData();
            LoadProductsData();

            // Установить начальную дату на текущую дату
            SelectOrderDateCalendar.SelectedDate = DateTime.Today;

            // Создать график для начальной даты
            CreatePlot(SelectOrderDateCalendar.SelectedDate.Value);
        }

        /* Загрузить данные из бд в OrdersDg, 
         * текстбоксы с информацией о выручке*/
        private void LoadOrdersData()
        {
            using (var entity = new SmartRetailEntities())
            {
                var AllOrders = entity.тЗаказы.ToList();
                OrdersDg.ItemsSource = AllOrders;
                AllOrdersTxt.Text = AllOrders.Count.ToString();

                string formattedAnnualRevenue = RevenueCalculator.CalculateAndFormatAnnualRevenue(AllOrders);
                AnnualRevenueTxt.Text = formattedAnnualRevenue;

                string formattedWeeklyRevenue = RevenueCalculator.CalculateAndFormatWeeklyRevenue(AllOrders, DateTime.Today);
                WeeklyRevenueTxt.Text = formattedWeeklyRevenue;
            }
        }

        // Загрузить данные из бд в OrdersProductsDg
        private void LoadOrdersProductsData()
        {
            using (var entity = new SmartRetailEntities())
            {
                OrdersProductsDg.ItemsSource = entity.тТовары.ToList();
            }
        }

        // Загрузить данные из бд в ClientsDg
        private void LoadClientsData()
        {
            using (var entity = new SmartRetailEntities())
            {
                ClientsDg.ItemsSource = entity.тКлиенты.ToList();
            }
        }

        // Загрузить данные из бд в ProductsDg
        private void LoadProductsData()
        {
            using (var entity = new SmartRetailEntities())
            {
                ProductsDg.ItemsSource = entity.тТовары.ToList();
            }
        }

        // Изменить цвет bg при нажатии, открыть соответствующую вкладку
        private void BordersChangeBgColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Сбросить задний фон у всех Border
            Home.Background = null;
            Orders.Background = null;
            Clients.Background = null;
            Products.Background = null;

            // Установить градиент в качестве заднего фона
            Border clickedBorder = sender as Border;
            // Отображение соответствующего TabItem
            switch (clickedBorder.Name)
            {
                case "Home":
                    MenuControl.SelectedItem = HomeItem;
                    break;
                case "Orders":
                    MenuControl.SelectedItem = OrdersItem;
                    break;
                case "Clients":
                    MenuControl.SelectedItem = ClientsItem;
                    break;
                case "Products":
                    MenuControl.SelectedItem = ProductsItem;
                    break;
            }
            clickedBorder.Background = (Brush)Resources["GradientBrush"];
        }

        // Создать метод для построения графика
        private void CreatePlot(DateTime selectedDate)
        {
            using (var entity = new SmartRetailEntities())
            {
                // Получить данные о месячной выручке по неделям для выбранного месяца
                DateTime startDateOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                DateTime endDateOfMonth = startDateOfMonth.AddMonths(1).AddDays(-1);
                var ordersForSelectedMonth = entity.тЗаказы.Where(order => order.Дата_покупки >= startDateOfMonth && order.Дата_покупки <= endDateOfMonth).ToList();
                Dictionary<string, decimal> weeklyRevenueData = RevenueCalculator.GetWeeklyRevenueByWeeks(ordersForSelectedMonth);

                // Создать новый график
                PlotProfit.Plot.Clear();
                PlotProfit.Plot.Title("Месячная выручка");
                PlotProfit.Plot.XLabel("Неделя");
                PlotProfit.Plot.YLabel("Выручка");

                // Добавить данные в график
                foreach (var kvp in weeklyRevenueData)
                {
                    double weekNumber = double.Parse(kvp.Key);
                    double revenue = (double)kvp.Value;
                    PlotProfit.Plot.Add.Bar(weekNumber, revenue);
                }

                // Обновить график
                PlotProfit.Refresh();
            }
        }

        //Выгрузить заказы по выбранной дате календаря
        private void SelectOrderDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectOrderDateCalendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = SelectOrderDateCalendar.SelectedDate.Value.Date;

                using (var entity = new SmartRetailEntities())
                {
                    var ordersForSelectedDate = entity.тЗаказы.Where(order => order.Дата_покупки == selectedDate).ToList();
                    LastOrdersDg.ItemsSource = ordersForSelectedDate;

                    // Устанавливаем количество записей в AllOrdersPerDay
                    AllOrdersPerDayTxt.Text = ordersForSelectedDate.Count.ToString();

                    // Годовая выручка
                    DateTime selectedYearStartDate = new DateTime(selectedDate.Year, 1, 1);
                    DateTime selectedYearEndDate = new DateTime(selectedDate.Year, 12, 31);
                    var ordersForSelectedYear = entity.тЗаказы.Where(order => order.Дата_покупки >= selectedYearStartDate && order.Дата_покупки <= selectedYearEndDate).ToList();
                    decimal annualRevenue = RevenueCalculator.CalculateAnnualRevenue(ordersForSelectedYear);
                    AnnualRevenueTxt.Text = RevenueCalculator.FormatRevenue(annualRevenue);

                    // Недельная выручка
                    DateTime StartOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek + 1);
                    DateTime selectedWeekEndDate = StartOfWeek.AddDays(7);
                    var ordersForSelectedWeek = entity.тЗаказы.Where(order => order.Дата_покупки >= StartOfWeek && order.Дата_покупки <= selectedWeekEndDate).ToList();
                    decimal weeklyRevenue = RevenueCalculator.CalculateWeeklyRevenue(ordersForSelectedWeek, selectedDate);
                    WeeklyRevenueTxt.Text = RevenueCalculator.FormatRevenue(weeklyRevenue);

                    // Обновить график для выбранной даты
                    CreatePlot(SelectOrderDateCalendar.SelectedDate.Value);
                }
            }
        }

        // Открыть форму добавления заказа
        private void AddOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            AddOrderBg.Visibility = Visibility.Visible;
            AddOrderForm.Visibility = Visibility.Visible;

            using (var dbContext = new SmartRetailEntities())
            {
                // Получаем доступные коды клиентов
                var clients = dbContext.тКлиенты.Select(client => client.КодКлиента).ToList();
                ClientsCodesBox.ItemsSource = clients;

                // Получаем доступные коды товаров
                var products = dbContext.тТовары.Select(product => product.КодТовара).ToList();
                ProductsCodesBox.ItemsSource = products;
            }
        }

        // Закрыть форму добавления заказа
        private void AddOrderBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddOrderBg.Visibility = Visibility.Collapsed;
            AddOrderForm.Visibility = Visibility.Collapsed;
        }

        // Добавить заказ в бд
        private void AddOrderFormBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ClientsCodesBox.Text) || string.IsNullOrEmpty(ProductsCodesBox.Text) || string.IsNullOrWhiteSpace(ProductsCount.Text) || BuyingDate.SelectedDate == null)
            {
                AddOrderFormErrorTxt.Visibility = Visibility.Visible;
                AddOrderFormErrorTxt.Text = "Заполните все поля";
                return;
            }

            int clientCode = Convert.ToInt32(ClientsCodesBox.Text);
            int productCode = Convert.ToInt32(ProductsCodesBox.Text);

            string productsCountText = ProductsCount.Text.Replace(" ", ""); // Удаляем все пробелы

            if (productsCountText.Length > 0 && productsCountText[0] == '0')
            {
                AddOrderFormErrorTxt.Visibility = Visibility.Visible;
                AddOrderFormErrorTxt.Text = "Количество товара должно быть больше 0";
                return;
            }

            int productCount = Convert.ToInt32(productsCountText);

            DateTime buyingDate = BuyingDate.SelectedDate.Value;

            using (var entity = new SmartRetailEntities())
            {
                if (entity.тЗаказы.Any(o => o.КодКлиента == clientCode && o.КодТовара == productCode && o.Дата_покупки == buyingDate))
                {
                    AddOrderFormErrorTxt.Visibility = Visibility.Visible;
                    AddOrderFormErrorTxt.Text = "Такой заказ уже существует";
                    return;
                }

                var product = entity.тТовары.Find(productCode);

                // Проверяем количество товара
                if (product.НаСкладе < productCount)
                {
                    AddOrderFormErrorTxt.Visibility = Visibility.Visible;
                    AddOrderFormErrorTxt.Text = "На складе недостаточно товара";
                    return;
                }

                // Вычитаем заказанное количество из количества товара
                product.НаСкладе -= productCount;

                // Создаем новый заказ и добавляем его в контекст
                var newOrder = new тЗаказы
                {
                    КодКлиента = clientCode,
                    КодТовара = productCode,
                    Количество = productCount,
                    Дата_покупки = buyingDate
                };
                entity.тЗаказы.Add(newOrder);

                // Сохраняем изменения в контексте
                entity.SaveChanges();
            }

            // Очищаем поля ввода и обновляем данные
            ClearInputControls(AddOrderForm);
            LoadOrdersData();
            LoadProductsData();

            // Скрываем формы
            AddOrderBg.Visibility = Visibility.Collapsed;
            AddOrderForm.Visibility = Visibility.Collapsed;
        }

        // Изменить видимость AddOrderFormErrorTxt когда input формы получает фокус
        private void ChangeAddOrderErrorTxtVisibility_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AddOrderFormErrorTxt.Visibility == Visibility.Visible)
                AddOrderFormErrorTxt.Visibility = Visibility.Collapsed;
        }

        // Удаление выделенной записи при нажатии на неё ПКМ
        private void OrdersDg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedOrder = OrdersDg.SelectedItem as тЗаказы;

            if (selectedOrder != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?", "Удаление заказа",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var entity = new SmartRetailEntities())
                    {
                        // Прикрепляем объект к контексту, если он объект данных был отсоединен
                        entity.тЗаказы.Attach(selectedOrder);
                        entity.тЗаказы.Remove(selectedOrder);
                        entity.SaveChanges();
                    }

                    // Повторно загружаем данные для обновления DataGrid
                    LoadOrdersData();
                }
            }
        }

        // Найти заказ по его номеру
        private void FindOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FindOrderBx.Text))
            {
                LoadOrdersData();
            }
            else
            {
                int orderCode = Convert.ToInt32(FindOrderBx.Text);
                using (var entity = new SmartRetailEntities())
                {
                    var searchingOrder = entity.тЗаказы.Where(order => order.КодЗаказа == orderCode).ToList();
                    OrdersDg.ItemsSource = searchingOrder;
                }
            }
        }

        // Открыть форму редактирования заказа
        private void EditOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if(OrdersDg.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            using (var dbContext = new SmartRetailEntities())
            {
                // Получаем доступные коды клиентов
                var clients = dbContext.тКлиенты.Select(client => client.КодКлиента).ToList();
                EditOrdClientCodeBox.ItemsSource = clients;

                // Получаем доступные коды товаров
                var products = dbContext.тТовары.Select(product => product.КодТовара).ToList();
                EditOrdProductCodeBox.ItemsSource = products;
            }

            var selectedOrder = (тЗаказы)OrdersDg.SelectedItem;
  
            EditOrdOrderCodeBx.Text = selectedOrder.КодЗаказа.ToString();
            EditOrdClientCodeBox.SelectedItem = selectedOrder.КодКлиента;
            EditOrdProductCodeBox.SelectedItem = selectedOrder.КодТовара;
            EditOrdCountBx.Text = selectedOrder.Количество.ToString();
            EditOrdBuyDate.SelectedDate = selectedOrder.Дата_покупки;

            EditOrderBg.Visibility = Visibility.Visible;
            EditOrderForm.Visibility = Visibility.Visible;
        }

        // Закрыть форму редактирования заказа
        private void EditOrderBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditOrderBg.Visibility = Visibility.Collapsed;
            EditOrderForm.Visibility = Visibility.Collapsed;
        }

        // Редактировать заказ
        private void SumbitOrdEdit_Click(object sender, RoutedEventArgs e)
        {
            int orderCode = Convert.ToInt32(EditOrdOrderCodeBx.Text);
            if (string.IsNullOrEmpty(EditOrdClientCodeBox.Text) || string.IsNullOrEmpty(EditOrdProductCodeBox.Text) || string.IsNullOrWhiteSpace(EditOrdCountBx.Text) || EditOrdBuyDate.SelectedDate == null)
            {
                EditOrderFormErrorTxt.Visibility = Visibility.Visible;
                EditOrderFormErrorTxt.Text = "Заполните все поля";
                return;
            }

            int clientCode = Convert.ToInt32(EditOrdClientCodeBox.Text);
            int productCode = Convert.ToInt32(EditOrdProductCodeBox.Text);

            string productsCountText = EditOrdCountBx.Text.Replace(" ", ""); // Удаляем все пробелы

            if (productsCountText.Length > 0 && productsCountText[0] == '0')
            {
                EditOrderFormErrorTxt.Visibility = Visibility.Visible;
                EditOrderFormErrorTxt.Text = "Количество товара должно быть больше 0";
                return;
            }

            int productCount = Convert.ToInt32(productsCountText);

            DateTime buyingDate = EditOrdBuyDate.SelectedDate.Value;

            using (var entity = new SmartRetailEntities())
            {
                if (entity.тЗаказы.Any(o => o.КодКлиента == clientCode && o.КодТовара == productCode && o.Дата_покупки == buyingDate))
                {
                    EditOrderFormErrorTxt.Visibility = Visibility.Visible;
                    EditOrderFormErrorTxt.Text = "Такой заказ уже существует";
                    return;
                }

                var product = entity.тТовары.Find(productCode);

                // Проверяем количество товара
                if (product.НаСкладе < productCount)
                {
                    EditOrderFormErrorTxt.Visibility = Visibility.Visible;
                    EditOrderFormErrorTxt.Text = "На складе недостаточно товара";
                    return;
                }

                // Вычитаем заказанное количество из количества товара
                product.НаСкладе -= productCount;

                // Получаем заказ
                var searchingOrder = entity.тЗаказы.Find(orderCode);
                if (searchingOrder != null)
                {
                    // Обновляем поля заказа
                    searchingOrder.КодКлиента = clientCode;
                    searchingOrder.КодТовара = productCode;
                    searchingOrder.Количество = productCount;
                    searchingOrder.Дата_покупки = buyingDate;
                }

                // Сохраняем изменения в контексте
                entity.SaveChanges();
            }

            // Обновляем данные и скрываем формы
            LoadOrdersData();
            LoadProductsData();
            EditOrderBg.Visibility = Visibility.Collapsed;
            EditOrderForm.Visibility = Visibility.Collapsed;
        }

        // Изменить видимость EditOrderFormErrorTxt когда input формы получает фокус
        private void ChangeEditOrderErrorTxtVisibility_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EditOrderFormErrorTxt.Visibility == Visibility.Visible)
                EditOrderFormErrorTxt.Visibility = Visibility.Collapsed;
        }

        // Обновить OrdersProductsDg
        private void UpdateProductsDgBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadOrdersProductsData();
        }

        // Открыть форму добавления клиента
        private void AddClientBtn_Click(object sender, RoutedEventArgs e)
        {
            AddClientBg.Visibility = Visibility.Visible;
            AddClientForm.Visibility = Visibility.Visible;
        }

        // Закрыть форму добавления клиента
        private void AddClientBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddClientBg.Visibility = Visibility.Collapsed;
            AddClientForm.Visibility = Visibility.Collapsed;
        }

        // Найти клиента по его номеру
        private void FindClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(FindClientBx.Text)) 
            {
                LoadClientsData();
            }
            else
            {
                int clientCode = Convert.ToInt32(FindClientBx.Text);
                using (var entity = new SmartRetailEntities())
                {
                    var seacrhingClient = entity.тКлиенты.Where(client => client.КодКлиента == clientCode).ToList();
                    ClientsDg.ItemsSource = seacrhingClient;
                }
            }
        }

        // Добавить клиента в бд
        private void AddClientFormBtn_Click(object sender, RoutedEventArgs e)
        {
            // Убрать лишние пробелы в полях формы
            string surname = SurnameBx.Text.Trim();
            string address = AddressBx.Text.Trim();
            string phone = PhoneBx.Text.Trim();
            string email = EmailBx.Text.Trim();

            if (string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email))
            {
                AddClientFormErrorTxt.Visibility = Visibility.Visible;
                AddClientFormErrorTxt.Text = "Заполните все поля";
                return;
            }

            if (phone.Length != 11 || !(phone.StartsWith("7") || phone.StartsWith("8")))
            {
                AddClientFormErrorTxt.Visibility = Visibility.Visible;
                AddClientFormErrorTxt.Text = "Некорректный номер телефона";
                return;
            }

            EmailValidationRule emailValidation = new EmailValidationRule();
            ValidationResult validationResult = emailValidation.Validate(email, CultureInfo.CurrentCulture);

            if (!validationResult.IsValid)
            {
                AddClientFormErrorTxt.Text = validationResult.ErrorContent.ToString();
                AddClientFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            using (var entity = new SmartRetailEntities())
            {
                var searchingClient = entity.тКлиенты.FirstOrDefault(client => client.Телефон == phone || client.Email == email);

                if (searchingClient != null)
                {
                    AddClientFormErrorTxt.Text = "Такой клиент уже существует";
                    AddClientFormErrorTxt.Visibility = Visibility.Visible;
                    return;
                }

                var newClient = new тКлиенты
                {
                    Фамилия = surname,
                    Адрес = address,
                    Телефон = phone,
                    Email = email
                };
                entity.тКлиенты.Add(newClient);
                entity.SaveChanges();
            }
            ClearInputControls(AddClientForm);
            LoadClientsData();
            AddClientBg.Visibility = Visibility.Collapsed;
            AddClientForm.Visibility = Visibility.Collapsed;
        }

        // Изменить видимость AddClientFormErrorTxt когда input формы получает фокус
        private void ChangeAddClientErrorTxtVisibility_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AddClientFormErrorTxt.Visibility == Visibility.Visible)
                AddClientFormErrorTxt.Visibility = Visibility.Collapsed;
        }

        // Удаление выделенной записи при нажатии ПКМ в ClientsDg
        private void ClientsDg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedClient = ClientsDg.SelectedItem as тКлиенты;

            if (selectedClient != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Удаление клиента",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var entity = new SmartRetailEntities())
                    {
                        // Прикрепляем объект к контексту, если он объект данных был отсоединен
                        entity.тКлиенты.Attach(selectedClient);
                        entity.тКлиенты.Remove(selectedClient);
                        entity.SaveChanges();
                    }

                    // Повторно загружаем данные для обновления DataGrid
                    LoadClientsData();
                }
            }
        }

        // Открыть форму редактирования клиента
        private void EditClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ClientsDg.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }
            
            var selectedClient = (тКлиенты)ClientsDg.SelectedItem;

            EditClientCodeBx.Text = selectedClient.КодКлиента.ToString();
            EditSurnameBx.Text = selectedClient.Фамилия;
            EditAddressBx.Text = selectedClient.Адрес;
            EditPhoneBx.Text = selectedClient.Телефон;
            EditEmailBx.Text = selectedClient.Email;


            EditClientBg.Visibility = Visibility.Visible;
            EditClientForm.Visibility = Visibility.Visible;
        }

        // Закрыть форму редактирования клиента
        private void EditClientBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditClientBg.Visibility = Visibility.Collapsed;
            EditClientForm.Visibility = Visibility.Collapsed;
        }

        // Редактировать клиента
        private void EditClientFormBtn_Click(object sender, RoutedEventArgs e)
        {
            // Убрать лишние пробелы в полях формы
            int clientCode = Convert.ToInt32(EditClientCodeBx.Text);
            string surname = EditSurnameBx.Text.Trim();
            string address = EditAddressBx.Text.Trim();
            string phone = EditPhoneBx.Text.Trim();
            string email = EditEmailBx.Text.Trim();

            if (string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email))
            {
                EditClientFormErrorTxt.Visibility = Visibility.Visible;
                EditClientFormErrorTxt.Text = "Заполните все поля";
                return;
            }

            if (phone.Length != 11 || !(phone.StartsWith("7") || phone.StartsWith("8")))
            {
                EditClientFormErrorTxt.Visibility = Visibility.Visible;
                EditClientFormErrorTxt.Text = "Некорректный номер телефона";
                return;
            }

            EmailValidationRule emailValidation = new EmailValidationRule();
            ValidationResult validationResult = emailValidation.Validate(email, CultureInfo.CurrentCulture);

            if (!validationResult.IsValid)
            {
                EditClientFormErrorTxt.Text = validationResult.ErrorContent.ToString();
                EditClientFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            using (var entity = new SmartRetailEntities())
            {
                var searchingClient = entity.тКлиенты.FirstOrDefault(client => client.Телефон == phone || client.Email == email);

                if (searchingClient != null)
                {
                    EditClientFormErrorTxt.Text = "Такой клиент уже существует";
                    EditClientFormErrorTxt.Visibility = Visibility.Visible;
                    return;
                }

                var searchingClientByCode = entity.тКлиенты.Find(clientCode);
                
                if(searchingClientByCode != null)
                {
                    searchingClientByCode.Фамилия = surname;
                    searchingClientByCode.Адрес = address;
                    searchingClientByCode.Телефон = phone;
                    searchingClientByCode.Email = email;
                }
                entity.SaveChanges();
            }
            LoadClientsData();
            EditClientBg.Visibility = Visibility.Collapsed;
            EditClientForm.Visibility = Visibility.Collapsed;
        }

        // Изменить видимость EditClientFormErrorTxt когда input формы получает фокус
        private void ChangeEditClientErrorTxtVisibility_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EditClientFormErrorTxt.Visibility == Visibility.Visible)
                EditClientFormErrorTxt.Visibility = Visibility.Collapsed;
        }

        // Открыть форму добавления товара
        private void AddProductBtn_Click(object sender, RoutedEventArgs e)
        {
            AddProductBg.Visibility = Visibility.Visible;
            AddProductForm.Visibility = Visibility.Visible;
        }

        // Закрыть форму добавления товара
        private void AddProductBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddProductBg.Visibility = Visibility.Collapsed;
            AddProductForm.Visibility = Visibility.Collapsed;
        }

        // Найти товар по его номеру
        private void FindProductBtn_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(FindProductBx.Text)) 
            {
                LoadProductsData();
            }
            else 
            {
                int productCode = Convert.ToInt32(FindProductBx.Text);
                using(var entity = new SmartRetailEntities())
                {
                    var searchingProduct = entity.тТовары.Where(product => product.КодТовара == productCode).ToList();
                    ProductsDg.ItemsSource = searchingProduct;
                }
            }
        }

        // Добавить товар в бд
        private void AddProductFormBtn_Click(object sender, RoutedEventArgs e)
        {
            string productName = ProductNameBx.Text.Trim();
            string pricetxt = PriceBx.Text.Replace(" ", "");
            string stocktxt = StockBx.Text.Replace(" ", "");

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(pricetxt) || string.IsNullOrEmpty(stocktxt))
            {
                AddProductFormErrorTxt.Text = "Заполните все поля";
                AddProductFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            if (pricetxt.Length > 0 && pricetxt[0] == '0') 
            {
                AddProductFormErrorTxt.Text = "Цена не может быть нулевой";
                AddProductFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            if (stocktxt.Length > 0 && stocktxt[0] == '0') 
            {
                AddProductFormErrorTxt.Text = "Количество товара не может быть нулевым";
                AddProductFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            decimal price = Convert.ToDecimal(pricetxt);
            int stock = Convert.ToInt32(stocktxt);

            using (var entity = new SmartRetailEntities())
            {
                var searchingProduct = entity.тТовары.FirstOrDefault(product => product.Наименование == productName);
                
                if(searchingProduct != null)
                {
                    AddProductFormErrorTxt.Text = "Такой товар уже существует";
                    AddProductFormErrorTxt.Visibility = Visibility.Visible;
                    return;
                }

                var newProduct = new тТовары
                {
                    Наименование = productName,
                    Цена = price,
                    НаСкладе = stock
                };
                entity.тТовары.Add(newProduct);
                entity.SaveChanges();
            }
            ClearInputControls(AddProductForm);
            LoadProductsData();
            AddProductBg.Visibility = Visibility.Collapsed;
            AddProductForm.Visibility = Visibility.Collapsed;
        }

        // Изменить видимость AddProductFormErrorTxt когда input формы получает фокус
        private void ChangeAddProductErrorTxtVisibility_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AddProductFormErrorTxt.Visibility == Visibility.Visible)
                AddProductFormErrorTxt.Visibility = Visibility.Collapsed;
        }

        // Удаление выделенной записи при нажатии ПКМ в ProductsDg
        private void ProductsDg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedProduct = ProductsDg.SelectedItem as тТовары;

            if (selectedProduct != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Удаление товара",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var entity = new SmartRetailEntities())
                    {
                        // Прикрепляем объект к контексту, если он объект данных был отсоединен
                        entity.тТовары.Attach(selectedProduct);
                        entity.тТовары.Remove(selectedProduct);
                        entity.SaveChanges();
                    }

                    // Повторно загружаем данные для обновления DataGrid
                    LoadProductsData();
                }
            }
        }

        // Открыть форму редактирования товара
        private void EditProductBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ProductsDg.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            var selectedProduct = (тТовары)ProductsDg.SelectedItem;

            EditProductCode.Text = selectedProduct.КодТовара.ToString();
            EditProductNameBx.Text = selectedProduct.Наименование;
            EditPriceBx.Text = selectedProduct.Цена.ToString();
            EditStockBx.Text = selectedProduct.НаСкладе.ToString();

            EditProductBg.Visibility = Visibility.Visible;
            EditProductForm.Visibility = Visibility.Visible;
        }

        // Закрыть форму редактирования товара
        private void EditProductBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditProductBg.Visibility = Visibility.Collapsed;
            EditProductForm.Visibility = Visibility.Collapsed;
        }

        // Редактировать товар
        private void EditProductFormBtn_Click(object sender, RoutedEventArgs e)
        {
            int productCode = Convert.ToInt32(EditProductCode.Text);
            string productName = EditProductNameBx.Text.Trim();
            string pricetxt = EditPriceBx.Text.Replace(" ", "");
            string stocktxt = EditStockBx.Text.Replace(" ", "");

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(pricetxt) || string.IsNullOrEmpty(stocktxt))
            {
                EditProductFormErrorTxt.Text = "Заполните все поля";
                EditProductFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            if (pricetxt.Length > 0 && pricetxt[0] == '0')
            {
                EditProductFormErrorTxt.Text = "Цена не может быть нулевой";
                EditProductFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            if (stocktxt.Length > 0 && stocktxt[0] == '0')
            {
                EditProductFormErrorTxt.Text = "Количество товара не может быть нулевым";
                EditProductFormErrorTxt.Visibility = Visibility.Visible;
                return;
            }

            decimal price = Convert.ToDecimal(pricetxt);
            int stock = Convert.ToInt32(stocktxt);

            using (var entity = new SmartRetailEntities())
            {
                var searchingProduct = entity.тТовары.FirstOrDefault(product => product.Наименование == productName);

                if (searchingProduct != null)
                {
                    EditProductFormErrorTxt.Text = "Такой товар уже существует";
                    EditProductFormErrorTxt.Visibility = Visibility.Visible;
                    return;
                }

                var searchingProductByCode = entity.тТовары.Find(productCode);

                if (searchingProductByCode != null)
                {
                    searchingProductByCode.Наименование = productName;
                    searchingProductByCode.Цена = price;
                    searchingProductByCode.НаСкладе = stock;
                }
                entity.SaveChanges();
            }
            LoadProductsData();
            EditProductBg.Visibility = Visibility.Collapsed;
            EditProductForm.Visibility = Visibility.Collapsed;
        }

        // Изменить видимость EditProductFormErrorTxt когда input формы получает фокус
        private void ChangeEditProductErrorTxtVisibility_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EditProductFormErrorTxt.Visibility == Visibility.Visible)
                EditProductFormErrorTxt.Visibility = Visibility.Collapsed;
        }

        // Очистить textbox, если он стал активным
        private void ClearTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Clear();
            textBox.Foreground = Brushes.Black;
        }

        // Очистить input в форме
        private static void ClearInputControls(DependencyObject parent)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
            {
                ClearInputControls(child);

                if (!(child is TextBox || child is ComboBox || child is DatePicker))
                {
                    continue;
                }

                if (child is TextBox textBox)
                {
                    textBox.Text = string.Empty;
                }
                else if (child is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = -1;
                }
                else if (child is DatePicker datePicker)
                {
                    datePicker.SelectedDate = null;
                }
            }
        }

        // Ограничение на ввод, только цифры
        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Validation.ValidateInputOnlyNumbers(sender, e);
        }

        // Ограничение на ввод, только буквы c кириллицей
        private void OnlyCyrillicLetters_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Validation.ValidateOnlyCyrillicLetters(sender, e);
        }
    }
}
