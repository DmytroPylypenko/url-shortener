using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Domain.Entities;

/// <summary>
/// Represents an application user who can authenticate and create short URLs.
/// </summary>
public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]  
    [MaxLength(20)]
    public string Role { get; set; } = "User";
}