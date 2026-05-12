using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models;

[Table("Examen")]
public class Examen
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date d'examen")]
    public DateTime Date_Ex { get; set; }

    [Required]
    [Range(0, 20, ErrorMessage = "La note doit être entre 0 et 20")]
    [Display(Name = "Note")]
    public float Note_Ex { get; set; }

    // FK Etudiant
    [Required]
    [Display(Name = "Étudiant")]
    public int EtudiantId { get; set; }
    public Etudiant? Etudiant { get; set; }

    // FK Module
    [Required]
    [Display(Name = "Module")]
    public int ModuleId { get; set; }
    public Module? Module { get; set; }
}
