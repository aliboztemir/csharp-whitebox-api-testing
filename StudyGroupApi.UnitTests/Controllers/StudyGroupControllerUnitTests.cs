using Moq;
using Microsoft.AspNetCore.Mvc;
using StudyGroupApi.Controllers;
using StudyGroupApi.Domain.Entities;
using StudyGroupApi.Repositories;
using StudyGroupApi.UnitTests.TestSupport.Builders;

namespace StudyGroupApi.UnitTests.Controllers
{
    [TestFixture]
    [Category("Unit")]
    [Category("Controller")]
    public class StudyGroupControllerUnitTests
    {
        private Mock<IStudyGroupRepository> _mockRepo;
        private StudyGroupController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IStudyGroupRepository>();
            _controller = new StudyGroupController(_mockRepo.Object);
        }

        // -- CreateStudyGroup -------------------------------------------------

        [Test]
        [Category("Unit")]
        public async Task Should_Create_StudyGroup_When_Request_Is_Valid()
        {
            var user = new UserBuilder().WithId(1).WithName("TestUser").Build();
            var studyGroup = new StudyGroupBuilder().WithId(1).WithUser(user).Build();

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _mockRepo.Verify(repo => repo.CreateStudyGroup(studyGroup), Times.Once);
        }

        [Test]
        [Category("Unit")]
        [Category("Validation")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_Name_Is_Too_Short()
        {
            var user = new UserBuilder().WithId(2).Build();
            var studyGroup = new StudyGroupBuilder().WithId(2).WithName("Math").WithUser(user).Build();

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup))
                     .ThrowsAsync(new ArgumentException("Name is too short"));

            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            _mockRepo.Verify(repo => repo.CreateStudyGroup(studyGroup), Times.Once);
        }

        [Test]
        [Category("Unit")]
        [Category("Validation")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_Name_Is_Too_Long()
        {
            var user = new UserBuilder().WithId(3).Build();
            var studyGroup = new StudyGroupBuilder().WithId(3).WithName("ThisIsAVeryLongStudyGroupNameThatExceeds30Chars").WithUser(user).Build();

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup))
                     .ThrowsAsync(new ArgumentException("Name is too long"));

            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            _mockRepo.Verify(repo => repo.CreateStudyGroup(studyGroup), Times.Once);
        }

        [Test]
        [Category("Unit")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_StudyGroup_Is_Null()
        {
            var result = await _controller.CreateStudyGroup(null) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            _mockRepo.Verify(repo => repo.CreateStudyGroup(It.IsAny<StudyGroup>()), Times.Never);
        }

        [Test, Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public async Task Should_Create_StudyGroup_When_User_List_Is_Empty()
        {
            var studyGroup = new StudyGroupBuilder().WithId(4).WithName("Empty Users Group").WithSubject(Subject.Chemistry).WithNoUsers().Build();
            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [Category("Unit")]
        [Category("Negative")]
        public async Task Should_Reject_StudyGroup_When_User_Already_Has_Group_With_Same_Subject()
        {
            var user = new UserBuilder().WithId(1).WithName("Alice").Build();
            var secondStudyGroup = new StudyGroupBuilder().WithId(2).WithName("Advanced Math").WithUser(user).Build();

            _mockRepo.Setup(repo => repo.CreateStudyGroup(secondStudyGroup))
                     .ThrowsAsync(new InvalidOperationException("User cannot create multiple groups with the same subject."));

            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            _mockRepo.Verify(repo => repo.CreateStudyGroup(secondStudyGroup), Times.Once);
        }

        [Test, Ignore("Clarification needed - Should the system allow multiple StudyGroups with the same Subject?")]
        public async Task Should_Reject_StudyGroup_When_Same_Subject_Already_Exists_SystemWide()
        {
            var user2 = new UserBuilder().WithId(2).WithName("Bob").Build();
            var secondStudyGroup = new StudyGroupBuilder().WithId(2).WithName("Math Experts").WithUser(user2).Build();

            _mockRepo.Setup(repo => repo.CreateStudyGroup(secondStudyGroup))
                     .ThrowsAsync(new InvalidOperationException("Duplicate subject study groups are not allowed system-wide."));

            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // -- GetStudyGroups ---------------------------------------------------

        [Test]
        [Category("Unit")]
        public async Task Should_Return_All_StudyGroups_When_Groups_Exist()
        {
            var user1 = new UserBuilder().WithId(1).WithName("Alice").Build();
            var user2 = new UserBuilder().WithId(2).WithName("Bob").Build();

            var studyGroups = new List<StudyGroup>
            {
                new StudyGroupBuilder().WithId(1).WithUser(user1).Build(),
                new StudyGroupBuilder().WithId(2).WithSubject(Subject.Physics).WithName("Physics Group").WithUser(user2).Build()
            };

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(studyGroups);

            var result = await _controller.GetStudyGroups() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual(1, returnedGroups[0].Users.Count);
            Assert.AreEqual(1, returnedGroups[1].Users.Count);
            _mockRepo.Verify(repo => repo.GetStudyGroups(), Times.Once);
        }

        [Test, Ignore("Clarification needed - Should study groups be sorted by creation date?")]
        public async Task Should_Return_StudyGroups_Sorted_By_CreationDate_NeedsClarification()
        {
            // Awaiting clarification on sorting behavior
        }

        [Test]
        [Category("Unit")]
        public async Task Should_Return_StudyGroups_Sorted_By_CreationDate()
        {
            var oldGroup = new StudyGroupBuilder().WithId(1).WithName("Chemistry Club").WithSubject(Subject.Chemistry).WithCreateDate(DateTime.Now.AddDays(-5)).WithNoUsers().Build();
            var newGroup = new StudyGroupBuilder().WithId(2).WithName("Physics Club").WithSubject(Subject.Physics).WithCreateDate(DateTime.Now).WithNoUsers().Build();

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(new List<StudyGroup> { newGroup, oldGroup });

            var result = await _controller.GetStudyGroups() as OkObjectResult;
            var returnedGroups = result?.Value as List<StudyGroup>;

            Assert.IsNotNull(returnedGroups);
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual("Physics Club", returnedGroups[0].Name);
            Assert.AreEqual("Chemistry Club", returnedGroups[1].Name);
            _mockRepo.Verify(repo => repo.GetStudyGroups(), Times.Once);
        }

        // -- SearchStudyGroups ------------------------------------------------

        [Test]
        [Category("Unit")]
        public async Task Should_Return_Filtered_StudyGroups_When_Subject_Matches()
        {
            var user1 = new UserBuilder().WithId(1).WithName("Alice").Build();
            var mathGroup = new StudyGroupBuilder().WithId(1).WithUser(user1).Build();

            _mockRepo.Setup(repo => repo.SearchStudyGroups("Math"))
                     .ReturnsAsync(new List<StudyGroup> { mathGroup });

            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, ((List<StudyGroup>)result.Value).Count);
            _mockRepo.Verify(repo => repo.SearchStudyGroups("Math"), Times.Once);
        }

        [Test]
        [Category("Unit")]
        public async Task Should_Return_Empty_When_No_StudyGroup_Matches_Subject()
        {
            _mockRepo.Setup(repo => repo.SearchStudyGroups("Biology"))
                     .ReturnsAsync(new List<StudyGroup>());

            var result = await _controller.SearchStudyGroups("Biology") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, ((List<StudyGroup>)result.Value).Count);
            _mockRepo.Verify(repo => repo.SearchStudyGroups("Biology"), Times.Once);
        }

        // -- JoinStudyGroup ---------------------------------------------------

        [Test]
        [Category("Unit")]
        public async Task Should_Add_User_To_StudyGroup_When_User_Is_Not_Member()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);

            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _mockRepo.Verify(repo => repo.JoinStudyGroup(1, 1), Times.Once);
        }

        [Test]
        [Category("Unit")]
        public async Task Should_Allow_User_To_Join_StudyGroups_With_Different_Subjects()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.JoinStudyGroup(2, 1)).Returns(Task.CompletedTask);

            var result1 = await _controller.JoinStudyGroup(1, 1) as OkResult;
            var result2 = await _controller.JoinStudyGroup(2, 1) as OkResult;

            Assert.IsNotNull(result1);
            Assert.AreEqual(200, result1.StatusCode);
            Assert.IsNotNull(result2);
            Assert.AreEqual(200, result2.StatusCode);
            _mockRepo.Verify(repo => repo.JoinStudyGroup(1, 1), Times.Once);
            _mockRepo.Verify(repo => repo.JoinStudyGroup(2, 1), Times.Once);
        }

        [Test]
        [Category("Unit")]
        [Category("Negative")]
        public async Task Should_Return_NotFound_When_StudyGroup_Does_Not_Exist()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(999, 1))
                     .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            _mockRepo.Verify(repo => repo.JoinStudyGroup(999, 1), Times.Once);
        }

        // -- LeaveStudyGroup --------------------------------------------------

        [Test]
        [Category("Unit")]
        public async Task Should_Remove_User_From_StudyGroup_When_User_Is_Member()
        {
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(2, 2)).Returns(Task.CompletedTask);

            var result = await _controller.LeaveStudyGroup(2, 2) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _mockRepo.Verify(repo => repo.LeaveStudyGroup(2, 2), Times.Once);
        }

        [Test]
        [Category("Unit")]
        [Category("Negative")]
        public async Task Should_Return_BadRequest_When_User_Is_Not_In_StudyGroup()
        {
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(3, 3))
                     .ThrowsAsync(new InvalidOperationException());

            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            _mockRepo.Verify(repo => repo.LeaveStudyGroup(3, 3), Times.Once);
        }

        [Test, Ignore("Clarification needed - Can a user rejoin a group after leaving?")]
        public async Task Should_Allow_User_To_Rejoin_StudyGroup_After_Leaving()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(1, 1)).Returns(Task.CompletedTask);

            await _controller.JoinStudyGroup(1, 1);
            await _controller.LeaveStudyGroup(1, 1);
            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}