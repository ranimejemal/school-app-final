using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Groupe")]
public class Groupe
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID_gp { get; set; }

    [Required(ErrorMessage = "Le libellé est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Libellé")]
    public string Libelle { get; set; } = string.Empty;

    // FK Niveau (1..1)
    [Required]
    [Display(Name = "Niveau")]
    public int NiveauId { get; set; }
    public Niveau? Niveau { get; set; }

    // FK Specialite (1..1)
    [Required]
    [Display(Name = "Spécialité")]
    public int SpecialiteId { get; set; }
    public Specialite? Specialite { get; set; }

    // Navigation
    public ICollection<Etudiant> Etudiants { get; set; } = new List<Etudiant>();
}
