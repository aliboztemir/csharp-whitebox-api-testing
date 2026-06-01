namespace StudyGroupApi.Models
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
    }
}
