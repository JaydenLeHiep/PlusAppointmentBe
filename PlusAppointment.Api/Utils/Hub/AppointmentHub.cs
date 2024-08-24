using Microsoft.AspNetCore.SignalR;
namespace PlusAppointment.Utils.Hub;

public class AppointmentHub: Microsoft.AspNetCore.SignalR.Hub
{
    // This method can be called by clients
    public async Task SendAppointmentUpdate(string message)
    {
        // Send a message to all connected clients
        await Clients.All.SendAsync("ReceiveAppointmentUpdate", message);
    }
    public async Task SendCustomerUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveCustomerUpdate", message);
    }

}