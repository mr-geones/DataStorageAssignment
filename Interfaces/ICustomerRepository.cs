using Data.Entities;

namespace Data.Interfaces;

public interface ICustomerRepository
{
    IEnumerable<CustomerEntity> GetAllCustomers();

    CustomerEntity? GetCustomer(int customerId);

    CustomerEntity CreateCustomer(string name);
}