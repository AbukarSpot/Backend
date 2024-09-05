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
            this.Date = Date.ToString("MMMM dd-yyyy");
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

}