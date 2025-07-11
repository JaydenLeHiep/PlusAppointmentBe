namespace PlusAppointment.Models.Classes
{
    public class OpeningHours
    {
        public int Id { get; set; }
        public int BusinessId { get; set; } // Foreign key to link with your Business table
        
        public Business.Business? Business { get; set; }

        // TimeSpan properties for each day of the week
        public TimeSpan MondayOpeningTime { get; set; }
        public TimeSpan MondayClosingTime { get; set; }
        
        public TimeSpan TuesdayOpeningTime { get; set; }
        public TimeSpan TuesdayClosingTime { get; set; }
        
        public TimeSpan WednesdayOpeningTime { get; set; }
        public TimeSpan WednesdayClosingTime { get; set; }
        
        public TimeSpan ThursdayOpeningTime { get; set; }
        public TimeSpan ThursdayClosingTime { get; set; }
        
        public TimeSpan FridayOpeningTime { get; set; }
        public TimeSpan FridayClosingTime { get; set; }
        
        public TimeSpan SaturdayOpeningTime { get; set; }
        public TimeSpan SaturdayClosingTime { get; set; }
        
        public TimeSpan SundayOpeningTime { get; set; }
        public TimeSpan SundayClosingTime { get; set; }

        public int MinimumAdvanceBookingMinutes { get; set; } // Number of hours required in advance for same-day booking
    }
}