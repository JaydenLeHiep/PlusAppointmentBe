using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConnectionController(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        [HttpGet("test-connection")]
        public async Task<ActionResult<string>> TestConnection()
        {
            logger.Info("Starting test connection method");

            try
            {
                logger.Debug("Attempting to open a connection to the database");
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                logger.Debug("Database connection opened and closed successfully");

                logger.Info("Database connection is successful");
                return Ok("Database connection is successful");
            }
            catch (Exception ex)
            {
                logger.Error("Database connection failed", ex);
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
            finally
            {
                if (_context.Database.GetDbConnection().State == System.Data.ConnectionState.Open)
                {
                    logger.Debug("Closing database connection");
                    await _context.Database.CloseConnectionAsync();
                }
                logger.Info("Test connection method completed");
            }
        }
    }
}
