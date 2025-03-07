using Data.Entities;

namespace Data.Interfaces;

public interface ICustomerService
{
    IEnumerable<CustomerEntity> GetAllCustomers();

    CustomerEntity GetCustomer(int customerId);

    CustomerEntity CreateCustomer(string name);
}