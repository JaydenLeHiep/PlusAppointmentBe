using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.NotAvailableDateRepository
{
    public interface INotAvailableDateRepository
    {
        Task<IEnumerable<NotAvailableDate>> GetAllByStaffIdAsync(int businessId, int staffId);
        Task<NotAvailableDate?> GetByIdAsync(int businessId, int staffId, int id);
        Task AddAsync(NotAvailableDate notAvailableDate);
        Task UpdateAsync(NotAvailableDate notAvailableDate);
        Task DeleteAsync(int businessId, int staffId, int id);
        Task<IEnumerable<NotAvailableDate>> GetAllByBusinessIdAsync(int businessId);
    }
}