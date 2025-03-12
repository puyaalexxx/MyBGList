using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyBGList.Models;

[Table("BoardGames")]
public class BoardGame
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required] 
    [MaxLength(1200)] 
    public string Name { get; set; } = null!;

    [Required]
    public int? Year { get; set; }

    [Required]
    public int MinPlayers { get; set; }
    
    [Required]
    public int MaxPlayers { get; set; }
    
    [Required]
    public int Playtime { get; set; }
    
    [Required]
    public int MinAge { get; set; }
    
    [Required]
    public int UsersRated { get; set; }

    [Required]
    [Precision(4,2)]
    public int RatingAverage { get; set; }
    
    [Required]
    public int BGGRank { get; set; }
    
    [Required]
    [Precision(4,2)]
    public int ComplexityAverage { get; set; }
    
    [Required]
    public int OwnedUsers { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }
    
    [Required]
    public DateTime LastModifiedDate { get; set; }

    public ICollection<BoardGames_Domains>? BoardGames_Domains { get; set; }
    public ICollection<BoardGames_Mechanics>? BoardGames_Mechanics { get; set; }
}