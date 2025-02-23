using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Data.Repositories;
using Data.Entities;
using Data.Data;
using Data.Services;
using Data.Interfaces;

namespace Data;

class Program
{
    private static ProjectContext? _db;
    private static IProjectService _projectService = null!;

    // ---------------------------------------------------------
    // 1. Helper methods for output
    // ---------------------------------------------------------
    private static void PrintHeader(string header)
    {
        string line = new string('═', header.Length + 8);
        Console.WriteLine($"╔{line}╗");
        Console.WriteLine($"║    {header}    ║");
        Console.WriteLine($"╚{line}╝");
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

    // ---------------------------------------------------------
    // 2. Main entry point
    // ---------------------------------------------------------
    static void Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not found in the configuration.");
        }

        // Configure DbContext
        var optionsBuilder = new DbContextOptionsBuilder<ProjectContext>();
        optionsBuilder.UseSqlServer(connectionString);

        // Initialize database context and ensure DB is created
        _db = new ProjectContext(configuration); // Initialize _db here
        _db.Database.EnsureCreated();

        // Initialize repository & service
        IProjectRepository projectRepository = new ProjectRepository(_db);
        _projectService = new ProjectService(projectRepository);

        Console.WriteLine("Database is ready.");

        while (true)
        {
            ShowMainMenu();
        }
    }

    // ---------------------------------------------------------
    // 3. Main Menu
    // ---------------------------------------------------------
    static void ShowMainMenu()
    {
        Console.Clear();
        PrintHeader("Project Management System");

        PrintMenuItem(1, "View All Projects");
        PrintMenuItem(2, "Create a New Project");
        PrintMenuItem(3, "Edit an Existing Project");
        PrintMenuItem(4, "Exit");

        Console.Write("Select an option: ");
        switch (Console.ReadLine()?.Trim())
        {
            case "1": ShowProjectsList(); break;
            case "2": AddNewProject(); break;
            case "3": EditProjectMenu(); break;
            case "4": Environment.Exit(0); break;
        }
    }

    // ---------------------------------------------------------
    // 4. Show Projects List
    // ---------------------------------------------------------
    static void ShowProjectsList()
    {
        Console.Clear();
        PrintSectionTitle("List of Projects");

        var projects = _projectService.GetAllProjects();
        if (!projects.Any())
        {
            Console.WriteLine("No projects found.");
            Console.ReadKey();
            return;
        }

        // Display all projects
        foreach (var p in projects)
        {
            Console.Write($"{p.ProjectNumber} - ");
            Console.WriteLine($"{p.Name} ({p.StartDate:yyyy-MM-dd} to {p.EndDate:yyyy-MM-dd}) - {p.Status}");
        }

        Console.Write("\nEnter project number to view details or 0 to go back: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input) && input != "0")
        {
            var project = _projectService.GetProject(input!); // Use null-forgiving operator
            if (project != null) DisplayProjectDetails(project);
        }
    }

    // ---------------------------------------------------------
    // 5. Display Project Details
    // ---------------------------------------------------------
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

        Console.Write("Customer: ");
        Console.WriteLine(project.Customer);

        Console.Write("Service: ");
        Console.WriteLine(project.Service);

        Console.Write("Total Price: ");
        Console.WriteLine($"{project.TotalPrice} SEK");

        Console.Write("Status: ");
        Console.WriteLine(project.Status);

        Console.Write("\nEdit Project? (Y/N): ");
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            EditProject(project);
        }
    }

    // ---------------------------------------------------------
    // 6. Create a New Project
    // ---------------------------------------------------------
    static void AddNewProject()
    {
        PrintSectionTitle("Create a New Project");

        var model = new ProjectInputModel
        {
            Name = GetInput("Name") ?? string.Empty,
            StartDate = GetDateInput("Start Date (yyyy-MM-dd)"),
            EndDate = GetDateInput("End Date (yyyy-MM-dd)"),
            ProjectManager = GetInput("Manager") ?? string.Empty,
            Customer = GetInput("Customer") ?? string.Empty,
            Service = GetInput("Service") ?? string.Empty,
            TotalPrice = GetDecimalInput("Total Price (SEK)"),
            Status = GetStatusInput()
        };

        if (_projectService.CreateProject(model) != null)
        {
            Console.WriteLine("Project added. Press any key...");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("Required fields cannot be empty.");
            Console.ReadKey();
        }
    }


    // ---------------------------------------------------------
    // 7. Edit Existing Project
    // ---------------------------------------------------------
    static void EditProjectMenu()
    {
        Console.Clear();
        PrintSectionTitle("Edit an Existing Project");

        Console.Write("Enter project number to edit: ");
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input))
        {
            var project = _projectService.GetProject(input!); // Use null-forgiving operator
            if (project != null)
            {
                EditProject(project);
            }
            else
            {
                Console.WriteLine("Project not found.");
                Console.ReadKey();
            }
        }
    }

    static void EditProject(ProjectEntity project)
    {
        Console.Clear();
        PrintSectionTitle($"Editing: {project.ProjectNumber}");

        project.Name = GetInput("Name", project.Name) ?? project.Name;
        project.StartDate = GetDateInput("Start Date", project.StartDate);
        project.EndDate = GetDateInput("End Date", project.EndDate);
        project.ProjectManager = GetInput("Manager", project.ProjectManager) ?? project.ProjectManager;
        project.Customer = GetInput("Customer", project.Customer) ?? project.Customer;
        project.Service = GetInput("Service", project.Service) ?? project.Service;
        project.TotalPrice = GetDecimalInput("Price", project.TotalPrice);
        project.Status = GetStatusInput(project.Status);

        Console.Write("Save? (y/n): ");
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            _db?.SaveChanges(); // Use null-conditional operator
            Console.WriteLine("Saved.");
        }
        Console.ReadKey();
    }

    // ---------------------------------------------------------
    // 8. Input Helpers
    // ---------------------------------------------------------
    private static string? GetInput(string prompt, string? defaultValue = null)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }

    private static DateTime GetDateInput(string prompt, DateTime? defaultValue = null)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) && defaultValue.HasValue)
            return defaultValue.Value;

        return DateTime.TryParse(input, out DateTime result)
            ? result
            : DateTime.Now;
    }

    private static decimal GetDecimalInput(string prompt, decimal? defaultValue = null)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) && defaultValue.HasValue)
            return defaultValue.Value;

        return decimal.TryParse(input, out decimal result)
            ? result
            : 0;
    }

    private static ProjectStatus GetStatusInput(ProjectStatus? defaultValue = null)
    {
        Console.Write("Status (1-NotStarted / 2-Ongoing / 3-Completed): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) && defaultValue.HasValue)
            return defaultValue.Value;

        return input switch
        {
            "1" => ProjectStatus.NotStarted,
            "2" => ProjectStatus.Ongoing,
            "3" => ProjectStatus.Completed,
            _ => ProjectStatus.NotStarted
        };
    }
}
