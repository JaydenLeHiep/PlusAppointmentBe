using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.NotAvailableTimeRepo;
using PlusAppointment.Services.Interfaces.NotAvailableTimeService;

namespace PlusAppointment.Services.Implementations.NotAvailableTimeService
{
    public class NotAvailableTimeService(INotAvailableTimeRepository notAvailableTimeRepository)
        : INotAvailableTimeService
    {
        public async Task<IEnumerable<NotAvailableTimeDto?>> GetAllByBusinessIdAsync(int businessId)
        {
            var times = await notAvailableTimeRepository.GetAllByBusinessIdAsync(businessId);
            return times.Select(t => new NotAvailableTimeDto
            {
                NotAvailableTimeId = t.NotAvailableTimeId,
                StaffId = t.StaffId,
                BusinessId = t.BusinessId,
                Date = t.Date,
                From = t.From,
                To = t.To,
                Reason = t.Reason
            }).ToList();
        }
        
        public async Task<IEnumerable<NotAvailableTimeDto?>> GetAllByStaffIdAsync(int businessId, int staffId)
        {
            var times = await notAvailableTimeRepository.GetAllByStaffIdAsync(businessId, staffId);
            return times.Select(t => new NotAvailableTimeDto
            {
                NotAvailableTimeId = t.NotAvailableTimeId,
                StaffId = t.StaffId,
                BusinessId = t.BusinessId,
                Date = t.Date,
                From = t.From,
                To = t.To,
                Reason = t.Reason
            }).ToList();
        }

        public async Task<NotAvailableTimeDto?> GetByIdAsync(int businessId, int staffId, int id)
        {
            var time = await notAvailableTimeRepository.GetByIdAsync(businessId, staffId, id);
            if (time == null) return null;

            return new NotAvailableTimeDto
            {
                NotAvailableTimeId = time.NotAvailableTimeId,
                StaffId = time.StaffId,
                BusinessId = time.BusinessId,
                Date = time.Date,
                From = time.From,
                To = time.To,
                Reason = time.Reason
            };
        }

        public async Task AddNotAvailableTimeAsync(int businessId, int staffId, NotAvailableTimeDto notAvailableTimeDto)
        {
            var time = new Models.Classes.NotAvailableTime
            {
                StaffId = staffId,
                BusinessId = businessId,
                Date = notAvailableTimeDto.Date,
                From = notAvailableTimeDto.From,
                To = notAvailableTimeDto.To,
                Reason = notAvailableTimeDto.Reason
            };
            await notAvailableTimeRepository.AddAsync(time);
        }

        public async Task UpdateNotAvailableTimeAsync(int businessId, int staffId, int id, NotAvailableTimeDto notAvailableTimeDto)
        {
            var time = await notAvailableTimeRepository.GetByIdAsync(businessId, staffId, id);
            if (time == null)
            {
                throw new KeyNotFoundException("Not available time not found");
            }
            time.Date = notAvailableTimeDto.Date;
            time.From = notAvailableTimeDto.From;
            time.To = notAvailableTimeDto.To;
            time.Reason = notAvailableTimeDto.Reason;

            await notAvailableTimeRepository.UpdateAsync(time);
        }

        public async Task DeleteNotAvailableTimeAsync(int businessId, int staffId, int id)
        {
            await notAvailableTimeRepository.DeleteAsync(businessId, staffId, id);
        }
    }
}