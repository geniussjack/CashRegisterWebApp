using CashRegisterWebApp.Models;

namespace CashRegisterWebApp
{
    public interface ICashRegisterDriver
    {
        void SetCashier();
        void Sale(string product, int price, bool isCash, double quantity = 1.0, int department = 1);
        void Sale(IEnumerable<Product> products, int sum, bool isCash, int department = 1);
        void ReturnSale(string product, int price, bool isCash, double quantity = 1.0, int department = 1);
        void ReturnSale(IEnumerable<Product> products, int sum, bool isCash, int department = 1);
        void DailyReport(bool withCleaning = false);
        void Print(string stringToPrint);
        void Print(List<string> stringsToPrint);
        void PrintTerminalCheck(List<string> stringsToPrint);
        void Cut();
        void PrintCliche();
        bool Discharge();
    }
}
