using CashRegisterWebApp.Models;


namespace CashRegisterWebApp
{
    // Интерфейс для кассового аппарата
    public interface ICashRegister
    {
        void PrintReceipt(List<Product> products);
    }
}
