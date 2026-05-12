using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Specialite")]
public class Specialite
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id_sp { get; set; }

    [Required(ErrorMessage = "Le libellé est obligatoire")]
    [StringLength(150)]
    [Display(Name = "Libellé")]
    public string Libelle { get; set; } = string.Empty;

    // Navigation
    public ICollection<Groupe> Groupes { get; set; } = new List<Groupe>();
}
