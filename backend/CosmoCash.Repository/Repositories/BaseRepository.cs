using CosmoCash.Repository.Context;

namespace CosmoCash.Repository.Repositories;

public abstract class BaseRepository
{
  protected readonly CosmoCashDbContext _context;

  protected BaseRepository(CosmoCashDbContext context) => _context = context;
}
