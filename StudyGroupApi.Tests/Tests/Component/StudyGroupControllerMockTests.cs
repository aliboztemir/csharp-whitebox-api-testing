using Moq;
using Microsoft.AspNetCore.Mvc;
using StudyGroupApi.Controllers;
using StudyGroupApi.Models;
using StudyGroupApi.Repositories;

namespace StudyGroupApi.Tests.Component
{
    [TestFixture]
    public class StudyGroupControllerMockTests
    {
        private Mock<IStudyGroupRepository> _mockRepo;
        private StudyGroupController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IStudyGroupRepository>();
            _controller = new StudyGroupController(_mockRepo.Object);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_Ok()
        {
            // Arrange
            var user = new User(1, "TestUser");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Short()
        {
            // Arrange
            var user = new User(2, "TestUser");
            var studyGroup = new StudyGroup(2, "Math", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).ThrowsAsync(new ArgumentException("Name is too short"));

            // Act
            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Long()
        {
            // Arrange
            var user = new User(3, "TestUser");
            var studyGroup = new StudyGroup(3, "ThisIsAVeryLongStudyGroupNameThatExceeds30Chars", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).ThrowsAsync(new ArgumentException("Name is too long"));

            // Act
            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_StudyGroup_Is_Null()
        {
            // Act: Call the controller method with a null StudyGroup
            var result = await _controller.CreateStudyGroup(null) as BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup"), Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public async Task CreateStudyGroup_Should_Allow_Empty_User_List()
        {
            // Arrange
            var studyGroup = new StudyGroup(4, "Empty Users Group", Subject.Chemistry, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_User_Creates_Second_Group_With_Same_Subject()
        {
            // Arrange
            var user = new User(1, "Alice");
            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });
            var secondStudyGroup = new StudyGroup(2, "Advanced Math", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(secondStudyGroup))
                     .ThrowsAsync(new InvalidOperationException("User cannot create multiple groups with the same subject."));

            // Act
            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup"), Ignore("Clarification needed - Should the system allow multiple StudyGroups with the same Subject?")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_Subject_Already_Exists_SystemWide()
        {
            // Arrange
            var user1 = new User(1, "Alice");
            var user2 = new User(2, "Bob");

            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user1 });
            var secondStudyGroup = new StudyGroup(2, "Math Experts", Subject.Math, DateTime.Now, new List<User> { user2 });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(secondStudyGroup))
                     .ThrowsAsync(new InvalidOperationException("Duplicate subject study groups are not allowed system-wide."));

            // Act
            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("GetStudyGroups")]
        public async Task GetStudyGroups_Should_Return_All_Groups()
        {
            // Arrange
            var user1 = new User(1, "Alice");
            var user2 = new User(2, "Bob");

            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user1 }),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user2 })
            };

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(studyGroups);

            // Act
            var result = await _controller.GetStudyGroups() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual(1, returnedGroups[0].Users.Count);
            Assert.AreEqual(1, returnedGroups[1].Users.Count);
        }

        [Test, Category("GetStudyGroups"), Ignore("Clarification needed - Should study groups be sorted by creation date?")]
        public async Task GetStudyGroups_NeedsClarification_Should_Return_Sorted_By_CreationDate()
        {
            // Awaiting clarification on sorting behavior
        }

        [Test, Category("GetStudyGroups")]
        public async Task GetStudyGroups_Should_Return_Sorted_By_CreationDate()
        {
            // Arrange
            var oldGroup = new StudyGroup(1, "Chemistry Club", Subject.Chemistry, DateTime.Now.AddDays(-5), new List<User>());
            var newGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            var studyGroups = new List<StudyGroup> { newGroup, oldGroup };

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(studyGroups);

            // Act
            var result = await _controller.GetStudyGroups() as OkObjectResult;
            var returnedGroups = result?.Value as List<StudyGroup>;

            // Assert
            Assert.IsNotNull(returnedGroups);
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual("Physics Club", returnedGroups[0].Name);
            Assert.AreEqual("Chemistry Club", returnedGroups[1].Name);
        }

        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Filtered_Groups()
        {
            // Arrange
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { new User(1, "Alice") }),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { new User(2, "Bob") })
            };

            _mockRepo.Setup(repo => repo.SearchStudyGroups("Math"))
                     .ReturnsAsync(studyGroups.Where(sg => sg.Subject == Subject.Math).ToList());

            // Act
            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(1, returnedGroups.Count);
            Assert.AreEqual("Math Club", returnedGroups[0].Name);
        }

        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Empty_If_No_Match()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.SearchStudyGroups("Biology"))
                     .ReturnsAsync(new List<StudyGroup>());

            // Act
            var result = await _controller.SearchStudyGroups("Biology") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(0, returnedGroups.Count);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Add_User_To_Group()
        {
            // Arrange
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Allow_User_To_Join_Different_Subjects()
        {
            // Arrange
            var user = new User(1, "Alice");
            var mathStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            var physicsStudyGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.JoinStudyGroup(2, 1)).Returns(Task.CompletedTask);

            // Act
            var result1 = await _controller.JoinStudyGroup(1, 1) as OkResult;
            var result2 = await _controller.JoinStudyGroup(2, 1) as OkResult;

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(200, result1.StatusCode);
            Assert.IsNotNull(result2);
            Assert.AreEqual(200, result2.StatusCode);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.JoinStudyGroup(999, 1))
                     .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Remove_User_From_Group()
        {
            // Arrange
            var user = new User(2, "Bob");
            var studyGroup = new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.LeaveStudyGroup(2, 2)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.LeaveStudyGroup(2, 2) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_User_Not_In_Group()
        {
            // Arrange
            var user = new User(3, "Charlie");
            var studyGroup = new StudyGroup(3, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.LeaveStudyGroup(3, 3)).ThrowsAsync(new InvalidOperationException());

            // Act
            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("LeaveStudyGroup"), Ignore("Clarification needed - Can a user rejoin a group after leaving?")]
        public async Task LeaveStudyGroup_Should_Allow_User_To_Rejoin_After_Leaving()
        {
            // Arrange
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(1, 1)).Returns(Task.CompletedTask);

            // Act
            await _controller.JoinStudyGroup(1, 1);
            await _controller.LeaveStudyGroup(1, 1);
            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
