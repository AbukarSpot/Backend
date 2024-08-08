using ProjectModels;

public interface ICustomerRepository {
    public Task AddCustomerAsync(Customer customer);
}