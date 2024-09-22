using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.IOpeningHoursRepository;
using PlusAppointment.Services.Interfaces.IOpeningHoursService;

namespace PlusAppointment.Services.Implementations.OpeningHoursService
{
    public class OpeningHoursService(IOpeningHoursRepository openingHoursRepository) : IOpeningHoursService
    {
        public async Task<OpeningHours?> GetByBusinessIdAsync(int businessId)
        {
            return await openingHoursRepository.GetByBusinessIdAsync(businessId);
        }

        public async Task AddOpeningHoursAsync(OpeningHours openingHours)
        {
            var existingOpeningHours = await openingHoursRepository.GetByBusinessIdAsync(openingHours.BusinessId);
            if (existingOpeningHours != null)
            {
                throw new InvalidOperationException("Opening hours already exist for this business. Consider updating instead.");
            }

            await openingHoursRepository.AddAsync(openingHours);
        }

        public async Task UpdateOpeningHoursAsync(OpeningHours openingHours)
        {
            var existingOpeningHours = await openingHoursRepository.GetByBusinessIdAsync(openingHours.BusinessId);
            if (existingOpeningHours == null)
            {
                throw new KeyNotFoundException("Opening hours not found for this business.");
            }

            existingOpeningHours.MondayOpeningTime = openingHours.MondayOpeningTime;
            existingOpeningHours.MondayClosingTime = openingHours.MondayClosingTime;
            existingOpeningHours.TuesdayOpeningTime = openingHours.TuesdayOpeningTime;
            existingOpeningHours.TuesdayClosingTime = openingHours.TuesdayClosingTime;
            existingOpeningHours.WednesdayOpeningTime = openingHours.WednesdayOpeningTime;
            existingOpeningHours.WednesdayClosingTime = openingHours.WednesdayClosingTime;
            existingOpeningHours.ThursdayOpeningTime = openingHours.ThursdayOpeningTime;
            existingOpeningHours.ThursdayClosingTime = openingHours.ThursdayClosingTime;
            existingOpeningHours.FridayOpeningTime = openingHours.FridayOpeningTime;
            existingOpeningHours.FridayClosingTime = openingHours.FridayClosingTime;
            existingOpeningHours.SaturdayOpeningTime = openingHours.SaturdayOpeningTime;
            existingOpeningHours.SaturdayClosingTime = openingHours.SaturdayClosingTime;
            existingOpeningHours.SundayOpeningTime = openingHours.SundayOpeningTime;
            existingOpeningHours.SundayClosingTime = openingHours.SundayClosingTime;
            existingOpeningHours.MinimumAdvanceBookingMinutes = openingHours.MinimumAdvanceBookingMinutes;

            await openingHoursRepository.UpdateAsync(existingOpeningHours);
        }

        public async Task DeleteOpeningHoursAsync(int businessId)
        {
            var existingOpeningHours = await openingHoursRepository.GetByBusinessIdAsync(businessId);
            if (existingOpeningHours == null)
            {
                throw new KeyNotFoundException("Opening hours not found for this business.");
            }

            await openingHoursRepository.DeleteAsync(businessId);
        }
    }
}