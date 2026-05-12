using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context     = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        // Roles
        string[] roles = { "Admin", "Etudiant", "Professeur" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Admin
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = "admin@school.tn" };
            var res   = await userManager.CreateAsync(admin, "Admin@1234");
            if (res.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await context.Niveaux.AnyAsync()) return;

        var niv1 = new Niveau { Libelle = "1ère année" };
        var niv2 = new Niveau { Libelle = "2ème année" };
        var niv3 = new Niveau { Libelle = "3ème année" };
        context.Niveaux.AddRange(niv1, niv2, niv3);

        var spInfo = new Specialite { Libelle = "Informatique" };
        var spMath = new Specialite { Libelle = "Mathématiques" };
        var spPhys = new Specialite { Libelle = "Physique" };
        context.Specialites.AddRange(spInfo, spMath, spPhys);
        await context.SaveChangesAsync();

        var g1 = new Groupe { Libelle = "G1 - INFO 1", NiveauId = niv1.Id_ni, SpecialiteId = spInfo.Id_sp };
        var g2 = new Groupe { Libelle = "G2 - INFO 2", NiveauId = niv2.Id_ni, SpecialiteId = spInfo.Id_sp };
        var g3 = new Groupe { Libelle = "G3 - MATH 1", NiveauId = niv1.Id_ni, SpecialiteId = spMath.Id_sp };
        context.Groupes.AddRange(g1, g2, g3);

        var mAlgo    = new Module { Libelle = "Algorithmique",       Duree = 60, Coef = 4 };
        var mBD      = new Module { Libelle = "Base de données",      Duree = 45, Coef = 3 };
        var mWeb     = new Module { Libelle = "Développement Web",    Duree = 60, Coef = 4 };
        var mMath    = new Module { Libelle = "Analyse mathématique", Duree = 45, Coef = 3 };
        var mReseaux = new Module { Libelle = "Réseaux",              Duree = 45, Coef = 3 };
        context.Modules.AddRange(mAlgo, mBD, mWeb, mMath, mReseaux);
        await context.SaveChangesAsync();

        var prof1 = new Professeur { Cin=11111111, Nom="Jemal",    Prenom="Ranime",   Date_nais=new DateTime(1978,3,10),  Email="ranime.jemal@school.tn",    Tel="22000001", Date_emb=new DateTime(2005,9,1) };
        var prof2 = new Professeur { Cin=22222222, Nom="Gasmi",    Prenom="Ibtissem", Date_nais=new DateTime(1982,7,22),  Email="ibtissem.gasmi@school.tn",  Tel="22000002", Date_emb=new DateTime(2010,9,1) };
        var prof3 = new Professeur { Cin=33333300, Nom="Jenzri",   Prenom="Ibtihel",  Date_nais=new DateTime(1985,5,14),  Email="ibtihel.jenzri@school.tn",  Tel="22000003", Date_emb=new DateTime(2012,9,1) };
        context.Professeurs.AddRange(prof1, prof2, prof3);
        await context.SaveChangesAsync();

        context.Affectations.AddRange(
            new Affectation { ProfesseurId=prof1.Id, ModuleId=mAlgo.Id_m,    Date_affectation=new DateTime(2024,9,1) },
            new Affectation { ProfesseurId=prof1.Id, ModuleId=mBD.Id_m,      Date_affectation=new DateTime(2024,9,1) },
            new Affectation { ProfesseurId=prof2.Id, ModuleId=mWeb.Id_m,     Date_affectation=new DateTime(2024,9,1) },
            new Affectation { ProfesseurId=prof2.Id, ModuleId=mReseaux.Id_m, Date_affectation=new DateTime(2024,9,1) },
            new Affectation { ProfesseurId=prof3.Id, ModuleId=mMath.Id_m,    Date_affectation=new DateTime(2024,9,1) }
        );

        var etList = new List<Etudiant>
        {
            new() { Cin=33333331, CNE=1001, Nom="Ben Soussia", Prenom="Ameni",   Date_nais=new DateTime(2002,4,5),   Email="ameni.bensoussia@etud.tn", Tel="55111111", GroupeId=g1.ID_gp },
            new() { Cin=33333332, CNE=1002, Nom="Trabelsi",    Prenom="Salma",   Date_nais=new DateTime(2002,8,12),  Email="salma.trabelsi@etud.tn",   Tel="55222222", GroupeId=g1.ID_gp },
            new() { Cin=33333333, CNE=1003, Nom="Khedher",     Prenom="Yassine", Date_nais=new DateTime(2001,11,30), Email="yassine.khedher@etud.tn",  Tel="55333333", GroupeId=g2.ID_gp },
            new() { Cin=33333334, CNE=1004, Nom="Ben Amor",    Prenom="Chaima",  Date_nais=new DateTime(2002,1,18),  Email="chaima.benamor@etud.tn",   Tel="55444444", GroupeId=g2.ID_gp },
            new() { Cin=33333335, CNE=1005, Nom="Sfaxi",       Prenom="Rami",    Date_nais=new DateTime(2002,6,25),  Email="rami.sfaxi@etud.tn",       Tel="55555555", GroupeId=g3.ID_gp },
            new() { Cin=33333336, CNE=1006, Nom="Ouertani",    Prenom="Nour",    Date_nais=new DateTime(2003,3,8),   Email="nour.ouertani@etud.tn",    Tel="55666666", GroupeId=g1.ID_gp },
            new() { Cin=33333337, CNE=1007, Nom="Ben Slimane", Prenom="Aziz",    Date_nais=new DateTime(2002,9,20),  Email="aziz.benslimane@etud.tn",  Tel="55777777", GroupeId=g3.ID_gp },
        };
        context.Etudiants.AddRange(etList);
        await context.SaveChangesAsync();

        context.Absences.AddRange(
            new Absence { EtudiantId=etList[0].Id, ProfesseurId=prof1.Id, ModuleId=mAlgo.Id_m,    Date_debut=new DateTime(2024,10,5),  Date_fin=new DateTime(2024,10,6),  Justification="Maladie" },
            new Absence { EtudiantId=etList[0].Id, ProfesseurId=prof2.Id, ModuleId=mWeb.Id_m,     Date_debut=new DateTime(2024,11,12), Date_fin=new DateTime(2024,11,12), Justification=null },
            new Absence { EtudiantId=etList[1].Id, ProfesseurId=prof2.Id, ModuleId=mWeb.Id_m,     Date_debut=new DateTime(2024,10,20), Date_fin=new DateTime(2024,10,22), Justification="Voyage familial" },
            new Absence { EtudiantId=etList[2].Id, ProfesseurId=prof1.Id, ModuleId=mBD.Id_m,      Date_debut=new DateTime(2024,9,15),  Date_fin=new DateTime(2024,9,15),  Justification=null },
            new Absence { EtudiantId=etList[3].Id, ProfesseurId=prof3.Id, ModuleId=mMath.Id_m,    Date_debut=new DateTime(2024,11,3),  Date_fin=new DateTime(2024,11,4),  Justification="Raisons médicales" },
            new Absence { EtudiantId=etList[4].Id, ProfesseurId=prof3.Id, ModuleId=mMath.Id_m,    Date_debut=new DateTime(2024,10,10), Date_fin=new DateTime(2024,10,10), Justification=null },
            new Absence { EtudiantId=etList[5].Id, ProfesseurId=prof1.Id, ModuleId=mAlgo.Id_m,    Date_debut=new DateTime(2024,12,2),  Date_fin=new DateTime(2024,12,3),  Justification="Certificat médical" }
        );

        var rnd = new Random(42);
        var modules = new[] { mAlgo, mBD, mWeb, mMath, mReseaux };
        foreach (var et in etList)
            foreach (var mod in modules)
                context.Examens.Add(new Examen
                {
                    EtudiantId = et.Id,
                    ModuleId   = mod.Id_m,
                    Date_Ex    = new DateTime(2025, 1, rnd.Next(10, 28)),
                    Note_Ex    = (float)Math.Round(rnd.NextDouble() * 20, 2)
                });

        await context.SaveChangesAsync();

        foreach (var et in etList)
        {
            var login = et.Email.Split('@')[0];
            if (await userManager.FindByNameAsync(login) == null)
            {
                var user = new ApplicationUser { UserName = login, Email = et.Email, PersonneId = et.Id };
                var res  = await userManager.CreateAsync(user, "Etud@1234");
                if (res.Succeeded) await userManager.AddToRoleAsync(user, "Etudiant");
            }
        }

        foreach (var pr in new[] { prof1, prof2, prof3 })
        {
            var login = pr.Email.Split('@')[0];
            if (await userManager.FindByNameAsync(login) == null)
            {
                var user = new ApplicationUser { UserName = login, Email = pr.Email, PersonneId = pr.Id };
                var res  = await userManager.CreateAsync(user, "Prof@1234");
                if (res.Succeeded) await userManager.AddToRoleAsync(user, "Professeur");
            }
        }
    }
}
