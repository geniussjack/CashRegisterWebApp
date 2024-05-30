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
                await context.Response.WriteAsync($"������ ������ JSON: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync($"������ ��������� ������: {ex.Message}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500; // Internal Server Error
                await context.Response.WriteAsync($"����������� ������: {ex.Message}");
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
                    throw new ArgumentException("�������� ������ �� ����� ���� ������.");
                }
                if (product.Amount <= 0)
                {
                    throw new ArgumentException($"���������� ������ '{product.Name}' ������ ���� ������ ����.");
                }
                if (product.Price <= 0)
                {
                    throw new ArgumentException($"���� ������ '{product.Name}' ������ ���� ������ ����.");
                }

                products.Add(product);
            }
            return products;
        }
    }
}