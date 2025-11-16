using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Core.DTOs;

public class ProfileDto
{
    public string? Name { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Phone]
    public string? Phone { get; set; }
    public IFormFile? Photo { get; set; }
}
