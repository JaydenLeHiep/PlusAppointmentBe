using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.StaffRepo;

namespace PlusAppointment.Repositories.Implementation.StaffRepo
{
    public class StaffRepository : IStaffRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public StaffRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staffs = await context.Staffs.ToListAsync();
                return staffs;
            }
        }

        public async Task<Staff> GetByIdAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs.FindAsync(id);
                if (staff == null)
                {
                    throw new KeyNotFoundException($"Staff with ID {id} not found");
                }

                return staff;
            }
        }

        public async Task<IEnumerable<Staff?>> GetAllByBusinessIdAsync(int businessId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staffs = await context.Staffs
                    .Where(s => s.BusinessId == businessId)
                    .OrderBy(s => s.StaffId)
                    .ToListAsync();
                return staffs;
            }
        }

        public async Task AddStaffAsync(Staff staff, int businessId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    throw new KeyNotFoundException($"Business with ID {businessId} not found.");
                }

                staff.BusinessId = businessId;
                await context.Staffs.AddAsync(staff);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddListStaffsAsync(Staff?[] staffs)
        {
            if (staffs == null || !staffs.Any())
            {
                throw new Exception("Staffs collection cannot be null or empty");
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var staffList = staffs.ToList();
                var businessId = staffList.First().BusinessId;
                var business = await context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    throw new KeyNotFoundException($"Business with ID {businessId} not found.");
                }

                await context.Staffs.AddRangeAsync(staffList);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Staff staff)
        {
            await using var connection = new NpgsqlConnection(_contextFactory.CreateDbContext().Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var updateQuery = @"
                    UPDATE staffs 
                    SET 
                        name = @Name, 
                        email = @Email, 
                        phone = @Phone, 
                        password = @Password,
                        business_id = @BusinessId
                    WHERE staff_id = @StaffId";

                await using var command = new NpgsqlCommand(updateQuery, connection, transaction);

                command.Parameters.AddWithValue("@Name", staff.Name);
                command.Parameters.AddWithValue("@Email", staff.Email);
                command.Parameters.AddWithValue("@Phone", staff.Phone);
                command.Parameters.AddWithValue("@Password", staff.Password);
                command.Parameters.AddWithValue("@BusinessId", staff.BusinessId);
                command.Parameters.AddWithValue("@StaffId", staff.StaffId);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Staff?> GetByBusinessIdStaffIdAsync(int businessId, int staffId)
        {

            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs
                    .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    throw new KeyNotFoundException($"Staff with ID {staffId} not found for Business ID {businessId}");
                }
                return staff;
            }
        }

        public async Task DeleteAsync(int businessId, int staffId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs
                    .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                    .FirstOrDefaultAsync();

                if (staff != null)
                {
                    context.Staffs.Remove(staff);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<Staff> GetByEmailAsync(string email)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs.SingleOrDefaultAsync(s => s.Email == email);
                if (staff == null)
                {
                    throw new KeyNotFoundException($"Staff with email {email} not found");
                }

                return staff;
            }
        }
    }
}
