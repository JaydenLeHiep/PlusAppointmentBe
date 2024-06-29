using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using log4net;
using WebApplication1.Data;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectionController));

    public ConnectionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("test-connection")]
    public async Task<ActionResult<string>> TestConnection()
    {
        Logger.Info("Starting test connection method");

        try
        {
            Logger.Debug("Attempting to open a connection to the database");
            await _context.Database.OpenConnectionAsync();
            await _context.Database.CloseConnectionAsync();
            Logger.Debug("Database connection opened and closed successfully");

            Logger.Info("Database connection is successful");
            return Ok("Database connection is successful");
        }
        catch (Exception ex)
        {
            Logger.Error("Database connection failed", ex);
            return StatusCode(500, $"Database connection failed: {ex.Message}");
        }
        finally
        {
            if (_context.Database.GetDbConnection().State == System.Data.ConnectionState.Open)
            {
                Logger.Debug("Closing database connection");
                await _context.Database.CloseConnectionAsync();
            }
            Logger.Info("Test connection method completed");
        }
    }
}