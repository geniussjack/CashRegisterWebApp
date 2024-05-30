using CashRegisterWebApp.Models;

namespace CashRegisterWebApp
{
    public class ReceiptPrinter
    {
        private List<Product> _products;

        // Конструктор по умолчанию
        public ReceiptPrinter()
        {
            _products = new List<Product>();
        }

        // Свойство для установки или получения списка продуктов
        public List<Product> Products
        {
            get { return _products; }
            set { _products = value; }
        }

        // Метод для добавления продукта в список
        public void AddProduct(Product product)
        {
            _products.Add(product);
        }

        // Метод для печати чека
        public void PrintReceipt()
        {
            decimal totalAmount = 0;
            Console.WriteLine("Чек:");
            foreach (var product in _products)
            {
                decimal productTotal = product.Amount * product.Price;
                Console.WriteLine($"{product.Name} * {product.Amount} = {productTotal:C}");
                totalAmount += productTotal;
            }
            Console.WriteLine("----------------------------");
            Console.WriteLine($"Итоговая сумма: {totalAmount:C}");
        }
    }
}