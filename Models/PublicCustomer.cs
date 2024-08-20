using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicModels {

    [Table("Customer")]
    public class Customer {
        [Key]
        [Required]
        [Column(TypeName = "varchar(36)")]
        public string CustomerId { get; set; }

        [Required]
        [Column(TypeName = "varchar(36)")]
        public string? Name { get; set; }
    }
}