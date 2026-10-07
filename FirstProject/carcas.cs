using System.Collections.Concurrent;
using static DataTransferObject;

public class User
{
  public string id{ get; set; } = Guid.NewGuid().ToString();
     public string username { get; set; } = string.Empty;
    public string passwordhash { get; set; } = string.Empty;
 public string role { get; set; } = "user";
}
public class Booking
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string userId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime starttime { get; private set; } 
    public DateTime endtime { get; private set; }
    public Booking(string userId, string resourceName, DateTime starttime, DateTime endtime)
    {
        this.userId = userId;
        this.ResourceName = resourceName;
        this.starttime = starttime;
        this.endtime = endtime;
    }
    public Booking() { }
}
public interface IUserRepository
{
    User GetById(string id);
    User GetByUsername(string username);
    void Add(User user);

}
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
        public User? GetById(string id)
        {
            users.TryGetValue(id, out var user);
            return user;
        }

        public User? GetByUsername(string username)
    {
        return users.Values.FirstOrDefault(u => u.username == username);
    }
    public void Add(User user)
    {
        users.TryAdd(user.id, user);
    }

}
public interface IBookingRepository
{
    Booking? GetById(string id);
    IEnumerable<Booking> GetByUserId(string userId);
    IEnumerable<Booking> GetAll();
    void Add(Booking booking);
    void Remove(string id);

}
public class InMemoryBookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<string, Booking> bookings = new ConcurrentDictionary<string, Booking>();

    public Booking? GetById(string id)
    {
        bookings.TryGetValue(id, out var booking);
        return booking;
    }
    public IEnumerable<Booking> GetByUserId(string userId)
    {
        return bookings.Values;
    }
    public IEnumerable<Booking> GetAll()
    {
        return bookings.Values;
    }
    public void Add(Booking booking)
    {
        bookings.TryAdd(booking.id, booking);

    }
    public void Remove(string id)
    {
        bookings.TryRemove(id, out _);
    }
}
public interface _PasswordHasher
{
    public string HashPassword(string password);

    public bool VerifyPassword(string password, string hash);
}

public class PasswordHasherImpl : _PasswordHasher
{
    public string HashPassword(string password)
    {
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
    }
    public bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
public class DataTransferObject
{
    public class RegisterDTO
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
    public class LoginDTO
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
    public class CreateBookingDTO
    {
        public string UserID { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public DateTime starttime { get; set; }
        public DateTime endtime { get; set; } 
    }
    public class BookingDTORsrponse
    { 
        public string id { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public DateTime starttime { get; set; }
        public DateTime endtime { get; set; }
    }
}
public class AuthService
{
    private readonly IUserRepository _users;
    private readonly _PasswordHasher _passwordHasher;
    public AuthService(IUserRepository users, _PasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }
    public User? Register(string username, string password)
    {
        if (_users.GetByUsername(username) != null)
        {
            throw new InvalidOperationException("User already exists");
        }
        var user = new User
        {
            username = username,
            passwordhash = _passwordHasher.HashPassword(password)
        };
        _users.Add(user);
        return user;
    }
    public User Login(DataTransferObject.LoginDTO loginDTO)
    {
        var user = _users.GetByUsername(loginDTO.username);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid username or password");
        }
        if (!_passwordHasher.VerifyPassword(loginDTO.password, user.passwordhash))
        {
            throw new InvalidOperationException("Invalid username or password");
        }
        return user;
    }
    public User? Register(DataTransferObject.RegisterDTO registerDTO)
    {
        _users.GetByUsername(registerDTO.username);
        if (_users.GetByUsername(registerDTO.username) != null)
        {
            throw new InvalidOperationException("User already exists");
        }
        string hashedPassword = _passwordHasher.HashPassword(registerDTO.password);
        var user = new User
        {
            id = Guid.NewGuid().ToString(),
            username = registerDTO.username,
            passwordhash = hashedPassword
        };
        _users.Add(user);
        return user;

    }
    public class BookingService
    {
        private readonly IBookingRepository _bookings;
        private readonly IUserRepository _users;
        public BookingService(IBookingRepository bookings, IUserRepository users)
        {
            _bookings = bookings;
            _users = users;
        }
        public Booking CreateBooking(CreateBookingDTO dto)
        {
      
            var user = _users.GetById(dto.UserID);
                    if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            var resourceBookings = _bookings.GetAll().Where(b => b.ResourceName == dto.ResourceName);
            if (resourceBookings.Any(b => (dto.starttime < b.endtime && dto.endtime > b.starttime)))
            {
                throw new InvalidOperationException("Resource is already booked for the specified time period");
            }
            var booking = new Booking(dto.UserID, dto.ResourceName, dto.starttime, dto.endtime);
            if (dto.endtime < dto.starttime)
            {
                throw new InvalidOperationException("End time must be after start time");
            }
            _bookings.Add(booking);

            return booking;
        }


    }

}

