
using System.Linq.Expressions;
using DatabaseContex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectModels;

namespace ProjectControllers {


    [Route("/[controller]")]
    [ApiController]
    public class CustomerController: ControllerBase {

        private readonly ICustomerRepository _customerRepo;

        public CustomerController(ICustomerRepository customerRepo)
        {
            _customerRepo = customerRepo;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateCustomer(
            string Name
        ) 
        {
            await _customerRepo.AddCustomerAsync(new Customer {
                CustomerId = Guid.NewGuid().ToString(),
                Name = Name
            });
            return Ok();
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllCustomers() 
        { 
            var customers = await this._customerRepo.GetAllCustomersAsync();
            return Ok(customers);
        }
    }
}
