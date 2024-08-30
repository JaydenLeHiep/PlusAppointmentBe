using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.NotAvailableDateRepository;
using PlusAppointment.Services.Interfaces.NotAvailableDateService;

namespace PlusAppointment.Services.Implementations.NotAvailableDate
{
    public class NotAvailableDateService : INotAvailableDateService
    {
        private readonly INotAvailableDateRepository _notAvailableDateRepository;

        public NotAvailableDateService(INotAvailableDateRepository notAvailableDateRepository)
        {
            _notAvailableDateRepository = notAvailableDateRepository;
        }

        public async Task<IEnumerable<NotAvailableDateDto?>> GetAllByStaffIdAsync(int businessId, int staffId)
        {
            var dates = await _notAvailableDateRepository.GetAllByStaffIdAsync(businessId, staffId);
            return dates.Select(d => new NotAvailableDateDto
            {
                NotAvailableDateId = d.NotAvailableDateId,
                StaffId = d.StaffId,
                BusinessId = d.BusinessId,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Reason = d.Reason
            }).ToList();
        }

        public async Task<NotAvailableDateDto?> GetByIdAsync(int businessId, int staffId, int id)
        {
            var date = await _notAvailableDateRepository.GetByIdAsync(businessId, staffId, id);
            if (date == null) return null;

            return new NotAvailableDateDto
            {
                NotAvailableDateId = date.NotAvailableDateId,
                StaffId = date.StaffId,
                BusinessId = date.BusinessId,
                StartDate = date.StartDate,
                EndDate = date.EndDate,
                Reason = date.Reason
            };
        }

        public async Task AddNotAvailableDateAsync(int businessId, int staffId, NotAvailableDateDto notAvailableDateDto)
        {
            var date = new Models.Classes.NotAvailableDate
            {
                StaffId = staffId,
                BusinessId = businessId,
                StartDate = notAvailableDateDto.StartDate,
                EndDate = notAvailableDateDto.EndDate,
                Reason = notAvailableDateDto.Reason
            };
            await _notAvailableDateRepository.AddAsync(date);
        }

        public async Task UpdateNotAvailableDateAsync(int businessId, int staffId, int id, NotAvailableDateDto notAvailableDateDto)
        {
            var date = await _notAvailableDateRepository.GetByIdAsync(businessId, staffId, id);
            if (date == null)
            {
                throw new KeyNotFoundException("Not available date not found");
            }
            date.StartDate = notAvailableDateDto.StartDate;
            date.EndDate = notAvailableDateDto.EndDate;
            date.Reason = notAvailableDateDto.Reason;

            await _notAvailableDateRepository.UpdateAsync(date);
        }

        public async Task DeleteNotAvailableDateAsync(int businessId, int staffId, int id)
        {
            await _notAvailableDateRepository.DeleteAsync(businessId, staffId, id);
        }
    }
}