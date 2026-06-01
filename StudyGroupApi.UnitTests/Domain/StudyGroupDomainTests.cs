using System.ComponentModel;
using StudyGroupApi.Domain.Entities;
using StudyGroupApi.Tests.TestSupport.Builders;

namespace StudyGroupApi.UnitTests.Domain
{
    [TestFixture]
    [NUnit.Framework.Category("Unit")]
    [NUnit.Framework.Category("Domain")]
    public class StudyGroupDomainTests
    {
        // -- Construction -----------------------------------------------------

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Set_StudyGroupId_When_Created()
        {
            var studyGroup = new StudyGroupBuilder().WithId(1).Build();
            Assert.AreEqual(1, studyGroup.StudyGroupId);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Store_Name_When_Created()
        {
            var studyGroup = new StudyGroupBuilder().WithName("Chemistry Society").WithId(5).WithSubject(Subject.Chemistry).Build();
            Assert.AreEqual("Chemistry Society", studyGroup.Name);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Store_Subject_When_Created()
        {
            var studyGroup = new StudyGroupBuilder().WithId(3).WithName("AI Enthusiasts").WithSubject(Subject.Math).Build();
            Assert.AreEqual(Subject.Math, studyGroup.Subject);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Set_CreationDate_When_Created()
        {
            var creationDate = DateTime.Now;
            var studyGroup = new StudyGroupBuilder().WithId(7).WithName("Astronomy Club").WithSubject(Subject.Physics).WithCreateDate(creationDate).Build();
            Assert.AreEqual(creationDate, studyGroup.CreateDate);
        }

        [Test, Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public void Should_Initialize_Empty_UserList_When_Users_Are_Null()
        {
            var studyGroup = new StudyGroup(12, "BioTech Innovators", Subject.Chemistry, DateTime.Now, null);
            Assert.IsNotNull(studyGroup.Users);
            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        // -- Validation -------------------------------------------------------

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_StudyGroup_When_Id_Is_Negative()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StudyGroup(-5, "Invalid ID Group", Subject.Physics, DateTime.Now, new List<User>())
            );

            Assert.That(exception.ParamName, Is.EqualTo("studyGroupId"));
            Assert.That(exception.Message, Does.Contain("ID cannot be negative"));
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_StudyGroup_When_Name_Is_Too_Short()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(8, "Bio", Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_StudyGroup_When_Name_Is_Too_Long()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(9, "TheLongestStudyGroupNameEverCreatedAndBeyond", Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_StudyGroup_When_Name_Is_Empty_Or_Null()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(15, "", Subject.Chemistry, DateTime.Now, new List<User>())
            );

            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(16, null, Subject.Chemistry, DateTime.Now, new List<User>())
            );
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        public void Should_Accept_StudyGroup_When_Name_Has_Exactly_5_Characters()
        {
            var studyGroup = new StudyGroupBuilder().WithId(20).WithName("Alpha").WithSubject(Subject.Physics).Build();
            Assert.AreEqual("Alpha", studyGroup.Name);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        public void Should_Accept_StudyGroup_When_Name_Has_Exactly_30_Characters()
        {
            var validName = "StudyGroupNameWith30Characters";
            var studyGroup = new StudyGroupBuilder().WithId(21).WithName(validName).WithSubject(Subject.Math).Build();
            Assert.AreEqual(validName, studyGroup.Name);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_StudyGroup_When_Subject_Is_Invalid()
        {
            var invalidSubject = (Subject)999;

            var exception = Assert.Throws<InvalidEnumArgumentException>(() =>
                new StudyGroup(70, "Genetics Research", invalidSubject, DateTime.Now, new List<User>())
            );

            Assert.That(exception.Message, Does.Contain("Invalid subject type"));
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Validation")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_StudyGroup_When_Date_Is_In_Past_Or_Far_Future()
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

        // -- User management --------------------------------------------------

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Add_User_To_StudyGroup_When_User_Is_Not_Already_Member()
        {
            var studyGroup = new StudyGroupBuilder().WithId(25).WithName("Robotics Lab").WithSubject(Subject.Physics).WithNoUsers().Build();
            var user = new UserBuilder().WithId(10).WithName("Sophia").Build();

            studyGroup.AddUser(user);

            Assert.AreEqual(1, studyGroup.Users.Count);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_Null_User_When_Adding_To_StudyGroup()
        {
            var studyGroup = new StudyGroupBuilder().WithId(30).WithName("Biology Club").WithSubject(Subject.Chemistry).WithNoUsers().Build();
            Assert.Throws<ArgumentNullException>(() => studyGroup.AddUser(null));
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_Duplicate_User_When_Adding_To_StudyGroup()
        {
            var user = new UserBuilder().WithId(11).WithName("David").Build();
            var studyGroup = new StudyGroupBuilder().WithId(35).WithName("Quantum Mechanics Group").WithSubject(Subject.Math).WithUser(user).Build();

            var exception = Assert.Throws<InvalidOperationException>(() => studyGroup.AddUser(user));
            Assert.AreEqual("User already exists in the group.", exception.Message);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_Duplicate_UserId_When_Adding_Different_User_Instance()
        {
            var existingUser = new UserBuilder().WithId(11).WithName("David").Build();
            var sameIdDifferentInstance = new UserBuilder().WithId(11).WithName("David Clone").Build();
            var studyGroup = new StudyGroupBuilder().WithId(36).WithName("Linear Algebra Group").WithSubject(Subject.Math).WithUser(existingUser).Build();

            var exception = Assert.Throws<InvalidOperationException>(() => studyGroup.AddUser(sameIdDifferentInstance));
            Assert.AreEqual("User already exists in the group.", exception.Message);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Remove_User_From_StudyGroup_When_User_Is_Member()
        {
            var user = new UserBuilder().WithId(6).WithName("John").Build();
            var studyGroup = new StudyGroupBuilder().WithId(5).WithName("Physics Club").WithSubject(Subject.Physics).WithNoUsers().Build();

            studyGroup.AddUser(user);
            studyGroup.RemoveUser(user);

            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_Null_User_When_Removing_From_StudyGroup()
        {
            var studyGroup = new StudyGroupBuilder().WithId(50).WithName("Advanced Mathematics").WithSubject(Subject.Math).WithNoUsers().Build();
            Assert.Throws<ArgumentNullException>(() => studyGroup.RemoveUser(null));
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        [NUnit.Framework.Category("Negative")]
        public void Should_Reject_NonExistent_User_When_Removing_From_StudyGroup()
        {
            var user = new UserBuilder().WithId(20).WithName("Jonathan").Build();
            var studyGroup = new StudyGroupBuilder().WithId(55).WithName("Artificial Intelligence Lab").WithSubject(Subject.Chemistry).WithNoUsers().Build();

            var exception = Assert.Throws<InvalidOperationException>(() => studyGroup.RemoveUser(user));
            Assert.AreEqual("User not found in the group.", exception.Message);
        }

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Remove_User_By_UserId_When_User_Instance_Differs()
        {
            var existingUser = new UserBuilder().WithId(21).WithName("Jonathan").Build();
            var sameIdDifferentInstance = new UserBuilder().WithId(21).WithName("Jonathan Clone").Build();
            var studyGroup = new StudyGroupBuilder().WithId(56).WithName("Data Science Group").WithSubject(Subject.Chemistry).WithUser(existingUser).Build();

            studyGroup.RemoveUser(sameIdDifferentInstance);

            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        // -- Sanity check -----------------------------------------------------

        [Test]
        [NUnit.Framework.Category("Unit")]
        public void Should_Initialize_With_Valid_State_When_All_Parameters_Are_Valid()
        {
            var users = new List<User>
            {
                new UserBuilder().WithId(35).WithName("Emma").Build(),
                new UserBuilder().WithId(40).WithName("David").Build()
            };
            var studyGroup = new StudyGroupBuilder().WithId(80).WithName("History Forum").WithSubject(Subject.Chemistry).WithUsers(users).Build();

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