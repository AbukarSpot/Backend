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
}