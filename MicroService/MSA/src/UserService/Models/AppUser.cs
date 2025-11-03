// src/UserService/Models/AppUser.cs
using Microsoft.AspNetCore.Identity;

namespace UserService.Models;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}