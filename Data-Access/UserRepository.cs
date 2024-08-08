
using DatabaseContex;
using Microsoft.EntityFrameworkCore;
using ProjectModels;

public class UserRepository: IUserRepository {

    private readonly ProjectContext _context;
    public UserRepository(ProjectContext context) {
        this._context = context;
    }

    public async Task AddUserAsync(User payload) {
        User? user = await this._context
                                .User
                                .SingleOrDefaultAsync(User => User.Username == payload.Username);
        if (user is not null) {
            throw new InvalidOperationException($"{user.Username} already exists.");
        }

        this._context.Add(payload);
        await this._context.SaveChangesAsync();
    }
}