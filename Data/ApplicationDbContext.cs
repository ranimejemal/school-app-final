using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── DbSets ────────────────────────────────────────────────────────────────
    public DbSet<Niveau>      Niveaux      { get; set; }
    public DbSet<Specialite>  Specialites  { get; set; }
    public DbSet<Groupe>      Groupes      { get; set; }
    public DbSet<Personne>    Personnes    { get; set; }
    public DbSet<Etudiant>    Etudiants    { get; set; }
    public DbSet<Professeur>  Professeurs  { get; set; }
    public DbSet<Module>      Modules      { get; set; }
    public DbSet<Absence>     Absences     { get; set; }
    public DbSet<Examen>      Examens      { get; set; }
    public DbSet<Affectation> Affectations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── TPH Inheritance (Personne → Etudiant / Professeur) ────────────────
        builder.Entity<Personne>()
            .HasDiscriminator<string>("Type")
            .HasValue<Etudiant>("Etudiant")
            .HasValue<Professeur>("Professeur");

        // ── Personne ──────────────────────────────────────────────────────────
        builder.Entity<Personne>(e =>
        {
            e.ToTable("Personne");
            e.HasKey(p => p.Id);
            e.Property(p => p.Nom).IsRequired().HasMaxLength(100);
            e.Property(p => p.Prenom).IsRequired().HasMaxLength(100);
            e.Property(p => p.Email).IsRequired().HasMaxLength(200);
            e.Property(p => p.Tel).HasMaxLength(20);
            e.HasIndex(p => p.Cin).IsUnique();
            e.HasIndex(p => p.Email).IsUnique();

            // 0..1 relation to ApplicationUser
            e.HasOne(p => p.ApplicationUser)
             .WithOne(u => u.Personne)
             .HasForeignKey<ApplicationUser>(u => u.PersonneId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Etudiant ──────────────────────────────────────────────────────────
        builder.Entity<Etudiant>(e =>
        {
            e.HasIndex(et => et.CNE).IsUnique();

            e.HasOne(et => et.Groupe)
             .WithMany(g => g.Etudiants)
             .HasForeignKey(et => et.GroupeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Professeur ────────────────────────────────────────────────────────
        builder.Entity<Professeur>(e =>
        {
            e.Property(p => p.Date_emb).IsRequired();
        });

        // ── Niveau ────────────────────────────────────────────────────────────
        builder.Entity<Niveau>(e =>
        {
            e.ToTable("Niveau");
            e.HasKey(n => n.Id_ni);
            e.Property(n => n.Libelle).IsRequired().HasMaxLength(100);
        });

        // ── Specialite ────────────────────────────────────────────────────────
        builder.Entity<Specialite>(e =>
        {
            e.ToTable("Specialite");
            e.HasKey(s => s.Id_sp);
            e.Property(s => s.Libelle).IsRequired().HasMaxLength(150);
        });

        // ── Groupe ────────────────────────────────────────────────────────────
        builder.Entity<Groupe>(e =>
        {
            e.ToTable("Groupe");
            e.HasKey(g => g.ID_gp);
            e.Property(g => g.Libelle).IsRequired().HasMaxLength(100);

            e.HasOne(g => g.Niveau)
             .WithMany(n => n.Groupes)
             .HasForeignKey(g => g.NiveauId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(g => g.Specialite)
             .WithMany(s => s.Groupes)
             .HasForeignKey(g => g.SpecialiteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Module ────────────────────────────────────────────────────────────
        builder.Entity<Module>(e =>
        {
            e.ToTable("Module");
            e.HasKey(m => m.Id_m);
            e.Property(m => m.Libelle).IsRequired().HasMaxLength(150);
            e.Property(m => m.Duree).IsRequired();
            e.Property(m => m.Coef).IsRequired();
        });

        // ── Absence ───────────────────────────────────────────────────────────
        builder.Entity<Absence>(e =>
        {
            e.ToTable("Absence");
            e.HasKey(a => a.Id);
            e.Property(a => a.Justification).HasMaxLength(500);

            e.HasOne(a => a.Etudiant)
             .WithMany(et => et.Absences)
             .HasForeignKey(a => a.EtudiantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Professeur)
             .WithMany(p => p.Absences)
             .HasForeignKey(a => a.ProfesseurId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Module)
             .WithMany()
             .HasForeignKey(a => a.ModuleId)
             .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one absence per (etudiant, professeur, module, date_debut)
            e.HasIndex(a => new { a.EtudiantId, a.ProfesseurId, a.ModuleId, a.Date_debut })
             .IsUnique()
             .HasDatabaseName("IX_Absence_UniqueSeance");
        });

        // ── Examen ────────────────────────────────────────────────────────────
        builder.Entity<Examen>(e =>
        {
            e.ToTable("Examen");
            e.HasKey(x => x.Id);
            e.Property(x => x.Note_Ex).IsRequired();

            e.HasOne(x => x.Etudiant)
             .WithMany(et => et.Examens)
             .HasForeignKey(x => x.EtudiantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Module)
             .WithMany(m => m.Examens)
             .HasForeignKey(x => x.ModuleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Affectation ───────────────────────────────────────────────────────
        builder.Entity<Affectation>(e =>
        {
            e.ToTable("Affectation");
            e.HasKey(a => a.Id);

            e.HasOne(a => a.Professeur)
             .WithMany(p => p.Affectations)
             .HasForeignKey(a => a.ProfesseurId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Module)
             .WithMany(m => m.Affectations)
             .HasForeignKey(a => a.ModuleId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
