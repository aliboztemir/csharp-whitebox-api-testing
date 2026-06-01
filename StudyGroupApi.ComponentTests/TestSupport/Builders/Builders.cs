using StudyGroupApi.Domain.Entities;

namespace StudyGroupApi.ComponentTests.TestSupport.Builders
{
    public class UserBuilder
    {
        private int _id = 1;
        private string _name = "Alice";

        public UserBuilder WithId(int id) { _id = id; return this; }
        public UserBuilder WithName(string name) { _name = name; return this; }

        public User Build() => new User(_id, _name);
    }

    public class StudyGroupBuilder
    {
        private int _id = 1;
        private string _name = "Math Club";
        private Subject _subject = Subject.Math;
        private DateTime _createDate = DateTime.Now;
        private List<User> _users = new();

        public StudyGroupBuilder WithId(int id) { _id = id; return this; }
        public StudyGroupBuilder WithName(string name) { _name = name; return this; }
        public StudyGroupBuilder WithSubject(Subject subject) { _subject = subject; return this; }
        public StudyGroupBuilder WithCreateDate(DateTime date) { _createDate = date; return this; }
        public StudyGroupBuilder WithUser(User user) { _users = new List<User> { user }; return this; }
        public StudyGroupBuilder WithUsers(List<User> users) { _users = users; return this; }
        public StudyGroupBuilder WithNoUsers() { _users = new List<User>(); return this; }

        public StudyGroup Build() => new StudyGroup
        {
            StudyGroupId = _id,
            Name = _name,
            Subject = _subject,
            CreateDate = _createDate,
            Users = _users
        };
    }
}
