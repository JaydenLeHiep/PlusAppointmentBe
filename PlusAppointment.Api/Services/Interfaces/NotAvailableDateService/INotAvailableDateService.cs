using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Services.Interfaces.NotAvailableDateService
{
    public interface INotAvailableDateService
    {
        Task<IEnumerable<NotAvailableDateDto?>> GetAllByStaffIdAsync(int businessId, int staffId);
        Task<NotAvailableDateDto?> GetByIdAsync(int businessId, int staffId, int id);
        Task AddNotAvailableDateAsync(int businessId, int staffId, NotAvailableDateDto notAvailableDateDto);
        Task UpdateNotAvailableDateAsync(int businessId, int staffId, int id, NotAvailableDateDto notAvailableDateDto);
        Task DeleteNotAvailableDateAsync(int businessId, int staffId, int id);
    }
}