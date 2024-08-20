using ProjectModels;

public interface ICustomerRepository {
    public Task AddCustomerAsync(Customer customer);
    public Task<List<PublicModels.Customer>> GetAllCustomersAsync();
}