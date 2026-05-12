using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Absence")]
public class Absence
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Date de la séance (sert aussi d'identifiant de séance pour la règle anti-doublon).</summary>
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date de séance")]
    public DateTime Date_debut { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date fin")]
    public DateTime Date_fin { get; set; }

    [StringLength(500)]
    [Display(Name = "Justification")]
    public string? Justification { get; set; }

    // FK Etudiant
    [Required]
    [Display(Name = "Étudiant")]
    public int EtudiantId { get; set; }
    public Etudiant? Etudiant { get; set; }

    // FK Professeur
    [Required]
    [Display(Name = "Professeur")]
    public int ProfesseurId { get; set; }
    public Professeur? Professeur { get; set; }

    // FK Module (séance)
    [Required]
    [Display(Name = "Module / Séance")]
    public int ModuleId { get; set; }
    public Module? Module { get; set; }

    [NotMapped]
    public int NombreJours => (int)(Date_fin - Date_debut).TotalDays + 1;

    [NotMapped]
    public bool EstJustifiee => !string.IsNullOrWhiteSpace(Justification);
}
