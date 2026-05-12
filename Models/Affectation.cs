using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Affectation")]
public class Affectation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date d'affectation")]
    public DateTime Date_affectation { get; set; }

    // FK Professeur (1..*)
    [Required]
    [Display(Name = "Professeur")]
    public int ProfesseurId { get; set; }
    public Professeur? Professeur { get; set; }

    // FK Module (1..*)
    [Required]
    [Display(Name = "Module")]
    public int ModuleId { get; set; }
    public Module? Module { get; set; }
}
