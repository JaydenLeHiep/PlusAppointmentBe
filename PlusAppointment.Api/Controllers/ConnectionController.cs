using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using log4net;
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
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectionController));

        public ConnectionController(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
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

        [HttpGet("test-redis-connection")]
        public ActionResult<string> TestRedisConnection()
        {
            Logger.Info("Starting Redis connection test");

            try
            {
                var db = _redisHelper.GetDatabase();
                Logger.Debug("Attempting to ping Redis server");
                var pingResult = db.Ping();

                Logger.Info($"Redis connection successful: Ping={pingResult.TotalMilliseconds}ms");
                return Ok($"Redis connection is successful: Ping={pingResult.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Logger.Error("Redis connection failed", ex);
                return StatusCode(500, $"Redis connection failed: {ex.Message}");
            }
        }
    }
}
