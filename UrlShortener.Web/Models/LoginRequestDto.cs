using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Web.Models;

/// <summary>
/// Represents credentials submitted by the user when attempting to log in.
/// </summary>
public class LoginRequestDto
{
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}