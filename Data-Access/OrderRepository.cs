
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using DatabaseContex;
using Microsoft.EntityFrameworkCore;
using ProjectControllers;
using ProjectModels;

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

    public async Task<int> GetPageCount() {
        var rows = await (   from Order in this._context.Orders
                            join Customers in this._context.Customer 
                                on Order.CustomerId equals Customers.CustomerId
                            join Users in this._context.User 
                                on Order.UserId equals Users.UserId
                            select 1
                        )
                        .ToListAsync();

        int pageCount = rows.Count / this._rowsPerPage;
        return pageCount;
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


        Customer? customerEntry = await this._context
                                        .Customer
                                        .SingleOrDefaultAsync(x => x.Name == CustomerName);
        
        User? userEntry = await this._context
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

        User user = await this.GetUser(Username);
        Customer customer = await this.GetCustomer(CustomerName);

        this._context.Orders.Add(new Orders {
            Id = guid.ToString(),
            OrderType = orderTypeValue,
            CustomerId = customer.CustomerId,
            UserId = user.UserId,
            CreatedDate = time
        });
        await this._context.SaveChangesAsync();
    }

    private async Task<User> GetUser(string Username) {
        User? user = await this._context
                                .User
                                .SingleOrDefaultAsync(User => User.Username == Username);
        
        if (user is null) {
            throw new InvalidOperationException("User does not exist.");
        }

        return user;
    }

    private async Task<Customer> GetCustomer(string CustomerName) {
            
        Customer? customer = await this._context
                                .Customer
                                .SingleOrDefaultAsync(Customer => Customer.Name == CustomerName);
        
        bool customerDoesNotExist = customer == null;
        if (customerDoesNotExist) {
            throw new InvalidOperationException("Customer does not exist.");
        }

        return customer;
    }

    public async Task<IEnumerable<PublicModels.Order>> FilterOrdersAsync(string Type, int pageNumber) {
        int type = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), Type);
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
        int type = (int) (OrderTypes) Enum.Parse(typeof(OrderTypes), typeChoice);
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
}