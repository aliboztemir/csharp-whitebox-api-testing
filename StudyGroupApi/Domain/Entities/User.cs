namespace StudyGroupApi.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }

        public User(int userId, string name)
        {
            UserId = userId;
            Name = name;
        }

        public User() { }
    }
}
