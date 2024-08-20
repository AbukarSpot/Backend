using ProjectModels;

public interface IUserRepository {
    public Task AddUserAsync(User user);

    public Task<List<PublicModels.User>> GetAllUsersAsync();
}