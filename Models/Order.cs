using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectModels {
    
    [Table("Orders")]
    public class Orders {
        [Key]
        [Column(TypeName = "varchar(36)")]
        public string Id { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int OrderType { get; set; }

        [Required]
        [Column(TypeName = "varchar(36)")]
        public string CustomerId { get; set; }

        [Required]
        [Column(TypeName = "varchar(36)")]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }
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
    public enum OrderPaginagionCount {
        [Display(Name = "")]
        All = 0,
        Type = 1,
        Customer = 2,
        CustomerAndType = 3
    }
}