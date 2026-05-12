using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

public class Etudiant : Personne
{
    [Required(ErrorMessage = "Le CNE est obligatoire")]
    [Display(Name = "CNE")]
    public int CNE { get; set; }

    // FK Groupe
    [Required]
    [Display(Name = "Groupe")]
    public int GroupeId { get; set; }
    public Groupe? Groupe { get; set; }

    // Navigation
    public ICollection<Absence> Absences  { get; set; } = new List<Absence>();
    public ICollection<Examen>  Examens   { get; set; } = new List<Examen>();
}
