using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StudyGroupApi.Controllers;
using StudyGroupApi.Domain.Entities;
using StudyGroupApi.ComponentTests.Fixtures;
using StudyGroupApi.ComponentTests.TestSupport.Builders;

namespace StudyGroupApi.ComponentTests.Controllers
{
    [TestFixture]
    [Category("Component")]
    [Category("Controller")]
    public class StudyGroupControllerDBComponentTests : DbContextFixture
    {
        private StudyGroupController _controller;

        [SetUp]
        public void SetupController()
        {
            _controller = new StudyGroupController(Repository);
        }

        // -- CreateStudyGroup -------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Persist_StudyGroup_To_Database_When_Request_Is_Valid()
        {
            var user = new UserBuilder().WithId(1).WithName("TestUser").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithUser(user).Build();

            await _controller.CreateStudyGroup(studyGroup);

            var savedGroup = await DbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Math Club", savedGroup.Name);
            Assert.AreEqual(1, savedGroup.Users.Count);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_Name_Is_Duplicate()
        {
            var user = new UserBuilder().WithId(1).WithName("TestUser").Build();
            var studyGroup1 = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user).Build();
            var studyGroup2 = new StudyGroupBuilder().WithId(2).WithName("Math Club").WithSubject(Subject.Physics).WithUser(user).Build();

            await _controller.CreateStudyGroup(studyGroup1);

            var result = await _controller.CreateStudyGroup(studyGroup2) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        [Category("Component")]
        [Category("Validation")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_Name_Is_Too_Short()
        {
            var user = new UserBuilder().WithId(2).Build();
            var studyGroup = new StudyGroupBuilder().WithId(2).WithName("Math").WithUser(user).Build();

            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        [Category("Component")]
        [Category("Validation")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_Name_Exceeds_30_Characters()
        {
            var user = new UserBuilder().WithId(3).Build();
            var studyGroup = new StudyGroupBuilder().WithId(3).WithName("ThisIsAVeryLongStudyGroupNameThatExceeds30Chars").WithUser(user).Build();

            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_StudyGroup_Is_Null()
        {
            var result = await _controller.CreateStudyGroup(null) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public async Task Should_Create_StudyGroup_When_User_List_Is_Empty()
        {
            var studyGroup = new StudyGroupBuilder().WithId(4).WithName("Empty Users Group").WithSubject(Subject.Chemistry).WithNoUsers().Build();

            await _controller.CreateStudyGroup(studyGroup);

            var savedGroup = await DbContext.StudyGroups.FirstOrDefaultAsync(sg => sg.StudyGroupId == 4);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Empty Users Group", savedGroup.Name);
            Assert.AreEqual(0, savedGroup.Users.Count);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_User_Already_Has_Group_With_Same_Subject()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var firstStudyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user).Build();
            var secondStudyGroup = new StudyGroupBuilder().WithId(2).WithName("Advanced Math").WithSubject(Subject.Math).WithUser(user).Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(firstStudyGroup);
            await DbContext.SaveChangesAsync();

            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Ignore("Clarification needed - Should the system allow multiple StudyGroups with the same Subject?")]
        public async Task Should_Reject_StudyGroup_When_Same_Subject_Already_Exists_SystemWide()
        {
            var user1 = new UserBuilder().WithId(1).WithName("Alice").Build();
            var user2 = new UserBuilder().WithId(2).WithName("Bob").Build();

            var firstStudyGroup = new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user1).Build();
            var secondStudyGroup = new StudyGroupBuilder().WithId(2).WithName("Math Experts").WithSubject(Subject.Math).WithUser(user2).Build();

            await DbContext.Users.AddRangeAsync(user1, user2);
            await DbContext.StudyGroups.AddAsync(firstStudyGroup);
            await DbContext.SaveChangesAsync();

            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // -- GetStudyGroups ---------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Return_All_StudyGroups_With_Users_When_Groups_Exist()
        {
            var user1 = new UserBuilder().WithId(1).WithName("Alice").Build();
            var user2 = new UserBuilder().WithId(2).WithName("Bob").Build();

            var studyGroups = new List<StudyGroup>
            {
                new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(user1).Build(),
                new StudyGroupBuilder().WithId(2).WithName("Physics Group").WithSubject(Subject.Physics).WithUser(user2).Build()
            };

            await DbContext.Users.AddRangeAsync(user1, user2);
            await DbContext.StudyGroups.AddRangeAsync(studyGroups);
            await DbContext.SaveChangesAsync();

            var result = await _controller.GetStudyGroups() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual(1, returnedGroups[0].Users.Count);
            Assert.AreEqual(1, returnedGroups[1].Users.Count);
        }

        [Test]
        [Category("Component")]
        public async Task Should_Return_StudyGroups_Sorted_By_CreationDate()
        {
            var firstGroup = new StudyGroupBuilder().WithId(1).WithName("Chemistry Club").WithSubject(Subject.Chemistry).WithNoUsers().Build();
            await Task.Delay(1);
            var secondGroup = new StudyGroupBuilder().WithId(2).WithName("Physics Club").WithSubject(Subject.Physics).WithNoUsers().Build();
            await Task.Delay(1);
            var thirdGroup = new StudyGroupBuilder().WithId(3).WithName("Biology Club").WithSubject(Subject.Chemistry).WithNoUsers().Build();

            await DbContext.StudyGroups.AddRangeAsync(firstGroup, secondGroup, thirdGroup);
            await DbContext.SaveChangesAsync();

            var result = await _controller.GetStudyGroups() as OkObjectResult;
            var studyGroups = result?.Value as List<StudyGroup>;

            Assert.IsNotNull(studyGroups);
            Assert.AreEqual(3, studyGroups.Count);
            Assert.AreEqual("Chemistry Club", studyGroups[0].Name);
            Assert.AreEqual("Physics Club", studyGroups[1].Name);
            Assert.AreEqual("Biology Club", studyGroups[2].Name);
        }

        // -- SearchStudyGroups ------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Return_Filtered_StudyGroups_When_Subject_Matches()
        {
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(new UserBuilder().WithId(1).WithName("Alice").Build()).Build(),
                new StudyGroupBuilder().WithId(2).WithName("Physics Group").WithSubject(Subject.Physics).WithUser(new UserBuilder().WithId(2).WithName("Bob").Build()).Build()
            };

            await DbContext.StudyGroups.AddRangeAsync(studyGroups);
            await DbContext.SaveChangesAsync();

            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, ((List<StudyGroup>)result.Value).Count);
        }

        [Test]
        [Category("Component")]
        public async Task Should_Return_Empty_When_No_StudyGroup_Matches_Subject()
        {
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroupBuilder().WithId(1).WithName("Math Club").WithSubject(Subject.Math).WithUser(new UserBuilder().WithId(1).WithName("Alice").Build()).Build(),
                new StudyGroupBuilder().WithId(2).WithName("Physics Group").WithSubject(Subject.Physics).WithUser(new UserBuilder().WithId(2).WithName("Bob").Build()).Build()
            };

            await DbContext.StudyGroups.AddRangeAsync(studyGroups);
            await DbContext.SaveChangesAsync();

            var result = await _controller.SearchStudyGroups("Chemistry") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, ((List<StudyGroup>)result.Value).Count);
        }

        [Test]
        [Category("Component")]
        [Category("Validation")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_Subject_Is_Invalid()
        {
            var result = await _controller.SearchStudyGroups("InvalidSubject") as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // -- JoinStudyGroup ---------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Add_User_To_StudyGroup_When_User_Is_Not_Member()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithNoUsers().Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            await _controller.JoinStudyGroup(1, 1);

            var updatedGroup = await DbContext.StudyGroups.Include(sg => sg.Users)
                                 .FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(1, updatedGroup.Users.Count);
        }

        [Test]
        [Category("Component")]
        public async Task Should_Allow_User_To_Join_StudyGroups_With_Different_Subjects()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var mathStudyGroup = new StudyGroupBuilder().WithId(1).WithSubject(Subject.Math).WithNoUsers().Build();
            var physicsStudyGroup = new StudyGroupBuilder().WithId(2).WithName("Physics Club").WithSubject(Subject.Physics).WithNoUsers().Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddRangeAsync(mathStudyGroup, physicsStudyGroup);
            await DbContext.SaveChangesAsync();

            await _controller.JoinStudyGroup(1, 1);
            await _controller.JoinStudyGroup(2, 1);

            var mathGroup = await DbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);
            var physicsGroup = await DbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(mathGroup);
            Assert.IsNotNull(physicsGroup);
            Assert.IsTrue(mathGroup.Users.Any(u => u.UserId == 1));
            Assert.IsTrue(physicsGroup.Users.Any(u => u.UserId == 1));
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Return_NotFound_When_StudyGroup_Does_Not_Exist()
        {
            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_UserId_Does_Not_Exist_On_Join()
        {
            var studyGroup = new StudyGroupBuilder().WithId(1).WithNoUsers().Build();
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            var result = await _controller.JoinStudyGroup(1, 999) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // -- LeaveStudyGroup --------------------------------------------------

        [Test]
        [Category("Component")]
        public async Task Should_Remove_User_From_StudyGroup_When_User_Is_Member()
        {
            var user = new UserBuilder().WithId(2).WithName("Bob").Build();
            var studyGroup = new StudyGroupBuilder().WithId(2).WithName("Physics Group").WithSubject(Subject.Physics).WithUser(user).Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            await _controller.LeaveStudyGroup(2, 2);

            var updatedGroup = await DbContext.StudyGroups.Include(sg => sg.Users)
                                 .FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(0, updatedGroup.Users.Count);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_User_Is_Not_In_StudyGroup()
        {
            var user = new UserBuilder().WithId(3).WithName("Charlie").Build();
            var studyGroup = new StudyGroupBuilder().WithId(3).WithName("Chemistry Club").WithSubject(Subject.Chemistry).WithNoUsers().Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        [Category("Component")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_UserId_Does_Not_Exist_On_Leave()
        {
            var studyGroup = new StudyGroupBuilder().WithId(1).WithNoUsers().Build();
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            var result = await _controller.LeaveStudyGroup(1, 999) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Ignore("Clarification needed - Can a user rejoin a group after leaving?")]
        public async Task Should_Allow_User_To_Rejoin_StudyGroup_After_Leaving()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithNoUsers().Build();

            await DbContext.Users.AddAsync(user);
            await DbContext.StudyGroups.AddAsync(studyGroup);
            await DbContext.SaveChangesAsync();

            await _controller.JoinStudyGroup(1, 1);
            await _controller.LeaveStudyGroup(1, 1);
            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
