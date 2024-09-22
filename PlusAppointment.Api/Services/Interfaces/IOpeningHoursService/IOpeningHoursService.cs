using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.IOpeningHoursService
{
    public interface IOpeningHoursService
    {
        // Method to get the opening hours for a specific business
        Task<OpeningHours?> GetByBusinessIdAsync(int businessId);

        // Method to add opening hours for a business
        Task AddOpeningHoursAsync(OpeningHours openingHours);

        // Method to update existing opening hours for a business
        Task UpdateOpeningHoursAsync(OpeningHours openingHours);

        // Method to delete opening hours for a business
        Task DeleteOpeningHoursAsync(int businessId);
    }
}