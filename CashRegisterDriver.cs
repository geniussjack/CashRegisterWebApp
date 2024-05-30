using CashRegisterWebApp.Models;
using DrvFRLib;
using Serilog;

namespace CashRegisterWebApp
{
    public class CashRegisterDriver : ICashRegisterDriver
    {
        private readonly Serilog.ILogger _logger;
        private DrvFR _frDriver;
        private readonly int _password;

        public CashRegisterDriver()
        {
            _logger = Log.ForContext<CashRegisterDriver>();
            _frDriver = new DrvFR();
            // _password = Convert.ToInt32(Config.Get("cashboxNumb", "1"));
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Connect();
                    return;
                }
                catch
                {
                    Thread.Sleep(300);
                }
            }
            Connect();
        }
        private void Connect()
        {
            _frDriver.ConnectionType = 6;
            _frDriver.ProtocolType = 0;
            // _frDriver.IPAddress = Config.Get("cashboxIP", "192.168.137.111");
            _frDriver.UseIPAddress = true;
            _frDriver.TCPPort = 7778;
            _frDriver.Timeout = 5000;
            _frDriver.Password = _password;
            int result = _frDriver.Connect();
            string res = _frDriver.ResultCodeDescription;
            if (result != 0)
            {
                throw new CashRegisterException($"Соединение с кассой - {result}: {res}");
            }
        }

        public bool Discharge()
        {
            bool opened = false;
            switch (_frDriver.ECRMode)
            {
                case 0:
                case 2:
                    opened = true;
                    break;
                case 3:
                    _frDriver.Password = 30;
                    _frDriver.FNCloseSession();
                    _frDriver.Password = _password;
                    if (_frDriver.ResultCode != 0)
                        throw new CashRegisterException($"Ошибка кассы: {_frDriver.ResultCodeDescription}");
                    break;
                case 4:
                    OpenSession();
                    break;
                case 8:
                    _frDriver.CancelCheck();
                    break;
                default:
                    throw new CashRegisterException($"Режим кассы: {_frDriver.ECRMode}");
            }
            int result = _frDriver.Connect();
            return opened;
        }

        public void SetCashier()
        {
            _frDriver.Password = 30;
            _frDriver.TableNumber = 2;
            _frDriver.FieldNumber = 2;
            // _frDriver.RowNumber = Convert.ToInt32(Config.Get("cashboxNumb", "1"));
            if (_frDriver.GetFieldStruct() == 0)
            {
                // _frDriver.ValueOfFieldString = Config.Get("cashier");
                if (_frDriver.WriteTable() != 0)
                {
                    throw new InvalidOperationException("Не удалось установить имя кассира. Отказ записи.");
                }
            }
            else
            {
                throw new CashRegisterException("Не удалось установить имя кассира.");
            }
        }

        private void LogErrorCode()
        {
            _logger.Information($"Код кассы: {_frDriver.ResultCodeDescription}");
            _frDriver.ContinuePrint();
        }
        public void Sale(string product, int price, bool isCash, double quantity = 1.0, int department = 1)
        {
            decimal convertedPrice = price / 100m;
            ProcessTransaction(product, convertedPrice, isCash, quantity, department, () => _frDriver.Sale());
        }

        public void Sale(IEnumerable<Product> products, int sum, bool isCash, int department = 1)
        {
            WaitForPrint();
            foreach (Product product in products)
            {
                decimal convertedPrice = product.Price / 100m;
                ProcessTransaction(product.Name, convertedPrice, isCash, product.Amount, department, () => _frDriver.Sale());
            }

            CloseCheck(sum / 100m, isCash);
        }

        public void ReturnSale(string product, int price, bool isCash, double quantity = 1.0, int department = 1)
        {
            decimal convertedPrice = price / 100m;
            ProcessTransaction(product, convertedPrice, isCash, quantity, department, () => _frDriver.ReturnSale());
        }

        private void ProcessTransaction(string product, decimal price, bool isCash, double quantity, int department, Action transaction)
        {
            WaitForPrint();
            _frDriver.Quantity = quantity;
            _frDriver.Price = price;
            _frDriver.Department = department;
            _frDriver.StringForPrinting = product;
            _frDriver.Tax1 = 1;
            _frDriver.Tax2 = 1;
            _frDriver.Tax3 = 1;
            _frDriver.Tax4 = 1;
            try
            {
                transaction.Invoke();
            }
            catch
            {
                CheckPrintFail();
            }
            if (_frDriver.ResultCode != 0)
            {
                decimal sum = (decimal)quantity * price;
                CloseCheck(sum, isCash);
            }
            else
            {
                throw new CashRegisterException(_frDriver.ResultCodeDescription);
            }
        }
        private void CheckPrintFail()
        {
            _logger.Information($"Код кассы: {_frDriver.ResultCodeDescription}");
            _frDriver.ContinuePrint();
        }
        public void ReturnSale(IEnumerable<Product> products, int sum, bool isCash, int department = 1)
        {
            WaitForPrint();
            foreach (Product product in products)
            {
                decimal convertedPrice = product.Price / 100m;
                _frDriver.Quantity = product.Amount;
                _frDriver.Price = convertedPrice;
                _frDriver.Department = department;
                _frDriver.StringForPrinting = product.Name;
                _frDriver.Tax1 = 0;
                _frDriver.Tax2 = 0;
                _frDriver.Tax3 = 0;
                _frDriver.Tax4 = 0;
                try
                {
                    _frDriver.ReturnSale();
                }
                catch
                {
                    CheckPrintFail();
                }
            }

            if (_frDriver.ResultCode == 0)
            {
                CloseCheck(sum / 100m, isCash);
            }
            else
            {
                throw new CashRegisterException(_frDriver.ResultCodeDescription);
            }
        }

        private void CloseCheck(decimal sum, bool isCash)
        {
            WaitForPrint();
            _frDriver.Summ1 = isCash ? sum : decimal.Zero;
            _frDriver.Summ2 = decimal.Zero;
            _frDriver.Summ3 = decimal.Zero;
            _frDriver.Summ4 = !isCash ? sum : decimal.Zero;
            _frDriver.DiscountOnCheck = 0.0;
            _frDriver.Tax1 = 0;
            _frDriver.Tax2 = 0;
            _frDriver.Tax3 = 0;
            _frDriver.Tax4 = 0;
            _frDriver.StringForPrinting = "===================";
            try
            {
                _frDriver.CloseCheck();
            }
            catch
            {
                CheckPrintFail();
            }
            if (_frDriver.ResultCode != 0)
            {
                throw new CashRegisterException(_frDriver.ResultCodeDescription);
            }
        }

        private void OpenSession()
        {
            _frDriver.Password = 30;
            _frDriver.OpenSession();
            _frDriver.Password = _password;
        }

        public void PrintCliche()
        {
            _frDriver.PrintCliche();
        }

        public void DailyReport(bool withCleaning = false)
        {
            WaitForPrint();
            if (_frDriver.ECRMode == 4)
            {
                throw new CashRegisterException("Смена не открыта.");
            }
            _frDriver.Password = 30;
            try
            {
                _frDriver.PrintDepartmentReport();
            }
            catch
            {
                CheckPrintFail();
            }
            WaitForPrint();
            if (withCleaning)
            {
                _frDriver.PrintReportWithCleaning();
            }
            else
            {
                _frDriver.PrintReportWithoutCleaning();
            }
            if (_frDriver.ResultCode != 0)
            {
                throw new CashRegisterException($"Код {_frDriver.ResultCode}: {_frDriver.ResultCodeDescription}");
            }
        }

        private void PrintString(string stringToPrint)
        {
            try
            {
                WaitForPrint();
                _frDriver.StringForPrinting = stringToPrint;
                _frDriver.DelayedPrint = false;
                _frDriver.PrintString();
            }
            catch
            {
                CheckPrintFail();
            }
        }

        public void Print(string stringToPrint)
        {
            try
            {
                _frDriver.Password = _password;
                _frDriver.UseReceiptRibbon = true;
                _frDriver.UseJournalRibbon = false;
                PrintString(stringToPrint);
                _frDriver.StringQuantity = 4;
                _frDriver.FeedDocument();
            }
            catch
            {
                CheckPrintFail();
            }
        }
        public void PrintAndCut(string stringToPrint)
        {
            try
            {
                PreparePrint();
                PrintString(stringToPrint);
                FeedAndCutDocument();
            }
            catch
            {
                CheckPrintFail();
            }
        }

        public void Print(List<string> stringsToPrint)
        {
            try
            {
                PreparePrint();
                foreach (string stringToPrint in stringsToPrint)
                {
                    PrintString(stringToPrint);
                }
                FeedAndCutDocument();
            }
            catch
            {
                CheckPrintFail();
            }
        }

        public void PrintTerminalCheck(List<string> stringsToPrint)
        {
            try
            {
                Print(stringsToPrint);
                CutCheckAndPrintCliche();
            }
            catch
            {
                CheckPrintFail();
            }
        }

        public void Cut()
        {
            try
            {
                WaitForPrint();
                _frDriver.Password = _password;
                _frDriver.CutType = true;
                _frDriver.FeedAfterCut = false;
                _frDriver.FeedLineCount = 2;
                _frDriver.CutCheck();
            }
            catch
            {
                CheckPrintFail();
            }
        }

        private void PreparePrint()
        {
            _frDriver.Password = _password;
            _frDriver.UseReceiptRibbon = true;
            _frDriver.UseJournalRibbon = false;
        }

        private void FeedAndCutDocument()
        {
            _frDriver.StringQuantity = 4;
            _frDriver.FeedDocument();
        }

        private void CutCheckAndPrintCliche()
        {
            _frDriver.CutType = true;
            _frDriver.FeedAfterCut = false;
            _frDriver.FeedLineCount = 2;
            _frDriver.CutCheck();
            _frDriver.PrintCliche();
        }

        private void WaitForPrint()
        {
            for (int i = 0; i < 50; i++)
            {
                _frDriver.GetECRStatus();
                int[] acceptableStatuses = new[] { 2, 3, 4, 7, 8, 9 };
                int mode = _frDriver.ECRMode;
                int edMode = _frDriver.ECRAdvancedMode;

                if (acceptableStatuses.Contains(mode) && edMode == 0)
                {
                    return;
                }
                Thread.Sleep(500);
            }
            throw new Exception("Не удалось дождаться окончания печати");
        }

        public void Dispose()
        {
            _frDriver.Disconnect();
            _frDriver = null;
            GC.Collect();
        }
    }
}