using System.ComponentModel.DataAnnotations;

namespace SchoolApp.Models.ViewModels;

// ── Login ─────────────────────────────────────────────────────────────────────
public class LoginViewModel
{
    [Required(ErrorMessage = "L'identifiant est obligatoire")]
    [Display(Name = "Identifiant")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Se souvenir de moi")]
    public bool RememberMe { get; set; }
}

// ── Absence search ────────────────────────────────────────────────────────────
public class AbsenceSearchViewModel
{
    public DateTime? DateDebut     { get; set; }
    public DateTime? DateFin       { get; set; }
    public string?   Justification { get; set; }
    public int?      EtudiantId    { get; set; }
    public int?      ModuleId      { get; set; }
    public List<Absence> Results   { get; set; } = new();
}

// ── Statistics ────────────────────────────────────────────────────────────────
public class StatisticsViewModel
{
    public int TotalEtudiants   { get; set; }
    public int TotalProfesseurs { get; set; }
    public int TotalModules     { get; set; }
    public int TotalGroupes     { get; set; }
    public int TotalAbsences    { get; set; }
    public int TotalExamens     { get; set; }

    // Absences par groupe
    public List<string> GroupeLabels      { get; set; } = new();
    public List<int>    AbsencesParGroupe { get; set; } = new();

    // Moyenne des notes par module
    public List<string> ModuleLabels  { get; set; } = new();
    public List<double> MoyennesNotes { get; set; } = new();

    // Répartition absences justifiées / non justifiées
    public int AbsencesJustifiees    { get; set; }
    public int AbsencesNonJustifiees { get; set; }
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
public class DashboardViewModel
{
    public string          UserName   { get; set; } = string.Empty;
    public string          Role       { get; set; } = string.Empty;
    public List<Absence>   Absences   { get; set; } = new();
    public List<Examen>    Examens    { get; set; } = new();
    public StatisticsViewModel Stats  { get; set; } = new();
}

// ── 2FA ───────────────────────────────────────────────────────────────────────
public class TwoFactorSetupViewModel
{
    public string SharedKey  { get; set; } = string.Empty;
    public string QrCodeUrl  { get; set; } = string.Empty;
    public bool   IsEnabled  { get; set; }
    [Required(ErrorMessage = "Le code est obligatoire")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code doit contenir 6 chiffres")]
    [Display(Name = "Code de vérification")]
    public string Code { get; set; } = string.Empty;
}

public class TwoFactorVerifyViewModel
{
    [Required(ErrorMessage = "Le code est obligatoire")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Le code doit contenir 6 chiffres")]
    [Display(Name = "Code de vérification")]
    public string Code { get; set; } = string.Empty;
    public bool RememberMachine { get; set; }
}

// ── Force Change Password ──────────────────────────────────────────────────────
public class ForceChangePasswordViewModel
{
    [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Au moins 6 caractères")]
    [Display(Name = "Nouveau mot de passe")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation est obligatoire")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
