using System.ComponentModel;
using StudyGroupApi.Models;

namespace StudyGroupApi.Tests.Unit
{
    [TestFixture]
    public class StudyGroupUnitTests
    {
        [Test]
        public void StudyGroup_Should_Have_Assigned_Id()
        {
            var studyGroup = new StudyGroup(1, "Robotics Club", Subject.Physics, DateTime.Now, new List<User>());
            Assert.AreEqual(1, studyGroup.StudyGroupId);
        }

        [Test]
        public void StudyGroup_Should_Store_Name_Correctly()
        {
            var studyGroup = new StudyGroup(5, "Chemistry Society", Subject.Chemistry, DateTime.Now, new List<User>());
            Assert.AreEqual("Chemistry Society", studyGroup.Name);
        }

        [Test]
        public void StudyGroup_Should_Store_Subject_Correctly()
        {
            var studyGroup = new StudyGroup(3, "AI Enthusiasts", Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual(Subject.Math, studyGroup.Subject);
        }

        [Test]
        public void StudyGroup_Should_Set_Creation_Timestamp()
        {
            var creationDate = DateTime.Now;
            var studyGroup = new StudyGroup(7, "Astronomy Club", Subject.Physics, creationDate, new List<User>());
            Assert.AreEqual(creationDate, studyGroup.CreateDate);
        }

        [Test, Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public void StudyGroup_Should_Initialize_Empty_User_List_When_Null()
        {
            var studyGroup = new StudyGroup(12, "BioTech Innovators", Subject.Chemistry, DateTime.Now, null);
            Assert.IsNotNull(studyGroup.Users);
            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        [Test]
        public void StudyGroup_Should_Reject_Negative_Id()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StudyGroup(-5, "Invalid ID Group", Subject.Physics, DateTime.Now, new List<User>())
            );

            Assert.That(exception.ParamName, Is.EqualTo("studyGroupId"));
            Assert.That(exception.Message, Does.Contain("ID cannot be negative"));
        }

        [Test]
        public void StudyGroup_Should_Reject_Names_Shorter_Than_Five_Characters()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(8, "Bio", Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Should_Reject_Names_Longer_Than_30_Characters()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(9, "TheLongestStudyGroupNameEverCreatedAndBeyond", Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Should_Reject_Empty_Or_Null_Name()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(15, "", Subject.Chemistry, DateTime.Now, new List<User>())
            );

            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(16, null, Subject.Chemistry, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Should_Accept_Name_With_Exactly_5_Characters()
        {
            var studyGroup = new StudyGroup(20, "Alpha", Subject.Physics, DateTime.Now, new List<User>());
            Assert.AreEqual("Alpha", studyGroup.Name);
        }

        [Test]
        public void StudyGroup_Should_Accept_Name_With_Exactly_30_Characters()
        {
            var validName = "StudyGroupNameWithExactly30Chars";
            var studyGroup = new StudyGroup(21, validName, Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual(validName, studyGroup.Name);
        }

        [Test]
        public void Adding_User_To_StudyGroup_Should_Increase_Count()
        {
            var studyGroup = new StudyGroup(25, "Robotics Lab", Subject.Physics, DateTime.Now, new List<User>());
            var user = new User(10, "Sophia");

            studyGroup.AddUser(user);

            Assert.AreEqual(1, studyGroup.Users.Count);
        }

        [Test]
        public void Adding_Null_User_Should_Throw_Exception()
        {
            var studyGroup = new StudyGroup(30, "Biology Club", Subject.Chemistry, DateTime.Now, new List<User>());
            Assert.Throws<ArgumentNullException>(() => studyGroup.AddUser(null));
        }

        [Test]
        public void Adding_Duplicate_User_Should_Throw_Exception()
        {
            var user = new User(11, "David");
            var studyGroup = new StudyGroup(35, "Quantum Mechanics Group", Subject.Math, DateTime.Now, new List<User> { user });

            Assert.Throws<InvalidOperationException>(() => studyGroup.AddUser(user));
        }


        [Test]
        public void RemoveUser_Should_Decrease_User_Count()
        {
            var user = new User(6, "John");
            var studyGroup = new StudyGroup(5, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            studyGroup.AddUser(user);

            studyGroup.RemoveUser(user);

            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        [Test]
        public void Removing_Null_User_Should_Throw_Exception()
        {
            var studyGroup = new StudyGroup(50, "Advanced Mathematics", Subject.Math, DateTime.Now, new List<User>());
            Assert.Throws<ArgumentNullException>(() => studyGroup.RemoveUser(null));
        }

        [Test]
        public void Removing_Non_Existing_User_Should_Throw_Exception()
        {
            var user = new User(20, "Jonathan");
            var studyGroup = new StudyGroup(55, "Artificial Intelligence Lab", Subject.Chemistry, DateTime.Now, new List<User>());

            var exception = Assert.Throws<InvalidOperationException>(() => studyGroup.RemoveUser(user));
            Assert.AreEqual("User not found in the group.", exception.Message);
        }

        [Test]
        public void StudyGroup_Should_Reject_Duplicate_Users()
        {
            var studyGroup = new StudyGroup(60, "Machine Learning Workshop", Subject.Math, DateTime.Now, new List<User>());
            var user = new User(25, "Michael");

            studyGroup.AddUser(user);

            var exception = Assert.Throws<InvalidOperationException>(() => studyGroup.AddUser(user));
            Assert.AreEqual("User already exists in the group.", exception.Message);
        }

        [Test]
        public void StudyGroup_Should_Reject_Invalid_Subject()
        {
            var invalidSubject = (Subject)999;

            var exception = Assert.Throws<InvalidEnumArgumentException>(() =>
                new StudyGroup(70, "Genetics Research", invalidSubject, DateTime.Now, new List<User>())
            );

            Assert.That(exception.Message, Does.Contain("Invalid subject type"));
        }

        [Test]
        public void StudyGroup_Should_Reject_Creation_With_Invalid_Date()
        {
            var pastDate = DateTime.Now.AddDays(-1);
            var farFutureDate = DateTime.Now.AddYears(100);

            var pastException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StudyGroup(75, "Organic Chemistry Lab", Subject.Chemistry, pastDate, new List<User>())
            );
            Assert.That(pastException.ParamName, Is.EqualTo("CreateDate"));
            Assert.That(pastException.Message, Does.Contain("cannot be in the past"));

            var futureException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StudyGroup(76, "Astrobiology Club", Subject.Physics, farFutureDate, new List<User>())
            );
            Assert.That(futureException.ParamName, Is.EqualTo("CreateDate"));
            Assert.That(futureException.Message, Does.Contain("too far in the future"));
        }

        [Test, NUnit.Framework.Category("SanityCheck")]
        public void StudyGroup_Should_Initialize_With_Valid_State()
        {
            var users = new List<User> { new User(35, "Emma"), new User(40, "David") };
            var studyGroup = new StudyGroup(80, "History Forum", Subject.Chemistry, DateTime.Now, users);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(80, studyGroup.StudyGroupId);
                Assert.AreEqual("History Forum", studyGroup.Name);
                Assert.AreEqual(Subject.Chemistry, studyGroup.Subject);
                Assert.AreEqual(2, studyGroup.Users.Count);
            });
        }
    }
}
