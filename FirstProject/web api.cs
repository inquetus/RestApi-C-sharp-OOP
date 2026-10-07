using static AuthService;

namespace FirstProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

       
        builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
        builder.Services.AddSingleton<_PasswordHasher, PasswordHasherImpl>();

        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<BookingService>();

       
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

       
        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = "swagger";
        });

        
        app.MapPost("/register", (DataTransferObject.RegisterDTO registerDTO, AuthService authService) =>
        {
            try
            {
                var user = authService.Register(registerDTO);
                return Results.Ok(new { user.id, user.username });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/login", (DataTransferObject.LoginDTO loginDTO, AuthService authService) =>
        {
            try
            {
                var user = authService.Login(loginDTO);
                return Results.Ok(new { user.id, user.username });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/bookings", (DataTransferObject.CreateBookingDTO createBookingDTO, BookingService bookingService) =>
        {
            try
            {
                var booking = bookingService.CreateBooking(createBookingDTO);
                return Results.Ok(new { booking.id, booking.userId, booking.ResourceName, booking.starttime, booking.endtime });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

      
        app.Run();
    }
}