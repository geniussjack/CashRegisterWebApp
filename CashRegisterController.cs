using Microsoft.AspNetCore.Mvc;

namespace CashRegisterWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CashRegisterController : ControllerBase
    {
        private readonly ICashRegisterDriver _cashRegisterDriver;

        public CashRegisterController(ICashRegisterDriver cashRegisterDriver)
        {
            _cashRegisterDriver = cashRegisterDriver;
        }
    }
}