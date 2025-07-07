using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;

namespace PlusAppointment.CronJobs.DeleteAppointmentsLastMonthJob;

public class DeleteAppointmentsLastMonthJob
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public DeleteAppointmentsLastMonthJob(IAppointmentWriteRepository appointmentWriteRepository)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
    }
    
    public async Task ExecuteAsync()
    {
        try
        {
            var isDeletingAppointmentsLastMonth = await _appointmentWriteRepository.DeleteAppointmentsBefore(DateTime.UtcNow.AddMonths(-1));

            if (!isDeletingAppointmentsLastMonth)
            {
                logger.Warn("Cron job ran, but no appointments were deleted.");
            }
            else
            {
                logger.Info("Cron job deleted last month's appointments successfully.");
            }
        }
        catch (Exception e)
        {
            logger.Error("Exception during cron job: " + e.Message, e);
            throw;
        }
        
    }
}