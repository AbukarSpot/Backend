namespace PublicModels {

    public class Order {
        public string Id { get; set; }
        public DateTime Date { get; set; }
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
            this.Date = Date;
            this.By = By;
            this.Type = Type;
            this.Customer = Customer;
        }
    }

}