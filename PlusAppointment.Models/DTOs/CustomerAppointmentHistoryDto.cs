namespace PlusAppointment.Models.DTOs;

public class CustomerAppointmentHistoryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public List<AppointmentDetailDto> AppointmentDetails { get; set; }
    public decimal TotalAmountPaid { get; set; }
}