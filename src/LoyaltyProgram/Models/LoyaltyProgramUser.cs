namespace LoyaltyProgram.Models
{
    public class LoyaltyProgramUser
    {
        public int Id { get;  internal set; }
        public string Name { get; set; }
        public int LoyaltyPoints { get; set; }
        public Kesukaan Settings { get; set; }
    }

    public class Kesukaan
    {
        public string[] Interests { get; set; }
    }
}