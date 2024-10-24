using black_follow.Data;
using black_follow.Data.Dtos.UserDto;
using black_follow.Interface;

namespace black_follow.Service;

public interface IUserService
{
 Task<(UserLoginDto? user, State state)> LoginAsync(UserLoginForm form);   
}

public class UserService:IUserService
{
    IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<(UserLoginDto? user, State state)> LoginAsync(UserLoginForm form)
    {
        return await _userRepository.LoginAsync(form);
    }
}