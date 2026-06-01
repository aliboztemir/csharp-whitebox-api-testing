using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudyGroupApi.Domain.Entities
{
    public class StudyGroup
    {
        public StudyGroup(int studyGroupId, string name, Subject subject, DateTime createDate, List<User> users)
        {
            if (studyGroupId < 0)
                throw new ArgumentOutOfRangeException(nameof(studyGroupId), "ID cannot be negative");

            ValidateName(name);

            if (!Enum.IsDefined(typeof(Subject), subject))
                throw new InvalidEnumArgumentException("Invalid subject type");

            if (createDate.Date < DateTime.Today)
                throw new ArgumentOutOfRangeException("CreateDate", "CreateDate cannot be in the past");
            if (createDate > DateTime.Now.AddYears(1))
                throw new ArgumentOutOfRangeException("CreateDate", "CreateDate is too far in the future");

            StudyGroupId = studyGroupId;
            Name = name;
            Subject = subject;
            CreateDate = createDate;
            Users = new List<User>();

            if (users == null)
                return;

            foreach (var user in users)
                AddUser(user);
        }

        private StudyGroup() { Users = new List<User>(); }

        [Key]
        public int StudyGroupId { get; set; }

        public string Name { get; init; }
        public Subject Subject { get; init; }
        public DateTime CreateDate { get; init; }
        public List<User> Users { get; init; }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (Users.Any(existingUser => existingUser.UserId == user.UserId))
                throw new InvalidOperationException("User already exists in the group.");
            Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = Users.FirstOrDefault(existing => existing.UserId == user.UserId);

            if (existingUser == null)
                throw new InvalidOperationException("User not found in the group.");

            Users.Remove(existingUser);
        }

        public static void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (name.Length < 5)
                throw new ArgumentException("Name must be at least 5 characters.", nameof(name));
            if (name.Length > 30)
                throw new ArgumentException("Name cannot exceed 30 characters.", nameof(name));
        }
    }

    public enum Subject
    {
        Math,
        Chemistry,
        Physics
    }
}
