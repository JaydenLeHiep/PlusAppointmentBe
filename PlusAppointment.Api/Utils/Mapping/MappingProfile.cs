
using AutoMapper;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Customers;
using PlusAppointment.Models.DTOs.Staff;

namespace PlusAppointment.Utils.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        
        CreateMap<Staff, StaffRetrieveDto>();

        CreateMap<Customer, CustomerRetrieveDto>();
    }
}