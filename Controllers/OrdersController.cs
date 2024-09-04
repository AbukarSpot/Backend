
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using DatabaseContex;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ProjectModels;
using PublicModels;

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

        [HttpPatch("")]
        public async Task<IActionResult> UpdateOrder(
            [FromQuery] UpdateOrderRequest request 
        ) {

            Object orderTypeChoice = request.orderType;
            bool invalidOrderType = Enum.IsDefined(typeof(OrderTypes), orderTypeChoice) == false;
            if (invalidOrderType) {
                return BadRequest("Invalid Order Type.");
            }

            await this._orderRepository.UpdateOrderAsync(
                request.orderId,
                request.createdDate,
                request.username,
                request.orderType,
                request.customerName
            );

            return Ok();
        }

        [HttpDelete("")]
        public async Task<IActionResult> RemoveOrder(
            [FromBody] List<string> OrderIds
        ) {
            Console.WriteLine($"ids: ");
            foreach(string id in OrderIds) {
                Console.WriteLine($"\t{id}");
            }
            
            await this._orderRepository.RemoveOrderAsync(OrderIds);
            return Ok(200);
        }
        
        [HttpGet("{pageNumber}")]
        public async Task<IActionResult> GetAllOrders(int pageNumber) {
            Console.WriteLine($"Parsing page: {pageNumber}");
            pageNumber -= 1;
            var allOrders = await this._orderRepository.GetAllOrdersAsync(pageNumber);
            return Ok(allOrders);
        }

        
        [HttpPost("")]
        public async Task<IActionResult> CreateOrder(
            OrderRequest request
        ) 
        {
            await this._orderRepository.CreateOrderAsync(
                request.Type,
                request.CustomerName,
                request.Username
            );
            return Ok();
        }

        [HttpGet("count")]
        public async Task<IActionResult> CountOrders(
            [FromQuery] OrderPaginationRequest request
        ) {
            
            Console.WriteLine($"criteria: {request.criteria}");
            int count = 0;
            if (request.criteria == OrderPaginagionCount.All) {
                count = await this._orderRepository.GetPageCount();
            }
            else if (request.criteria == OrderPaginagionCount.Type) {
                count = await this._orderRepository.GetPageTypeCount(request);
            }
            else if (request.criteria == OrderPaginagionCount.Customer) {
                count = await this._orderRepository.GetPageCustomerCount(request);
            }
            else if (request.criteria == OrderPaginagionCount.CustomerAndType) {
                count = await this._orderRepository.GetPageTypeAndCustomerCount(request);
            }
            return Ok(count);
        }

        [HttpGet("/filter/type/{type}/{page}")]
        public async Task<IActionResult> GetMatchingOrders(
            string type,
            int page
        ) {
            page -= 1;
            var orders = await this._orderRepository.FilterOrdersAsync(type, page);
            return Ok(orders);
        }

        [HttpGet("/filter/customer/{customer}/{page}")]
        public async Task<IActionResult> GetMatchingCutomerOrders(
            string customer,
            int page
        ) {
            try {
                page -= 1;
                var orders = await this._orderRepository.GetSpecificCustomerOrdersAsync(customer, page);
                if (orders.Count() < 1) {
                    return BadRequest($"{customer} is not registered as a customer.");
                }

                return Ok(orders);
            }
            catch (ArgumentException error) {
                return BadRequest("Invalid order status choice.");
            }
        }

        [HttpGet("/filter/type/customer/{type}/{customer}/{page}")]
        public async Task<IActionResult> GetMatchingCutomeraAndTypeOrders(
            string customer,
            string type,
            int page
        ) {
            page -= 1;
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
