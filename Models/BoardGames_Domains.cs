using System.ComponentModel.DataAnnotations;

namespace MyBGList.Models;

public class BoardGames_Domains
{
    [Required]
    [Key]
    public int BoardGameId { get; set; }
    
    [Required]
    [Key]
    public int DomainId { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }

    public BoardGame? BoardGame { get; set; }
    public Domain? Domain { get; set; }
}