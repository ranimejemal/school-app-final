using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Personne")]
public abstract class Personne
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le CIN est obligatoire")]
    [Display(Name = "CIN")]
    public int Cin { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de naissance est obligatoire")]
    [DataType(DataType.Date)]
    [Display(Name = "Date de naissance")]
    public DateTime Date_nais { get; set; }

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Format email invalide")]
    [StringLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone est obligatoire")]
    [Phone]
    [StringLength(20)]
    [Display(Name = "Téléphone")]
    public string Tel { get; set; } = string.Empty;

    // FK vers ApplicationUser (0..1)
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    [NotMapped]
    public string NomComplet => $"{Prenom} {Nom}";
}
