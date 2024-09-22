using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.IOpeningHoursRepository
{
    public interface IOpeningHoursRepository
    {
        Task<OpeningHours?> GetByBusinessIdAsync(int businessId);
        Task AddAsync(OpeningHours openingHours);
        Task UpdateAsync(OpeningHours openingHours);
        Task DeleteAsync(int businessId);
    }
}