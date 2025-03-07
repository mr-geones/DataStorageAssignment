using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Data.Repositories;
using Data.Entities;
using Data.Services;
using Data.Interfaces;

namespace Data;

class Program
{
    private static ProjectContext? _database;

    private static IProjectService _projectService = null!;

    private static ICustomerService _customerService = null!;

    // =================================================================
    // SECTION 1: DISPLAY HELPERS - Methods that format console output
    // =================================================================

    private static void PrintHeader(string header)
    {
        string borderLine = new string('═', header.Length + 8);

        Console.WriteLine($"╔{borderLine}╗");
        Console.WriteLine($"║    {header}    ║");
        Console.WriteLine($"╚{borderLine}╝");
    }

    private static void PrintSectionTitle(string title)
    {
        Console.WriteLine($"\n===== {title} =====");
    }

    private static void PrintMenuItem(int number, string text)
    {
        Console.Write($"{number}. ");
        Console.WriteLine(text);
    }

    // =================================================================
    // SECTION 2: APPLICATION STARTUP - Main entry point and setup
    // =================================================================

    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ProjectContext>();
        optionsBuilder.UseSqlServer(connectionString);

        _database = new ProjectContext(configuration);
        _database.Database.EnsureCreated();

        IProjectRepository projectRepository = new ProjectRepository(_database);
        _projectService = new ProjectService(projectRepository);

        ICustomerRepository customerRepository = new CustomerRepository(_database);
        _customerService = new CustomerService(customerRepository);

        Console.WriteLine("Database is ready.");

        while (true)
        {
            ShowMainMenu();
        }
    }

    // =================================================================
    // SECTION 3: MENU SYSTEMS - Displaying and handling menu options
    // =================================================================

    static void ShowMainMenu()
    {
        Console.Clear();
        PrintHeader("Project Management System");

        PrintMenuItem(1, "View All Projects");
        PrintMenuItem(2, "Create a New Project");
        PrintMenuItem(3, "Edit an Existing Project");
        PrintMenuItem(4, "Exit");

        // Get and process user input
        Console.Write("\nSelect an option (1-4): ");
        string? userChoice = Console.ReadLine()?.Trim();

        switch (userChoice)
        {
            case "1":
                ShowProjectsList();
                break;
            case "2":
                AddNewProject();
                break;
            case "3":
                EditProjectMenu();
                break;
            case "4":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Invalid option. Press any key to try again...");
                Console.ReadKey();
                break;
        }
    }

    // =================================================================
    // SECTION 4: PROJECT DISPLAY - Viewing project information
    // =================================================================

    static void ShowProjectsList()
    {
        Console.Clear();
        PrintSectionTitle("List of Projects");

        var allProjects = _projectService.GetAllProjects();

        if (!allProjects.Any())
        {
            Console.WriteLine("No projects found in the database.");
            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey();
            return;
        }

        foreach (var project in allProjects)
        {
            Console.Write($"{project.ProjectNumber} - ");
            Console.WriteLine($"{project.Name} ({project.StartDate:yyyy-MM-dd} to {project.EndDate:yyyy-MM-dd}) - {project.Status}");
        }

        Console.Write("\nEnter project number to view details or 0 to go back: ");
        var userInput = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(userInput) && userInput != "0")
        {
            try
            {
                var selectedProject = _projectService.GetProject(userInput);
                DisplayProjectDetails(selectedProject);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Project with number '{userInput}' not found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    static void DisplayProjectDetails(ProjectEntity project)
    {
        Console.Clear();
        PrintSectionTitle($"Project Details ({project.ProjectNumber})");

        Console.Write("Name: ");
        Console.WriteLine(project.Name);

        Console.Write("Duration: ");
        Console.WriteLine($"{project.StartDate:yyyy-MM-dd} to {project.EndDate:yyyy-MM-dd}");

        Console.Write("Manager: ");
        Console.WriteLine(project.ProjectManager);

        // Get customer name from ID
        var customer = _customerService.GetCustomer(project.CustomerId);
        Console.Write("Customer: ");
        Console.WriteLine(customer.Name);

        Console.Write("Service: ");
        Console.WriteLine(project.Service);

        Console.Write("Total Price: ");
        Console.WriteLine($"{project.TotalPrice} SEK");

        Console.Write("Status: ");
        Console.WriteLine(project.Status);

        Console.Write("\nWould you like to edit this project? (Y/N): ");
        if (Console.ReadLine()?.Trim().ToUpper() == "Y")
        {
            EditProject(project);
        }
    }

    // =================================================================
    // SECTION 5: PROJECT CREATION - Adding new projects
    // =================================================================

    static void AddNewProject()
    {
        Console.Clear();
        PrintSectionTitle("Create a New Project");

        // Display available customers first
        var customers = _customerService.GetAllCustomers();
        Console.Write("\nCustomer Name: ");
        string customerName = Console.ReadLine()?.Trim() ?? string.Empty;

        // Check if customer exists, if not create one
        int customerId;
        var existingCustomer = _customerService.GetAllCustomers()
            .FirstOrDefault(c => c.Name.Equals(customerName, StringComparison.OrdinalIgnoreCase));

        if (existingCustomer != null)
        {
            customerId = existingCustomer.CustomerId;
        }
        else
        {
            // Create new customer
            var newCustomer = _customerService.CreateCustomer(customerName);
            customerId = newCustomer.CustomerId;
        }

        var projectData = new ProjectInputModel
        {
            Name = GetInput("Project Name") ?? string.Empty,
            StartDate = GetDateInput("Start Date (yyyy-MM-dd)"),
            EndDate = GetDateInput("End Date (yyyy-MM-dd)"),
            ProjectManager = GetInput("Project Manager") ?? string.Empty,
            CustomerId = customerId,
            Service = GetInput("Service Provided") ?? string.Empty,
            TotalPrice = GetDecimalInput("Total Price (SEK)"),
            Status = GetStatusInput()
        };

        var createdProject = _projectService.CreateProject(projectData);

        if (createdProject != null)
        {
            Console.WriteLine($"Project successfully added with ID: {createdProject.ProjectNumber}");
            Console.WriteLine("Press any key to continue...");
        }
        else
        {
            Console.WriteLine("Failed to create project. Required fields cannot be empty.");
            Console.WriteLine("Press any key to try again...");
        }

        Console.ReadKey();
    }

    // =================================================================
    // SECTION 6: PROJECT EDITING - Modifying existing projects
    // =================================================================

    static void EditProjectMenu()
    {
        Console.Clear();
        PrintSectionTitle("Edit an Existing Project");

        Console.Write("Enter project number to edit: ");
        var projectNumber = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(projectNumber))
        {
            try
            {
                var projectToEdit = _projectService.GetProject(projectNumber);
                EditProject(projectToEdit);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Project with number '{projectNumber}' not found.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    static void EditProject(ProjectEntity project)
    {
        Console.Clear();
        PrintSectionTitle($"Editing Project: {project.ProjectNumber}");

        Console.WriteLine("Press Enter to keep current value, or type a new value.");

        project.Name = GetInput("Name", project.Name) ?? project.Name;
        project.StartDate = GetDateInput("Start Date", project.StartDate);
        project.EndDate = GetDateInput("End Date", project.EndDate);
        project.ProjectManager = GetInput("Manager", project.ProjectManager) ?? project.ProjectManager;

        // Display available customers
        var currentCustomer = _customerService.GetCustomer(project.CustomerId);
        Console.Write($"Customer Name [{currentCustomer.Name}]: ");
        string customerNameInput = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!string.IsNullOrEmpty(customerNameInput))
        {
            // Check if customer exists, if not create one
            var existingCustomer = _customerService.GetAllCustomers()
                .FirstOrDefault(c => c.Name.Equals(customerNameInput, StringComparison.OrdinalIgnoreCase));

            if (existingCustomer != null)
            {
                project.CustomerId = existingCustomer.CustomerId;
            }
            else
            {
                // Create new customer
                var newCustomer = _customerService.CreateCustomer(customerNameInput);
                project.CustomerId = newCustomer.CustomerId;
            }
        }

        project.Service = GetInput("Service", project.Service) ?? project.Service;
        project.TotalPrice = GetDecimalInput("Price (SEK)", project.TotalPrice);
        project.Status = GetStatusInput(project.Status);

        Console.Write("\nSave these changes? (Y/N): ");
        if (Console.ReadLine()?.Trim().ToUpper() == "Y")
        {
            _database?.SaveChanges();
            Console.WriteLine("Changes saved successfully.");
        }
        else
        {
            Console.WriteLine("Changes were discarded.");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    // =================================================================
    // SECTION 7: INPUT HELPERS - Methods to collect and validate input
    // =================================================================

    private static string? GetInput(string prompt, string? defaultValue = null)
    {
        string displayPrompt = defaultValue != null
            ? $"{prompt} [{defaultValue}]"
            : prompt;

        Console.Write($"{displayPrompt}: ");
        var userInput = Console.ReadLine()?.Trim();

        return string.IsNullOrEmpty(userInput) ? defaultValue : userInput;
    }

    private static DateTime GetDateInput(string prompt, DateTime? defaultValue = null)
    {
        string displayPrompt = defaultValue.HasValue
            ? $"{prompt} [{defaultValue.Value:yyyy-MM-dd}]"
            : prompt;

        Console.Write($"{displayPrompt}: ");
        var userInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(userInput) && defaultValue.HasValue)
            return defaultValue.Value;

        if (DateTime.TryParse(userInput, out DateTime result))
            return result;
        else
        {
            Console.WriteLine("Invalid date format. Using today's date.");
            return DateTime.Now;
        }
    }

    private static decimal GetDecimalInput(string prompt, decimal? defaultValue = null)
    {
        string displayPrompt = defaultValue.HasValue
            ? $"{prompt} [{defaultValue.Value}]"
            : prompt;

        Console.Write($"{displayPrompt}: ");
        var userInput = Console.ReadLine()?.Trim();

        // Return default if user just pressed Enter
        if (string.IsNullOrEmpty(userInput) && defaultValue.HasValue)
            return defaultValue.Value;

        // Try to parse the decimal, return 0 if invalid
        if (decimal.TryParse(userInput, out decimal result))
            return result;
        else
        {
            Console.WriteLine("Invalid number format. Using 0.");
            return 0;
        }
    }

    private static ProjectStatus GetStatusInput(ProjectStatus? defaultValue = null)
    {
        string currentStatus = defaultValue.HasValue
            ? $" [Current: {defaultValue.Value}]"
            : "";

        Console.WriteLine($"Project Status{currentStatus}:");
        Console.WriteLine("1 - Not Started");
        Console.WriteLine("2 - Ongoing");
        Console.WriteLine("3 - Completed");

        Console.Write("Enter your choice (1-3): ");
        var userInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(userInput) && defaultValue.HasValue)
            return defaultValue.Value;

        return userInput switch
        {
            "1" => ProjectStatus.NotStarted,
            "2" => ProjectStatus.Ongoing,
            "3" => ProjectStatus.Completed,
            _ => ProjectStatus.NotStarted
        };
    }
}
