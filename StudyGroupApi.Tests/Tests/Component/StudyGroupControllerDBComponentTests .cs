using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StudyGroupApi.Controllers;
using StudyGroupApi.Data;
using StudyGroupApi.Repositories;
using StudyGroupApi.Models;

namespace StudyGroupApi.Tests.Component
{
    [TestFixture]
    public class StudyGroupControllerDBComponentTests
    {
        private AppDbContext _dbContext;
        private StudyGroupRepository _repository;
        private StudyGroupController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.OpenConnection();
            _dbContext.Database.EnsureCreated();

            _repository = new StudyGroupRepository(_dbContext);
            _controller = new StudyGroupController(_repository);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.CloseConnection();
            _dbContext.Dispose();
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Save_To_Database()
        {
            // Arrange
            var user = new User(1, "TestUser");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });

            // Act
            await _controller.CreateStudyGroup(studyGroup);

            // Assert
            var savedGroup = await _dbContext.StudyGroups.FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Math Club", savedGroup.Name);
            Assert.AreEqual(1, savedGroup.Users.Count); // Ensure user is added
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Duplicate()
        {
            // Arrange
            var user = new User(1, "TestUser");
            var studyGroup1 = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });
            var studyGroup2 = new StudyGroup(2, "Math Club", Subject.Physics, DateTime.Now, new List<User> { user });

            await _controller.CreateStudyGroup(studyGroup1);

            // Act
            var result = await _controller.CreateStudyGroup(studyGroup2) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Short()
        {
            // Arrange
            var user = new User(2, "TestUser");
            var studyGroup = new StudyGroup(2, "Math", Subject.Math, DateTime.Now, new List<User> { user });

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

            // Act
            await _controller.CreateStudyGroup(studyGroup);

            // Assert
            var count = await _dbContext.StudyGroups.CountAsync();
            Assert.AreEqual(0, count, "Database should not store a StudyGroup with a name longer than 30 characters.");
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_StudyGroup_Is_Null()
        {
            // Act
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

            // Act
            await _controller.CreateStudyGroup(studyGroup);

            // Assert
            var savedGroup = await _dbContext.StudyGroups.FirstOrDefaultAsync(sg => sg.StudyGroupId == 4);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Empty Users Group", savedGroup.Name);
            Assert.AreEqual(0, savedGroup.Users.Count);
        }


        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_User_Creates_Second_Group_With_Same_Subject()
        {
            // Arrange
            var user = new User(1, "Alice");
            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });
            var secondStudyGroup = new StudyGroup(2, "Advanced Math", Subject.Math, DateTime.Now, new List<User> { user });

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(firstStudyGroup);
            await _dbContext.SaveChangesAsync();

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

            await _dbContext.Users.AddRangeAsync(user1, user2);
            await _dbContext.StudyGroups.AddAsync(firstStudyGroup);
            await _dbContext.SaveChangesAsync();

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

            await _dbContext.Users.AddRangeAsync(user1, user2);
            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

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

        [Test, Category("GetStudyGroups")]
        public async Task GetStudyGroups_Should_Return_Sorted_By_CreationDate()
        {
            var firstGroup = new StudyGroup(1, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());
            await Task.Delay(1);
            var secondGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());
            await Task.Delay(1);
            var thirdGroup = new StudyGroup(3, "Biology Club", Subject.Chemistry, DateTime.Now, new List<User>());

            await _dbContext.StudyGroups.AddRangeAsync(firstGroup, secondGroup, thirdGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetStudyGroups() as OkObjectResult;
            var studyGroups = result?.Value as List<StudyGroup>;

            // Assert
            Assert.IsNotNull(studyGroups);
            Assert.AreEqual(3, studyGroups.Count);
            Assert.AreEqual("Chemistry Club", studyGroups[0].Name);
            Assert.AreEqual("Physics Club", studyGroups[1].Name);
            Assert.AreEqual("Biology Club", studyGroups[2].Name);
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

            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, ((List<StudyGroup>)result.Value).Count);
        }

        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Empty_If_No_Match()
        {
            // Arrange
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { new User(1, "Alice") }),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { new User(2, "Bob") })
            };

            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.SearchStudyGroups("Chemistry") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, ((List<StudyGroup>)result.Value).Count);
        }

        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_BadRequest_If_Subject_Is_Invalid()
        {
            // Act
            var result = await _controller.SearchStudyGroups("InvalidSubject") as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Add_User_To_Group()
        {
            // Arrange
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            await _controller.JoinStudyGroup(1, 1);

            // Assert
            var updatedGroup = await _dbContext.StudyGroups.Include(sg => sg.Users)
                                 .FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(1, updatedGroup.Users.Count);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Allow_User_To_Join_Different_Subjects()
        {
            // Arrange
            var user = new User(1, "Alice");

            var mathStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            var physicsStudyGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddRangeAsync(mathStudyGroup, physicsStudyGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            await _controller.JoinStudyGroup(1, 1);
            await _controller.JoinStudyGroup(2, 1);

            // Assert
            var mathGroup = await _dbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);
            var physicsGroup = await _dbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(mathGroup);
            Assert.IsNotNull(physicsGroup);
            Assert.IsTrue(mathGroup.Users.Any(u => u.UserId == 1));
            Assert.IsTrue(physicsGroup.Users.Any(u => u.UserId == 1));
        }


        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            // Act
            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Return_BadRequest_If_UserId_Is_Invalid()
        {
            // Arrange
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.JoinStudyGroup(1, 999) as BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }


        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Remove_User_From_Group()
        {
            // Arrange
            var user = new User(2, "Bob");
            var studyGroup = new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user });

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            await _controller.LeaveStudyGroup(2, 2);

            // Assert
            var updatedGroup = await _dbContext.StudyGroups.Include(sg => sg.Users)
                                 .FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(0, updatedGroup.Users.Count);
        }

        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_User_Not_In_Group()
        {
            // Arrange
            var user = new User(3, "Charlie");
            var studyGroup = new StudyGroup(3, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_UserId_Is_Invalid()
        {
            // Arrange
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.LeaveStudyGroup(1, 999) as BadRequestResult;

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

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

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
