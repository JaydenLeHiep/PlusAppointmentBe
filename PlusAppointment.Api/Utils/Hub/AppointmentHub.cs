using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Models.Classes;

namespace PlusAppointment.Utils.Hub;

public class AppointmentHub: Microsoft.AspNetCore.SignalR.Hub
{
    // This method can be called by clients
    public async Task SendAppointmentUpdate(Appointment appointment)
    {
        await Clients.All.SendAsync("ReceiveAppointmentUpdate", new 
        {
            message = "A new appointment has been booked.",
            appointment = appointment
        });
    }
    
    public async Task SendAppointmentDelete(int appointmentId)
    {
        // Send a message to all connected clients
        await Clients.All.SendAsync("ReceiveAppointmentDeleted", appointmentId);
    }

    public async Task SendAppointmentStatusChange(int appointmentId, string status)
    {
        // Send a message to all connected clients
        await Clients.All.SendAsync("ReceiveAppointmentStatusChanged", new { AppointmentId = appointmentId, Status = status });
    }
    
    public async Task SendNotificationUpdate(string message)
    {
        // Send a message to all connected clients
        await Clients.All.SendAsync("ReceiveNotificationUpdate", message);
    }
    
    public async Task SendCustomerUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveCustomerUpdate", message);
    }
    public async Task SendServiceUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveServiceUpdate", message);
    }
    public async Task SendStaffUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveStaffUpdate", message);
    }

}