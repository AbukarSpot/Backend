
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
            try {
                await _customerRepo.AddCustomerAsync(new Customer {
                    CustomerId = Guid.NewGuid().ToString(),
                    Name = Name
                });
                return Ok();
            }
            catch(InvalidOperationException ex) {
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status208AlreadyReported };
            }

        }

        [HttpGet("/All")]
        public async Task<IActionResult> GetAllCustomers() 
        { 
            try {
                var customers = await this._customerRepo.GetAllCustomersAsync();
                return Ok(customers);
            } 
            catch (Exception error) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            } 
        }
    }
}
