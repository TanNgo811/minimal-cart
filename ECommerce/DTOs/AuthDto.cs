namespace ECommerce.DTOs;

public record LoginDto(string Email, string Password);

public record RegisterDto(
    string Email,
    string Username,
    string Password,
    string FirstName,
    string LastName,
    string Address,
    string PhoneNumber
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    string Role
);
