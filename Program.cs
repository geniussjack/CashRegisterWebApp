using CashRegisterWebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace CashRegisterWebApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 });

        static async Task GetRoot(HttpContext context)
        {
            await context.Response.WriteAsync("Cash Register API is running.");
        }

        static async Task PrintReceipt(HttpContext context)
        {
            try
            {
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                var products = ValidateAndDeserializeProducts(requestBody);

                ICashRegister cashRegister = new MockCashRegister();
                cashRegister.PrintReceipt(products);

                await context.Response.WriteAsync("Receipt printed successfully.");
            }
            catch (JsonReaderException ex)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync($"Ошибка чтения JSON: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync($"Ошибка валидации данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500; // Internal Server Error
                await context.Response.WriteAsync($"Неизвестная ошибка: {ex.Message}");
            }
        }

        static List<Product> ValidateAndDeserializeProducts(string json)
        {
            var products = new List<Product>();

            var jsonArray = JArray.Parse(json);
            foreach (var item in jsonArray)
            {
                var product = item.ToObject<Product>();

                if (string.IsNullOrEmpty(product.Name))
                {
                    throw new ArgumentException("Название товара не может быть пустым.");
                }
                if (product.Amount <= 0)
                {
                    throw new ArgumentException($"Количество товара '{product.Name}' должно быть больше нуля.");
                }
                if (product.Price <= 0)
                {
                    throw new ArgumentException($"Цена товара '{product.Name}' должна быть больше нуля.");
                }

                products.Add(product);
            }
            return products;
        }
    }
}