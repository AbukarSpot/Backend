
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using DatabaseContex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Data.SqlClient;
using ProjectControllers;
using ProjectModels;
using PublicModels;

public class OrderRepository: IOrderRepository {

    private readonly ProjectContext _context;
    private readonly int _rowsPerPage = 5;
    public OrderRepository(ProjectContext context) {
        this._context = context;
    }

    private string GetOrderType(int OrderType) {
        OrderTypes orderEnumValue = (OrderTypes) Enum.Parse(
            typeof(OrderTypes), 
            OrderType.ToString()
        );

        return orderEnumValue.ToString();
    }

    public async Task<List<OrderDistributionResult>> GetOrdersDistributionByDateAsync(
        string startDate,
        string stopDate
    ) {
        DateTime t1 = DateTime.Parse(startDate);
        DateTime t2 = DateTime.Parse(stopDate);

        // Context is out of sync, wrote raw sql
        var payload = await _context
                            .Orders
                            .Where(order => (
                                order.CreatedDate >= t1 && 
                                order.CreatedDate <= t2
                            ))
                            .ToListAsync();
        
        Dictionary<int, List<bool>> distributionMap = new Dictionary<int, List<bool>>(); 
        payload
            .ForEach(order => {
                Console.WriteLine($"Type: {this.GetOrderType(order.OrderType)}");
                if (distributionMap.ContainsKey(order.OrderType)) {
                    distributionMap[order.OrderType].Add(true);
                } else {
                    distributionMap[order.OrderType] = new List<bool>() { true };
                }
            });
        
        return distributionMap.Select(e => new OrderDistributionResult() {
            Type = this.GetOrderType(e.Key),
            OrderCount = e.Value.Count()
        }).ToList();

        // return orderDistribution;
    }

    public async Task<List<OrderFreqResult>> GetOrdersFrequencyByDateAsync(
        string startDate,
        string stopDate
    ) {
        DateTime t1 = DateTime.Parse(startDate);
        DateTime t2 = DateTime.Parse(stopDate);

        var payload = await _context.Orders.Select(order => new {
            order.CreatedDate,
            order.OrderType
        })
        .Where(day => day.CreatedDate >= t1 && day.CreatedDate <= t2)
        .ToListAsync();
        
        var orderFreq = payload
                            .GroupBy(order => order.CreatedDate.ToString("MM-dd-yyyy"))
                            .ToDictionary(key => key.Key, value => new OrderFreqResult {
                                OrderCount = value.Count(),
                                DateOf = value.FirstOrDefault()?.CreatedDate.ToString("MM-dd-yyyy")
                            })
                            .ToList();

        return orderFreq.Select(e => e.Value).ToList();
    }
    public async Task<IEnumerable<PublicModels.Order>> GetAllOrdersAsync(int pageNumber) {
        var allOrders = (   from Order in this._context.Orders
                            join Customers in this._context.Customer 
                                on Order.CustomerId equals Customers.CustomerId
                            join Users in this._context.User 
                                on Order.UserId equals Users.UserId
                            select new {
                                Order.Id,
                                Order.CreatedDate,
                                Users.Username,
                                Order.OrderType,
                                Customers.Name
                            }
                        )
                        .AsEnumerable()
                        .Skip(this._rowsPerPage * pageNumber)
                        .Take(this._rowsPerPage)
                        .OrderByDescending(order => order.CreatedDate)
                        .Select(order => {
                            string orderType = this.GetOrderType(order.OrderType);
                            return  new PublicModels.Order(
                                        Id: order.Id,
                                        Date: order.CreatedDate,
                                        By: order.Username,
                                        Type: orderType,
                                        Customer: order.Name
                                    );
                        });
        
        return allOrders;
    }
    public async Task UpdateOrderAsync(
        string OrderId,
        DateTime CreatedDate,
        string Username,
        string OrderType,
        string CustomerName
    ) {
        Object orderTypeChoice = OrderType;
        bool invalidOrderType = Enum.IsDefined(typeof(OrderTypes), orderTypeChoice) == false;
        if (invalidOrderType) {
            throw new InvalidOperationException($"{OrderType} is an invalid order type.");
        }

        Orders? entry = await this._context
                                        .Orders
                                        .SingleOrDefaultAsync(Order => Order.Id == OrderId);
        bool orderEntryDoesNotExist = entry == null;
        if (orderEntryDoesNotExist) {
            throw new InvalidOperationException("Order does not exist.");
        }


        ProjectModels.Customer? customerEntry = await this._context
                                        .Customer
                                        .SingleOrDefaultAsync(x => x.Name == CustomerName);
        
        ProjectModels.User? userEntry = await this._context
                                .User
                                .SingleOrDefaultAsync(x => x.Username == Username);
        
        
        if (customerEntry is null) {
            throw new InvalidOperationException("Customer does not exist.");
        }

        if (userEntry is null) {
            throw new InvalidOperationException("User does not exist.");
        }

        entry.CustomerId = customerEntry.CustomerId;
        entry.UserId = userEntry.UserId;
        int orderType = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), OrderType.ToString());
        entry.OrderType = orderType;
        entry.CreatedDate = CreatedDate;        
        
        // int rowsUpdated = this._context
        //                         .Orders
        //                         .Where(order => order.Id == OrderId)
        //                         .ExecuteUpdate(
        //                             setter => setter
        //                                         .SetProperty(o => o.CustomerId, customerEntry.CustomerId)
        //                                         .SetProperty(o => o.UserId, userEntry.UserId)
        //                                         .SetProperty(o => o.OrderType,orderType)
        //                                         .SetProperty(o => o.CreatedDate, CreatedDate)
        //                         );
        // if (rowsUpdated < 1) {
        //     throw new InvalidOperationException("Updated 0 rows.");
        // } 
        await this._context.SaveChangesAsync();
    }
    
    public async Task RemoveOrderAsync(List<string> OrderIds) {        
        (
            from order in this._context.Orders
            where OrderIds.Contains(order.Id)
            select order
        )
        .ExecuteDelete();
        await this._context.SaveChangesAsync();
    }

    public async Task CreateMultipleOrdersAsync(
        List<OrderRequest> requests
    ) {
        requests.ForEach((req) => {
            this.CreateOrderAsync(
                req.type,
                req.customerName,
                req.username
            ).Wait();
        });
    }

    public async Task CreateOrderAsync(
        string Type,
        string CustomerName,
        string Username
    ) {
        Object orderType = Type;
        bool invalidOrderType = Enum.IsDefined(typeof(OrderTypes), orderType) == false;
        if (invalidOrderType) {
            throw new InvalidOperationException("Invalid Order Type.");
        }

        Guid guid = Guid.NewGuid();
        DateTime time = DateTime.Now;
        OrderTypes orderTypeOption;
        Enum.TryParse(Type, out orderTypeOption);
        int orderTypeValue = (int) orderTypeOption;

        ProjectModels.User user = await this.GetUser(Username);
        ProjectModels.Customer customer = await this.GetCustomer(CustomerName);

        this._context.Orders.Add(new Orders {
            Id = guid.ToString(),
            OrderType = orderTypeValue,
            CustomerId = customer.CustomerId,
            UserId = user.UserId,
            CreatedDate = time
        });
        await this._context.SaveChangesAsync();
    }

    private async Task<ProjectModels.User> GetUser(string Username) {
        ProjectModels.User? user = await this._context
                                .User
                                .SingleOrDefaultAsync(User => User.Username == Username);
        
        if (user is null) {
            throw new InvalidOperationException("User does not exist.");
        }

        return user;
    }

    private async Task<ProjectModels.Customer> GetCustomer(string CustomerName) {
            
        ProjectModels.Customer? customer = await this._context
                                .Customer
                                .SingleOrDefaultAsync(Customer => Customer.Name == CustomerName);
        
        bool customerDoesNotExist = customer == null;
        if (customerDoesNotExist) {
            throw new InvalidOperationException("Customer does not exist.");
        }

        return customer;
    }

    public async Task<IEnumerable<PublicModels.Order>> FilterOrdersAsync(string Type, int pageNumber) {
        int type;
        try {
            type = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), Type);
        } 
        catch (ArgumentException error) {
            throw new ArgumentException("Invalid order status choice.");
        }

        var orders = (
            from Order in this._context.Orders
            join Customers in this._context.Customer 
                on Order.CustomerId equals Customers.CustomerId
            join Users in this._context.User 
                on Order.UserId equals Users.UserId
            where Order.OrderType == type
            select new {
                Order.Id,
                Order.CreatedDate,
                Users.Username,
                Order.OrderType,
                Customers.Name
            }
        )
        .AsEnumerable()
        .Skip(this._rowsPerPage * pageNumber)
        .Take(this._rowsPerPage)
        .Select(order => {
            string orderType = this.GetOrderType(order.OrderType);
            return  new PublicModels.Order(
                        Id: order.Id,
                        Date: order.CreatedDate,
                        By: order.Username,
                        Type: orderType,
                        Customer: order.Name
                    );
        });

        return orders;
    }

    public async Task<IEnumerable<PublicModels.Order>> GetSpecificCustomerOrdersAsync(
        string customerName,
        int pageNumber 
    ) {
        var orders = (
            from Order in this._context.Orders
            join Customers in this._context.Customer 
                on Order.CustomerId equals Customers.CustomerId
            join Users in this._context.User 
                on Order.UserId equals Users.UserId
            where Customers.Name == customerName
            select new {
                Order.Id,
                Order.CreatedDate,
                Users.Username,
                Order.OrderType,
                Customers.Name
            }
        )
        .AsEnumerable()
        .Skip(this._rowsPerPage * pageNumber)
        .Take(this._rowsPerPage)
        .Select(order => {
            string orderType = this.GetOrderType(order.OrderType);
            return  new PublicModels.Order(
                        Id: order.Id,
                        Date: order.CreatedDate,
                        By: order.Username,
                        Type: orderType,
                        Customer: order.Name
                    );
        });

        return orders;
    }

    public async Task<IEnumerable<PublicModels.Order>> GetSpecificCustomerAndTypeOrdersAsync(
        string customerName, 
        string typeChoice, 
        int pageNumber 
    ) {
        int type;
        try { 
            type = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), typeChoice);
        } catch (ArgumentException error) {
            throw new ArgumentException("Invalid order status choice.");
        }
        
        var orders = (
            from Order in this._context.Orders
            join Customers in this._context.Customer 
                on Order.CustomerId equals Customers.CustomerId
            join Users in this._context.User 
                on Order.UserId equals Users.UserId
            where 
                Customers.Name == customerName &&
                Order.OrderType == type
            select new {
                Order.Id,
                Order.CreatedDate,
                Users.Username,
                Order.OrderType,
                Customers.Name
            }
        )
        .AsEnumerable()
        .Skip(this._rowsPerPage * pageNumber)
        .Take(this._rowsPerPage)
        .Select(order => {
            string orderType = this.GetOrderType(order.OrderType);
            return  new PublicModels.Order(
                        Id: order.Id,
                        Date: order.CreatedDate,
                        By: order.Username,
                        Type: orderType,
                        Customer: order.Name
                    );
        });

        return orders;
    } 

    public async Task<int> GetPageCount() {
        var rows = await (   from Order in this._context.Orders
                            join Customers in this._context.Customer 
                                on Order.CustomerId equals Customers.CustomerId
                            join Users in this._context.User 
                                on Order.UserId equals Users.UserId
                            select 1
                        )
                        .ToListAsync();

        int pageCount = ( rows.Count / this._rowsPerPage ) + 1;
        return pageCount;
    }

    public async Task<int> GetPageTypeCount(OrderPaginationRequest request) {
        if (request.type == null) {
            throw new ArgumentNullException("Must provide a type name.");
        }

        int type = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), request.type);
        var rows = await (   from Order in this._context.Orders
                            join Customers in this._context.Customer 
                                on Order.CustomerId equals Customers.CustomerId
                            join Users in this._context.User 
                                on Order.UserId equals Users.UserId
                            where Order.OrderType == type
                            select 1
                        )
                        .ToListAsync();

        int pageCount = ( rows.Count / this._rowsPerPage ) + 1;
        return pageCount;
    }

    public async Task<int> GetPageCustomerCount(OrderPaginationRequest request) {
        if (request.customerName == null) {
            throw new ArgumentNullException("Must provide a customer name.");
        }

        var rows = await (   from Order in this._context.Orders
                            join Customers in this._context.Customer 
                                on Order.CustomerId equals Customers.CustomerId
                            join Users in this._context.User 
                                on Order.UserId equals Users.UserId
                            where Customers.Name == request.customerName
                            select 1
                        )
                        .ToListAsync();

        int pageCount = ( rows.Count / this._rowsPerPage ) + 1;
        return pageCount;
    }

    public async Task<int> GetPageTypeAndCustomerCount(OrderPaginationRequest request) {
        if (request.customerName == null || request.type == null) {
            throw new ArgumentNullException("Must provide customer name and type.");
        }
        int type = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), request.type);
        var rows = await (   from Order in this._context.Orders
                            join Customers in this._context.Customer 
                                on Order.CustomerId equals Customers.CustomerId
                            join Users in this._context.User 
                                on Order.UserId equals Users.UserId
                            where 
                                Customers.Name == request.customerName &&
                                Order.OrderType == type
                            select 1
                        )
                        .ToListAsync();

        int pageCount = ( rows.Count / this._rowsPerPage ) + 1;
        return pageCount;
    }

}