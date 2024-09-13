using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Services.Interfaces.NotAvailableTimeService
{
    public interface INotAvailableTimeService
    {
        Task<IEnumerable<NotAvailableTimeDto?>> GetAllByBusinessIdAsync(int businessId);
        Task<IEnumerable<NotAvailableTimeDto?>> GetAllByStaffIdAsync(int businessId, int staffId);
        Task<NotAvailableTimeDto?> GetByIdAsync(int businessId, int staffId, int id);
        Task AddNotAvailableTimeAsync(int businessId, int staffId, NotAvailableTimeDto notAvailableTimeDto);
        Task UpdateNotAvailableTimeAsync(int businessId, int staffId, int id, NotAvailableTimeDto notAvailableTimeDto);
        Task DeleteNotAvailableTimeAsync(int businessId, int staffId, int id);
    }
}