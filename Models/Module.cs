using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Module")]
public class Module
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id_m { get; set; }

    [Required(ErrorMessage = "Le libellé est obligatoire")]
    [StringLength(150)]
    [Display(Name = "Libellé")]
    public string Libelle { get; set; } = string.Empty;

    [Required]
    [Range(1, 500, ErrorMessage = "La durée doit être entre 1 et 500 heures")]
    [Display(Name = "Durée (h)")]
    public int Duree { get; set; }

    [Required]
    [Range(1, 10, ErrorMessage = "Le coefficient doit être entre 1 et 10")]
    [Display(Name = "Coefficient")]
    public int Coef { get; set; }

    // Navigation
    public ICollection<Affectation> Affectations { get; set; } = new List<Affectation>();
    public ICollection<Examen>      Examens       { get; set; } = new List<Examen>();
}
