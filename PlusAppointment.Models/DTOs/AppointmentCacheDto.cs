public class AppointmentCacheDto
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string StaffPhone { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<int> ServiceIds { get; set; } = new List<int>();
}