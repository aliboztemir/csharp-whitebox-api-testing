using Microsoft.EntityFrameworkCore;
using StudyGroupApi.ComponentTests.Fixtures;
using StudyGroupApi.Tests.TestSupport.Builders;
using StudyGroupApi.Domain.Entities;

namespace StudyGroupApi.ComponentTests.Repositories
{
    [TestFixture]
    [Category("Component")]
    [Category("Repository")]
    public class StudyGroupRepositoryComponentTests : DbContextFixture
    {
        // -- CreateStudyGroup -------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Persist_StudyGroup_When_Valid()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user).Build();

            await Repository.CreateStudyGroup(studyGroup);

            var saved = await DbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);
            Assert.IsNotNull(saved);
            Assert.AreEqual("Math Club", saved.Name);
            Assert.AreEqual(1, saved.Users.Count);
        }

        [Test]
        [Category("Component")]
        [Category("Validation")]
        [Category("Negative")]
        public void Should_Throw_ArgumentException_When_Name_Is_Too_Short()
        {
            Assert.Throws<ArgumentException>(() => new StudyGroupBuilder().WithId(1).WithName("Bio").Build());
        }

        [Test]
        [Category("Component")]
        [Category("Validation")]
        [Category("Negative")]
        public void Should_Throw_ArgumentException_When_Name_Is_Too_Long()
        {
            Assert.Throws<ArgumentException>(() => new StudyGroupBuilder().WithId(1).WithName("ThisIsAVeryLongStudyGroupNameExceeds30Chars").Build());
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Throw_InvalidOperationException_When_Name_Is_Duplicate()
        {
            var user1 = new UserBuilder().WithId(1).WithName("Alice").Build();
            var user2 = new UserBuilder().WithId(2).WithName("Bob").Build();
            var first = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user1).Build();
            var second = new StudyGroupBuilder().WithId(2).WithName("Math Club").WithSubject(Subject.Physics).WithUser(user2).Build();

            await Repository.CreateStudyGroup(first);

            Assert.ThrowsAsync<InvalidOperationException>(() => Repository.CreateStudyGroup(second));
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Throw_InvalidOperationException_When_User_Already_Has_Group_With_Same_Subject()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var first = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user).Build();
            var second = new StudyGroupBuilder().WithId(2).WithName("Advanced Math").WithSubject(Subject.Math).WithUser(user).Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(first);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => Repository.CreateStudyGroup(second));
        }

        // -- GetStudyGroups ---------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Return_StudyGroups_Ordered_By_CreateDate_Ascending()
        {
            var first = new StudyGroupBuilder().WithId(1).WithName("Chemistry Club").WithSubject(Subject.Chemistry).WithNoUsers().Build();
            await Task.Delay(1);
            var second = new StudyGroupBuilder().WithId(2).WithName("Physics Club").WithSubject(Subject.Physics).WithNoUsers().Build();
            await Task.Delay(1);
            var third = new StudyGroupBuilder().WithId(3).WithName("Biology Club").WithSubject(Subject.Chemistry).WithNoUsers().Build();

            await DbContext.StudyGroups.AddRangeAsync(first, second, third);
            await DbContext.SaveChangesAsync();

            var result = await Repository.GetStudyGroups();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Chemistry Club", result[0].Name);
            Assert.AreEqual("Physics Club", result[1].Name);
            Assert.AreEqual("Biology Club", result[2].Name);
        }

        // -- SearchStudyGroups ------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Return_Matching_StudyGroups_When_Subject_Is_Valid()
        {
            var mathGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithNoUsers().Build();
            var physicsGroup = new StudyGroupBuilder().WithId(2).WithName("Physics Club").WithSubject(Subject.Physics).WithNoUsers().Build();

            await DbContext.StudyGroups.AddRangeAsync(mathGroup, physicsGroup);
            await DbContext.SaveChangesAsync();

            var result = await Repository.SearchStudyGroups("Math");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Math Club", result[0].Name);
        }

        [Test]
        [Category("Component")]
        [Category("Validation")]
        [Category("Negative")]
        public void Should_Throw_ArgumentException_When_Subject_Is_Invalid()
        {
            Assert.ThrowsAsync<ArgumentException>(() => Repository.SearchStudyGroups("InvalidSubject"));
        }

        // -- JoinStudyGroup ---------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Add_User_To_StudyGroup_When_Joining()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithNoUsers().Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            await Repository.JoinStudyGroup(1, 1);

            var updated = await DbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);
            Assert.AreEqual(1, updated.Users.Count);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public void Should_Throw_KeyNotFoundException_When_StudyGroup_Not_Found_On_Join()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => Repository.JoinStudyGroup(999, 1));
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Throw_ArgumentException_When_User_Not_Found_On_Join()
        {
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithNoUsers().Build();
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<ArgumentException>(() => Repository.JoinStudyGroup(1, 999));
        }

        // -- LeaveStudyGroup --------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Remove_User_From_StudyGroup_When_Leaving()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user).Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            await Repository.LeaveStudyGroup(1, 1);

            var updated = await DbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);
            Assert.AreEqual(0, updated.Users.Count);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public void Should_Throw_KeyNotFoundException_When_StudyGroup_Not_Found_On_Leave()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => Repository.LeaveStudyGroup(999, 1));
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Throw_InvalidOperationException_When_User_Not_In_Group_On_Leave()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithNoUsers().Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => Repository.LeaveStudyGroup(1, 1));
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Throw_ArgumentException_When_User_Not_Found_On_Leave()
        {
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithNoUsers().Build();
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<ArgumentException>(() => Repository.LeaveStudyGroup(1, 999));
        }
    }
}
