using System.ComponentModel.DataAnnotations;

namespace StudyGroupApi.Models
{
    public class StudyGroup
    {
        public StudyGroup(int studyGroupId, string name, Subject subject, DateTime createDate, List<User> users)
        {
            StudyGroupId = studyGroupId;
            Name = name;
            Subject = subject;
            CreateDate = createDate;
            Users = users;
        }
        public StudyGroup() { }

        [Key]  // ? Ensures EF Core recognizes StudyGroupId as the Primary Key
        public int StudyGroupId { get; set; }

        public string Name { get; }
        public Subject Subject { get; }
        public DateTime CreateDate { get; }
        public List<User> Users { get; private set; }

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }
    }

    public enum Subject
    {
        Math,
        Chemistry,
        Physics
    }
}
