using black_follow.Data;
using black_follow.Data.Dtos.UserDto;

namespace black_follow.Interface;

public interface IUserRepository
{
    Task<(UserLoginDto? user, State state )> LoginAsync(UserLoginForm form);
}