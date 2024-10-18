using Hangfire.Dashboard;


public class IPAddressAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string[] _allowedIPs;

    public IPAddressAuthorizationFilter(params string[] allowedIPs)
    {
        _allowedIPs = allowedIPs;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        
        if (remoteIp == "::1" || remoteIp == "127.0.0.1")
        {
            return true; // Allow localhost access
        }

        // Check if the remote IP address is in the list of allowed IPs
        return _allowedIPs.Contains(remoteIp);
    }
}