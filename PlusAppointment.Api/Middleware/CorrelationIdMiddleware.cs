using log4net;

namespace PlusAppointment.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private static readonly ILog Log = LogManager.GetLogger(typeof(CorrelationIdMiddleware));

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check if the request contains a Correlation ID, otherwise generate a new one
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }

        // Add the Correlation ID to the response headers for clients to track
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Store the Correlation ID in HttpContext.Items to access it later
        context.Items["CorrelationId"] = correlationId;

        // Log the start of the request
        Log.Info($"Starting request with Correlation ID: {correlationId}");

        // Continue to the next middleware or action
        await _next(context);

        // Log the end of the request
        Log.Info($"Completed request with Correlation ID: {correlationId}");
    }
}