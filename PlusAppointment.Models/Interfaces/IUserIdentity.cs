namespace PlusAppointment.Models.Interfaces;

public interface IUserIdentity
{
    int Id { get; }
    string Username { get; }
    string Role { get; }
}