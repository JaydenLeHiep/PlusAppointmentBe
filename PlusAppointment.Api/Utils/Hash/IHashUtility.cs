namespace PlusAppointment.Utils.Hash;

public interface IHashUtility
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string password);
}
