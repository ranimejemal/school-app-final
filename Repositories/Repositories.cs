using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;

namespace SchoolApp.Repositories;

// ── Generic Repository Interface ──────────────────────────────────────────────
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}

// ── Generic Repository Implementation ────────────────────────────────────────
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _set     = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _set.Where(predicate).ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}

// ── Unit of Work Interface ────────────────────────────────────────────────────
public interface IUnitOfWork : IDisposable
{
    IRepository<SchoolApp.Models.Niveau>      Niveaux      { get; }
    IRepository<SchoolApp.Models.Specialite>  Specialites  { get; }
    IRepository<SchoolApp.Models.Groupe>      Groupes      { get; }
    IRepository<SchoolApp.Models.Etudiant>    Etudiants    { get; }
    IRepository<SchoolApp.Models.Professeur>  Professeurs  { get; }
    IRepository<SchoolApp.Models.Module>      Modules      { get; }
    IRepository<SchoolApp.Models.Absence>     Absences     { get; }
    IRepository<SchoolApp.Models.Examen>      Examens      { get; }
    IRepository<SchoolApp.Models.Affectation> Affectations { get; }
    Task<int> CompleteAsync();
}

// ── Unit of Work Implementation ───────────────────────────────────────────────
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<SchoolApp.Models.Niveau>      Niveaux      { get; }
    public IRepository<SchoolApp.Models.Specialite>  Specialites  { get; }
    public IRepository<SchoolApp.Models.Groupe>      Groupes      { get; }
    public IRepository<SchoolApp.Models.Etudiant>    Etudiants    { get; }
    public IRepository<SchoolApp.Models.Professeur>  Professeurs  { get; }
    public IRepository<SchoolApp.Models.Module>      Modules      { get; }
    public IRepository<SchoolApp.Models.Absence>     Absences     { get; }
    public IRepository<SchoolApp.Models.Examen>      Examens      { get; }
    public IRepository<SchoolApp.Models.Affectation> Affectations { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context     = context;
        Niveaux      = new Repository<SchoolApp.Models.Niveau>(context);
        Specialites  = new Repository<SchoolApp.Models.Specialite>(context);
        Groupes      = new Repository<SchoolApp.Models.Groupe>(context);
        Etudiants    = new Repository<SchoolApp.Models.Etudiant>(context);
        Professeurs  = new Repository<SchoolApp.Models.Professeur>(context);
        Modules      = new Repository<SchoolApp.Models.Module>(context);
        Absences     = new Repository<SchoolApp.Models.Absence>(context);
        Examens      = new Repository<SchoolApp.Models.Examen>(context);
        Affectations = new Repository<SchoolApp.Models.Affectation>(context);
    }

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
