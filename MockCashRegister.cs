using CashRegisterWebApp.Models;



namespace CashRegisterWebApp
{
    // Имитация кассового аппарата
    public class MockCashRegister : ICashRegister
    {
        public void PrintReceipt(List<Product> products)
        {
            double totalAmount = 0;

            // Определяем максимальные длины строк для названия товара и цены
            int maxNameLength = 0;
            int maxPriceLength = 0;
            int maxTotalLength = 0;

            foreach (var product in products)
            {
                if (product.Name.Length > maxNameLength)
                {
                    maxNameLength = product.Name.Length;
                }
                var priceLength = product.Price.ToString("C").Length;
                if (priceLength > maxPriceLength)
                {
                    maxPriceLength = priceLength;
                }
                var totalLength = (product.Amount * product.Price).ToString("C").Length;
                if (totalLength > maxTotalLength)
                {
                    maxTotalLength = totalLength;
                }
            }

            Console.WriteLine("Чек (кассовый аппарат):");
            foreach (var product in products)
            {
                double productTotal = product.Amount * product.Price;
                // Форматируем строку с выравниванием
                string formattedLine = $"{product.Name.PadRight(maxNameLength)} * {product.Amount} * {product.Price.ToString("C").PadLeft(maxPriceLength)} = {productTotal.ToString("C").PadLeft(maxTotalLength)}";
                Console.WriteLine(formattedLine);
                totalAmount += productTotal;
            }
            Console.WriteLine(new string('-', maxNameLength + maxPriceLength + maxTotalLength + 10));
            Console.WriteLine($"{"Итоговая сумма:".PadRight(maxNameLength + maxPriceLength + 6)}{totalAmount.ToString("C").PadLeft(maxTotalLength)}");

        }
    }
}
