
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectModels {
    
    [Table("User")]
    public class User {
        [Key]
        [Column(TypeName = "varchar(36)")]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(36)")]
        public string Username { get; set; }
        
        [Column(TypeName = "varchar(36)")]
        public string? Password { get; set; }
    }
}