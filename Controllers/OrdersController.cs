
using System.Linq.Expressions;
using DatabaseContex;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ProjectModels;

/*
    server: sqlserverhostv.database.windows.net
    username: adminUsername
    password: adminPass1
*/
namespace ProjectControllers {

    [Route("/[controller]")]
    [ApiController]
    public class OrdersController: ControllerBase {

        private readonly ProjectContext _context;
        private readonly IOrderRepository _orderRepository;
        public OrdersController(ProjectContext context, IOrderRepository orderRepository)
        {
            this._context = context;
            this._orderRepository = orderRepository;
        }

        [HttpPatch("/{orderId}")]
        public async Task<IActionResult> UpdateOrder(
            string orderId,
            DateTime CreatedDate,
            string Username,
            string OrderType,
            string CustomerName
        ) {

            Object orderTypeChoice = OrderType;
            bool invalidOrderType = Enum.IsDefined(typeof(OrderTypes), orderTypeChoice) == false;
            if (invalidOrderType) {
                return BadRequest("Invalid Order Type.");
            }

            try {
                await this._orderRepository.UpdateOrderAsync(
                    orderId,
                    CreatedDate,
                    Username,
                    OrderType,
                    CustomerName
                );

                return Ok();                    
            } 
            catch (InvalidOperationException error) {
                return BadRequest(error.Message);
            }
            catch (Exception error) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("")]
        public async Task<IActionResult> RemoveOrder(
            [FromBody] List<string> OrderIds
        ) {
            Console.WriteLine($"ids: ");
            foreach(string id in OrderIds) {
                Console.WriteLine($"\t{id}");
            }
            try {
                    await this._orderRepository.RemoveOrderAsync(OrderIds);
                    return Ok(200);
            } catch (Exception error) {
                return StatusCode(500);
            }
        }
        
        [HttpGet("")]
        public async Task<IActionResult> GetAllOrders(int pageNumber) {
            try {
                var allOrders = await this._orderRepository.GetAllOrdersAsync(pageNumber);
                return Ok(allOrders);
            } 
            catch (Exception error) {
                return StatusCode(500);
            }
        }

        
        [HttpPost("")]
        public async Task<IActionResult> CreateOrder(
            OrderRequest request
        ) 
        {
            try {
                await this._orderRepository.CreateOrderAsync(
                    request.Type,
                    request.CustomerName,
                    request.Username
                );
                return Ok();
            }
            catch (InvalidOperationException error) {
                return BadRequest(error.Message);
            }
            catch (Exception error) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/count")]
        public async Task<IActionResult> CountOrders() {
            
            try {
                int count = await this._orderRepository.GetPageCount();
                return Ok(count);
            } catch (Exception error) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/filter/type/{type}")]
        public async Task<IActionResult> GetMatchingOrders(
            string type,
            int page
        ) {
            try {
                var orders = await this._orderRepository.FilterOrdersAsync(type, page);
                return Ok(orders);
            }
            catch (ArgumentException error) {
                return BadRequest("Invalid order status choice.");
            } 
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/filter/customer/{customer}")]
        public async Task<IActionResult> GetMatchingCutomerOrders(
            string customer,
            int page
        ) {
            try {
                var orders = await this._orderRepository.GetSpecificCustomerOrdersAsync(customer, page);
                if (orders.Count() < 1) {
                    return BadRequest($"{customer} is not registered as a customer.");
                }

                return Ok(orders);
            }
            catch (ArgumentException error) {
                return BadRequest("Invalid order status choice.");
            } 
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/filter/type/customer/{type}/{customer}")]
        public async Task<IActionResult> GetMatchingCutomeraAndTypeOrders(
            string customer,
            string type,
            int page
        ) {
            try {
                var orders = await this._orderRepository.GetSpecificCustomerAndTypeOrdersAsync(
                    customer, 
                    type, 
                    page
                );

                if (orders.Count() < 1) {
                    return BadRequest($"{customer} is not registered as a customer.");
                }
                
                return Ok(orders);
            }
            catch (ArgumentException error) {
                return BadRequest("Invalid order status choice.");
            } 
            catch {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }


    [Flags]
    public enum OrderTypes {
        Standard = 1,
        SaleOrder = 2,
        PurchaseOrder = 4,
        TransferOrder = 8,
        ReturnOrder = 16
    }
}
