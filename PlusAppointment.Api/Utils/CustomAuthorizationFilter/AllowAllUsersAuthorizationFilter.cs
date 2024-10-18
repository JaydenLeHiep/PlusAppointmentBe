using Hangfire.Dashboard;

namespace PlusAppointment.Utils.CustomAuthorizationFilter;

public class AllowAllUsersAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow access to everyone
        return true;
    }
}