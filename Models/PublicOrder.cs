using ProjectModels;
namespace PublicModels {

    public class Order {
        public string Id { get; set; }
        public string Date { get; set; }
        public string By { get; set; }
        public string Type { get; set; }
        public string Customer { get; set; }

        public Order() {}
        
        public Order(
            string Id,
            DateTime Date,
            string By,
            string Type,
            string Customer
        ) {
            this.Id = Id;
            this.Date = Date.ToString("MMM dd-yyyy");
            this.By = By;
            this.Type = Type;
            this.Customer = Customer;
        }
    }

    public class UpdateOrderRequest {
        public string orderId { get; set; }
        public DateTime createdDate { get; set; }
        public string username { get; set; }
        public string orderType { get; set; }
        public string customerName { get; set; }
    }

    public class OrderRequest {
        public string type { get; set; } = String.Empty;
        public string customerName { get; set; } = String.Empty;
        public string username { get; set; } = String.Empty;
    }

    public class BulkOrder {
        public List<OrderRequest>? Orders { get; set; }
    }

    public class OrderPaginationRequest {
        public OrderPaginagionCount criteria { get; set; }
        public string? customerName { get; set; }
        public string? type { get; set; }
    }

    public class OrderFreqResult {
        public string? DateOf { get; set; }
        public string Type { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderDistributionResult {
        public string? DateOf { get; set; }
        public string Type { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderAnalyticsRequest {
        public string startDate {get; set;} = String.Empty;
        public string stopDate {get; set;} = String.Empty;
    }
}