using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Niveau")]
public class Niveau
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id_ni { get; set; }

    [Required(ErrorMessage = "Le libellé est obligatoire")]
    [StringLength(100, ErrorMessage = "Maximum 100 caractères")]
    [Display(Name = "Libellé")]
    public string Libelle { get; set; } = string.Empty;

    // Navigation
    public ICollection<Groupe> Groupes { get; set; } = new List<Groupe>();
}
