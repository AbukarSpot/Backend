using ProjectModels;

public interface IOrderRepository {
    public Task<IEnumerable<PublicModels.Order>> GetAllOrdersAsync(int page);
    public Task UpdateOrderAsync(
        string OrderId,
        DateTime CreatedDate,
        string Username,
        string OrderType,
        string CustomerName
    );
    public Task RemoveOrderAsync(List<string> OrderIds);
    public Task CreateOrderAsync(
        string Type,
        string CustomerName,
        string Username
    );

    public Task<IEnumerable<PublicModels.Order>> FilterOrdersAsync(string Type, int pageNumber);
    public Task<IEnumerable<PublicModels.Order>> GetSpecificCustomerOrdersAsync(string customerName, int pageNumber);
    public Task<IEnumerable<PublicModels.Order>> GetSpecificCustomerAndTypeOrdersAsync(
        string customerName, 
        string typeChoice, 
        int pageNumber
    );
    public Task<int> GetPageCount();
    public Task<int> GetPageTypeCount(OrderPaginationRequest request);
    public Task<int> GetPageCustomerCount(OrderPaginationRequest request);
    public Task<int> GetPageTypeAndCustomerCount(OrderPaginationRequest request);
    public Task CreateMultipleOrdersAsync(
        List<OrderRequest> requests
    );

    public Task<List<OrderFreqResult>> GetOrdersFrequencyByDateAsync(
        string startDate,
        string stopDate
    );

    public Task<List<OrderDistributionResult>> GetOrdersDistributionByDateAsync(
        string startDate,
        string stopDate
    );
}