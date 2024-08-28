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
        public string Type { get; set; }
        public string CustomerName { get; set; }
        public string Username { get; set; }
    }

    public class OrderPaginationRequest {
        public OrderPaginagionCount criteria { get; set; }
        public string? customerName { get; set; }
        public string? type { get; set; }
    }

    public enum OrderPaginagionCount {
        [Display(Name = "")]
        All = 0,
        Type = 1,
        Customer = 2,
        CustomerAndType = 3
    }
}