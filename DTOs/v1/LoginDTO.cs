using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;

namespace MyBGList.DTOs.v1;

public class LoginDTO
{
    [Required] [MaxLength(255)] 
    [CustomKeyValue("x-test-1", "value 1")]
    [CustomKeyValue("x-test-2", "value 2")]
    public string? UserName { get; set; }

    [Required] public string? Password { get; set; }
}