using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

public class Professeur : Personne
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date d'embauche")]
    public DateTime Date_emb { get; set; }

    // Navigation
    public ICollection<Affectation> Affectations { get; set; } = new List<Affectation>();
    public ICollection<Absence>     Absences      { get; set; } = new List<Absence>();
}
