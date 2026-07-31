using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("LoginActivity")]
public class LoginActivity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(20)]
    public string UserType { get; set; } = string.Empty;

    public DateTime LoginTime { get; set; }

    [MaxLength(100)]
    public string SessionInfo { get; set; } = string.Empty;
}
