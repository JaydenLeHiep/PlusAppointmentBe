using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.NotAvailableTimeRepo
{
    public interface INotAvailableTimeRepository
    {
        Task<IEnumerable<NotAvailableTime>> GetAllByBusinessIdAsync(int businessId);
        Task<IEnumerable<NotAvailableTime>> GetAllByStaffIdAsync(int businessId, int staffId);
        Task<NotAvailableTime?> GetByIdAsync(int businessId, int staffId, int id);
        Task AddAsync(NotAvailableTime notAvailableTime);
        Task UpdateAsync(NotAvailableTime notAvailableTime);
        Task DeleteAsync(int businessId, int staffId, int id);
    }
}