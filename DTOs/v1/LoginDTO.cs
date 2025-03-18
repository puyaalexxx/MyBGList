using System.ComponentModel.DataAnnotations;

namespace MyBGList.DTOs.v1;

public class LoginDTO
{
    [Required] [MaxLength(255)] public string? UserName { get; set; }

    [Required] public string? Password { get; set; }
}