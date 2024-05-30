namespace CashRegisterWebApp
{
    public class CashRegisterException : Exception
    {
        private readonly Serilog.ILogger _logger;
        public CashRegisterException(string message) : base(message)
        {
            _logger.Error($"Ошибка кассы - {message}");
        }
    }
}
