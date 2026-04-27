using System;
using System.Linq;
using Xunit;

namespace proectiq.Tests
{
    // Класс с тестами для расчета скидок
    public class DiscountCalculationTests
    {
        // Метод для расчета скидки (копия из MainWindow)
        private string GetDiscountString(double totalSum)
        {
            if (totalSum < 10000) return "0%";
            if (totalSum < 50000) return "5%";
            if (totalSum < 300000) return "10%";
            return "15%";
        }

        // Тест 1: Проверка правильности расчета скидки для разных сумм
        [Theory]
        [InlineData(0, "0%")]
        [InlineData(5000, "0%")]
        [InlineData(9999.99, "0%")]
        [InlineData(10000, "5%")]
        [InlineData(25000, "5%")]
        [InlineData(49999.99, "5%")]
        [InlineData(50000, "10%")]
        [InlineData(150000, "10%")]
        [InlineData(299999.99, "10%")]
        [InlineData(300000, "15%")]
        [InlineData(1000000, "15%")]
        public void Discount_ShouldBeCorrect_ForDifferentAmounts(double totalSum, string expectedDiscount)
        {
            // Act
            string actualDiscount = GetDiscountString(totalSum);

            // Assert
            Assert.Equal(expectedDiscount, actualDiscount);
        }
    }

    // Класс с тестами для валидации данных партнера
    public class PartnerValidationTests
    {
        // Тест 2: Проверка, что наименование партнера не может быть пустым
        [Fact]
        public void PartnerName_ShouldNotBeEmptyOrWhitespace()
        {
            // Arrange
            var invalidNames = new[] { "", " ", "   ", null };

            // Act & Assert
            foreach (var name in invalidNames)
            {
                bool isValid = !string.IsNullOrWhiteSpace(name);
                Assert.False(isValid, $"Имя '{name ?? "null"}' должно считаться невалидным");
            }
        }

        // Тест 3: Проверка, что рейтинг партнера может быть от 0 до 10
        [Theory]
        [InlineData(0, true)]
        [InlineData(5, true)]
        [InlineData(10, true)]
        [InlineData(-1, false)]
        [InlineData(11, false)]
        [InlineData(15, false)]
        public void PartnerRating_ShouldBeBetween0And10(double rating, bool expectedIsValid)
        {
            // Act
            bool isValid = rating >= 0 && rating <= 10;

            // Assert
            Assert.Equal(expectedIsValid, isValid);
        }
    }

    // Класс с тестами для проверки истории покупок
    public class PurchaseHistoryTests
    {
        // Тест 4: Проверка, что количество продукции должно быть положительным
        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(0.001, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(-100, false)]
        public void ProductQuantity_ShouldBePositive(double quantity, bool expectedIsValid)
        {
            // Act
            bool isValid = quantity > 0;

            // Assert
            Assert.Equal(expectedIsValid, isValid);
        }

        // Тест 5: Проверка, что дата продажи не может быть в будущем
        [Fact]
        public void SaleDate_ShouldNotBeInFuture()
        {
            // Arrange
            var pastDate = DateTime.Now.AddDays(-1);
            var currentDate = DateTime.Now;
            var futureDate = DateTime.Now.AddDays(1);

            // Act & Assert
            Assert.True(pastDate <= DateTime.Now, "Дата в прошлом валидна");
            Assert.True(currentDate <= DateTime.Now, "Текущая дата валидна");
            Assert.False(futureDate <= DateTime.Now, "Дата в будущем не валидна");
        }
    }

    // Дополнительные тесты для комплексной проверки
    public class IntegrationLogicTests
    {
        private string GetDiscountString(double totalSum)
        {
            if (totalSum < 10000) return "0%";
            if (totalSum < 50000) return "5%";
            if (totalSum < 300000) return "10%";
            return "15%";
        }

        // Тест 6: Проверка расчета конечной стоимости со скидкой
        [Theory]
        [InlineData(5000, 10000, 10000)] // без скидки
        [InlineData(10000, 10000, 9500)] // 5% скидка
        [InlineData(50000, 10000, 9000)] // 10% скидка
        [InlineData(300000, 10000, 8500)] // 15% скидка
        public void FinalPrice_ShouldApplyCorrectDiscount(double totalPurchases, double basePrice, double expectedPrice)
        {
            // Arrange
            string discountString = GetDiscountString(totalPurchases);
            double discountPercent = double.Parse(discountString.TrimEnd('%')) / 100.0;

            // Act
            double finalPrice = basePrice * (1 - discountPercent);

            // Assert
            Assert.Equal(expectedPrice, finalPrice, 2); // С точностью до 2 знаков
        }
    }

    // Тесты для проверки форматов данных
    public class DataFormatTests
    {
        // Тест 7: Проверка формата телефона
        [Theory]
        [InlineData("493 123 45 67", true)]
        [InlineData("987 123 56 66", true)]
        [InlineData("+7 123 456 78 90", true)]
        [InlineData("12345", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void PhoneNumber_ShouldHaveValidFormat(string phone, bool expectedIsValid)
        {
            // Act
            bool hasDigits = !string.IsNullOrWhiteSpace(phone) && phone.Any(char.IsDigit);

            // Assert
            Assert.Equal(expectedIsValid, hasDigits);
        }

        // Тест 8: Проверка формата email
        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("name@domain.ru", true)]
        [InlineData("test@test", true)]
        [InlineData("invalid-email", false)]
        [InlineData("@domain.com", false)]
        [InlineData("user@", false)]
        [InlineData("", false)]
        public void Email_ShouldContainAtSymbol(string email, bool expectedIsValid)
        {
            // Act
            bool containsAt = !string.IsNullOrWhiteSpace(email) && email.Contains("@");

            // Assert
            Assert.Equal(expectedIsValid, containsAt);
        }
    }
}