using DatabaseContex;
using Microsoft.EntityFrameworkCore;
using ProjectModels;

public class CustomerRepository : ICustomerRepository
{
    private readonly ProjectContext _context;

    public CustomerRepository(ProjectContext context)
    {
        _context = context;
    }

    public async Task AddCustomerAsync(Customer payload)
    {
        var customer = await this._context.Customer.SingleOrDefaultAsync(c => c.Name == payload.Name);
        if(customer is not null) {
            throw new InvalidOperationException($"{payload.Name} already exists.");
        }
        this._context.Customer.Add(payload);
        await this._context.SaveChangesAsync();
    }

    public async Task<List<PublicModels.Customer>> GetAllCustomersAsync() {
        var customers = await (
            from Customer in this._context.Customer
            select new {
                Customer.CustomerId,
                Customer.Name
            }
        )
        .Select(x => new PublicModels.Customer() {
            CustomerId = x.CustomerId,
            Name = x.Name
        })
        .ToListAsync();

        return customers;
    }
}