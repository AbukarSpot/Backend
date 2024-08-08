using ProjectModels;

public interface IUserRepository {
    public Task AddUserAsync(User user);
}