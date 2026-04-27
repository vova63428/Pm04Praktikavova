using proectiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace proectiq
{
    public partial class MainWindow : Window
    {
        TestBaseEntities db = new TestBaseEntities();
        private Partners selectedPartner = null;
        private Partners_product selectedHistory = null;

        public MainWindow()
        {
            InitializeComponent();
            LoadPartners();
            LoadProducts();
            ClearPartnerForm();
            ClearHistoryForm();
            EnableHistoryButtons(false);
        }

        // Загрузка списка партнеров
        void LoadPartners()
        {
            var listpartners = db.Partners.Select(a => new
            {
                a.ID,
                a.Тип_партнера,
                a.Наименование_партнера,
                a.Директор,
                a.Телефон_партнера,
                a.Электронная_почта_партнера,
                a.Юридический_адрес_партнера,
                a.ИНН,
                a.Рейтинг,
                Скидка = db.Partners_product.Where(y => a.ID == y.ID_Partner).Sum(x => x.Количество_продукции) ?? 0
            }).ToList();

            // Преобразуем для отображения скидки в виде процентов
            var displayList = listpartners.Select(p => new
            {
                p.ID,
                p.Тип_партнера,
                p.Наименование_партнера,
                p.Директор,
                p.Телефон_партнера,
                p.Электронная_почта_партнера,
                p.Юридический_адрес_партнера,
                p.ИНН,
                p.Рейтинг,
                Скидка = GetDiscountString(p.Скидка)
            }).ToList();

            listPartner.ItemsSource = displayList;
        }

        string GetDiscountString(double totalSum)
        {
            if (totalSum < 10000) return "0%";
            if (totalSum < 50000) return "5%";
            if (totalSum < 300000) return "10%";
            return "15%";
        }

        // Загрузка продуктов в комбобокс
        void LoadProducts()
        {
            var products = db.Product.ToList();
            cmbProduct.ItemsSource = products;
        }

        // Загрузка истории покупок выбранного партнера
        void LoadHistory()
        {
            if (selectedPartner == null)
            {
                listHistory.ItemsSource = null;
                return;
            }

            var history = from pp in db.Partners_product
                          join p in db.Product on pp.ID_Product equals p.ID
                          where pp.ID_Partner == selectedPartner.ID
                          select new
                          {
                              pp.ID,
                              pp.ID_Product,
                              ProductName = p.Наименование_продукции,
                              Quantity = pp.Количество_продукции,
                              SaleDate = pp.Дата_продажи,
                              Cost = (pp.Количество_продукции ?? 0) * (p.Минимальная_стоимость_для_партнера ?? 0)
                          };

            listHistory.ItemsSource = history.ToList();
            UpdatePartnerDiscount();
        }

        // Обновление скидки партнера
        void UpdatePartnerDiscount()
        {
            if (selectedPartner == null) return;

            var sum = db.Partners_product.Where(y => selectedPartner.ID == y.ID_Partner)
                                         .Sum(x => x.Количество_продукции) ?? 0;

            string sale = GetDiscountString(sum);
            lblDiscount.Content = $"{sale} (всего: {sum:N0} шт.)";
        }

        // Очистка формы партнера
        void ClearPartnerForm()
        {
            txtType.Clear();
            txtName.Clear();
            txtDirector.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtRating.Clear();
            txtAddress.Clear();
            txtINN.Clear();
            selectedPartner = null;
        }

        // Очистка формы истории
        void ClearHistoryForm()
        {
            cmbProduct.SelectedIndex = -1;
            txtQuantity.Clear();
            dpSaleDate.SelectedDate = DateTime.Now;
            selectedHistory = null;
        }

        // Включение/отключение кнопок истории
        void EnableHistoryButtons(bool enable)
        {
            btnAddHistory.IsEnabled = true;
            btnEditHistory.IsEnabled = enable;
            btnDeleteHistory.IsEnabled = enable;
        }

        // Выбор партнера в списке
        private void ListPartner_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = listPartner.SelectedItem;
            if (selected != null)
            {
                // Получаем свойства выбранного элемента через рефлексию
                var properties = selected.GetType().GetProperties();
                var id = (int)properties.First(p => p.Name == "ID").GetValue(selected);

                selectedPartner = db.Partners.Find(id);

                if (selectedPartner != null)
                {
                    // Заполняем форму данными выбранного партнера
                    txtType.Text = selectedPartner.Тип_партнера;
                    txtName.Text = selectedPartner.Наименование_партнера;
                    txtDirector.Text = selectedPartner.Директор;
                    txtPhone.Text = selectedPartner.Телефон_партнера;
                    txtEmail.Text = selectedPartner.Электронная_почта_партнера;
                    txtRating.Text = selectedPartner.Рейтинг?.ToString();
                    txtAddress.Text = selectedPartner.Юридический_адрес_партнера;
                    txtINN.Text = selectedPartner.ИНН?.ToString();

                    LoadHistory();
                    ClearHistoryForm();
                    EnableHistoryButtons(false);
                }
            }
        }

        // Двойной клик по партнеру
        private void ListPartner_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Можно оставить пустым или добавить дополнительную логику
        }

        // Добавление партнера
        private void BtnAddPartner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите наименование партнера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newPartner = new Partners
                {
                    Тип_партнера = txtType.Text,
                    Наименование_партнера = txtName.Text,
                    Директор = txtDirector.Text,
                    Телефон_партнера = txtPhone.Text,
                    Электронная_почта_партнера = txtEmail.Text,
                    Юридический_адрес_партнера = txtAddress.Text
                };

                if (!string.IsNullOrWhiteSpace(txtRating.Text))
                {
                    if (double.TryParse(txtRating.Text, out double rating))
                        newPartner.Рейтинг = rating;
                }

                if (!string.IsNullOrWhiteSpace(txtINN.Text))
                {
                    if (double.TryParse(txtINN.Text, out double inn))
                        newPartner.ИНН = inn;
                }

                db.Partners.Add(newPartner);
                db.SaveChanges();

                MessageBox.Show("Партнер успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPartners();
                ClearPartnerForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Редактирование партнера
        private void BtnEditPartner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPartner == null)
                {
                    MessageBox.Show("Выберите партнера для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var partner = db.Partners.Find(selectedPartner.ID);
                if (partner != null)
                {
                    partner.Тип_партнера = txtType.Text;
                    partner.Наименование_партнера = txtName.Text;
                    partner.Директор = txtDirector.Text;
                    partner.Телефон_партнера = txtPhone.Text;
                    partner.Электронная_почта_партнера = txtEmail.Text;
                    partner.Юридический_адрес_партнера = txtAddress.Text;

                    if (!string.IsNullOrWhiteSpace(txtRating.Text))
                    {
                        if (double.TryParse(txtRating.Text, out double rating))
                            partner.Рейтинг = rating;
                    }

                    if (!string.IsNullOrWhiteSpace(txtINN.Text))
                    {
                        if (double.TryParse(txtINN.Text, out double inn))
                            partner.ИНН = inn;
                    }

                    db.SaveChanges();

                    MessageBox.Show("Данные партнера обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPartners();
                    ClearPartnerForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Удаление партнера
        private void BtnDeletePartner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPartner == null)
                {
                    MessageBox.Show("Выберите партнера для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите удалить партнера \"{selectedPartner.Наименование_партнера}\"?\nВсе связанные покупки также будут удалены!",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var partner = db.Partners.Find(selectedPartner.ID);
                    if (partner != null)
                    {
                        db.Partners.Remove(partner);
                        db.SaveChanges();

                        MessageBox.Show("Партнер удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadPartners();
                        ClearPartnerForm();
                        listHistory.ItemsSource = null;
                        lblDiscount.Content = "0%";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Выбор истории в списке
        private void ListHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = listHistory.SelectedItem;
            if (selected != null)
            {
                var propertyInfo = selected.GetType().GetProperties();
                var id = (int)propertyInfo.First(p => p.Name == "ID").GetValue(selected);
                selectedHistory = db.Partners_product.Find(id);

                if (selectedHistory != null)
                {
                    cmbProduct.SelectedValue = selectedHistory.ID_Product;
                    txtQuantity.Text = selectedHistory.Количество_продукции?.ToString();
                    dpSaleDate.SelectedDate = selectedHistory.Дата_продажи;
                    EnableHistoryButtons(true);
                }
            }
        }

        // Добавление покупки
        private void BtnAddHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPartner == null)
                {
                    MessageBox.Show("Выберите партнера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbProduct.SelectedValue == null || string.IsNullOrWhiteSpace(txtQuantity.Text) || dpSaleDate.SelectedDate == null)
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(txtQuantity.Text, out double quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество продукции!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newHistory = new Partners_product
                {
                    ID_Product = (int)cmbProduct.SelectedValue,
                    ID_Partner = selectedPartner.ID,
                    Количество_продукции = quantity,
                    Дата_продажи = dpSaleDate.SelectedDate
                };

                db.Partners_product.Add(newHistory);
                db.SaveChanges();

                MessageBox.Show("Покупка добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadHistory();
                ClearHistoryForm();
                LoadPartners(); // Обновляем скидку в списке партнеров
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Редактирование покупки
        private void BtnEditHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedHistory == null)
                {
                    MessageBox.Show("Выберите покупку для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbProduct.SelectedValue == null || string.IsNullOrWhiteSpace(txtQuantity.Text) || dpSaleDate.SelectedDate == null)
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(txtQuantity.Text, out double quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество продукции!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var history = db.Partners_product.Find(selectedHistory.ID);
                if (history != null)
                {
                    history.ID_Product = (int)cmbProduct.SelectedValue;
                    history.Количество_продукции = quantity;
                    history.Дата_продажи = dpSaleDate.SelectedDate;

                    db.SaveChanges();

                    MessageBox.Show("Покупка обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadHistory();
                    ClearHistoryForm();
                    LoadPartners(); // Обновляем скидку в списке партнеров
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Удаление покупки
        private void BtnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedHistory == null)
                {
                    MessageBox.Show("Выберите покупку для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы уверены, что хотите удалить эту покупку?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var history = db.Partners_product.Find(selectedHistory.ID);
                    if (history != null)
                    {
                        db.Partners_product.Remove(history);
                        db.SaveChanges();

                        MessageBox.Show("Покупка удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadHistory();
                        ClearHistoryForm();
                        LoadPartners(); // Обновляем скидку в списке партнеров
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Очистка формы истории
        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            ClearHistoryForm();
            EnableHistoryButtons(false);
        }
    }
}