using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SchoolApp.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(200)]
    public string? Profil { get; set; }

    public int? PersonneId { get; set; }
    public Personne? Personne { get; set; }

    /// <summary>True when account was created by admin – user must change password on first login.</summary>
    public bool MustChangePassword { get; set; }
}
