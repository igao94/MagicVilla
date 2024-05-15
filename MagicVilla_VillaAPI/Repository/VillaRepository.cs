using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;

namespace MagicVilla_VillaAPI.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _context;

        public VillaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Villa> UpdateAsync(Villa villa)
        {
            villa.UpdateDate = DateTime.Now;
            _context.Villas.Update(villa);
            await _context.SaveChangesAsync();

            return villa;
        }
    }
}