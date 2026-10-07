# FirstProject
A clean, modular RESTful API built with .NET 10 and ASP.NET Core Minimal APIs, designed for user authentication and resource reservation logic with overlap validation.
Features & Architecture
Clean Architecture: Structured separation into DTOs, Domain Models, Repositories, and Business Logic Services.
Dependency Injection: Services and repositories (IUserRepository, IBookingRepository, AuthService, BookingService) are registered and resolved via the native .NET DI container.
Authentication: User registration with custom password hashing and validation.
Reservation Engine: Business logic to prevent double-booking and handle date/time slot overlapping errors. Interactive Documentation: Integrated Swagger UI (Swashbuckle) for API testing and schema verification.
