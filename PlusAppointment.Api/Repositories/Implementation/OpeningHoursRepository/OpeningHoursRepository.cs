using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.IOpeningHoursRepository;

namespace PlusAppointment.Repositories.Implementation.OpeningHoursRepository
{
    public class OpeningHoursRepository(ApplicationDbContext context) : IOpeningHoursRepository
    {
        public async Task<OpeningHours?> GetByBusinessIdAsync(int businessId)
        {
            var openingHours = await context.OpeningHours
                .FirstOrDefaultAsync(oh => oh.BusinessId == businessId);

            return openingHours;
        }

        public async Task AddAsync(OpeningHours openingHours)
        {
            await context.OpeningHours.AddAsync(openingHours);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OpeningHours openingHours)
        {
            context.OpeningHours.Update(openingHours);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int businessId)
        {
            var openingHours = await GetByBusinessIdAsync(businessId);
            if (openingHours != null)
            {
                context.OpeningHours.Remove(openingHours);
                await context.SaveChangesAsync();
            }
        }
        
    }
}