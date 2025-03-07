using Data.Entities;
using Data.Interfaces;

namespace Data.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IEnumerable<CustomerEntity> GetAllCustomers()
    {
        try
        {
            return _repository.GetAllCustomers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving all customers: {ex.Message}");
            throw new Exception("Failed to retrieve customers. Please try again later.", ex);
        }
    }

    public CustomerEntity GetCustomer(int customerId)
    {
        try
        {
            var customer = _repository.GetCustomer(customerId);

            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with ID {customerId} not found.");
            }

            return customer;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving customer {customerId}: {ex.Message}");
            throw new Exception($"Failed to retrieve customer {customerId}. Please try again later.", ex);
        }
    }

    public CustomerEntity CreateCustomer(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name cannot be empty.");
        }

        try
        {
            return _repository.CreateCustomer(name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating customer: {ex.Message}");
            throw new Exception("Failed to create customer. Please try again later.", ex);
        }
    }
}