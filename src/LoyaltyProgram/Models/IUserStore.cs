namespace LoyaltyProgram.Models
{
    public interface IUserStore
    {
        void Save(LoyaltyProgramUser newUser);
        LoyaltyProgramUser GetById(int id);
    }
}