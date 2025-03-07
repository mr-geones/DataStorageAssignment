using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ProjectContext _context;

    public CustomerRepository(ProjectContext context)
    {
        _context = context;
    }

    public IEnumerable<CustomerEntity> GetAllCustomers()
    {
        return _context.Customers.ToList();
    }

    public CustomerEntity? GetCustomer(int customerId)
    {
        return _context.Customers.Find(customerId);
    }

    public CustomerEntity CreateCustomer(string name)
    {
        var customer = new CustomerEntity
        {
            Name = name
        };

        _context.Customers.Add(customer);
        _context.SaveChanges();

        return customer;
    }
}