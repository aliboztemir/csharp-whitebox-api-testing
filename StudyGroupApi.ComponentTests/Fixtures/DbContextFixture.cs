using Microsoft.EntityFrameworkCore;
using StudyGroupApi.Data;
using StudyGroupApi.Repositories;

namespace StudyGroupApi.ComponentTests.Fixtures
{
    public abstract class DbContextFixture
    {
        protected AppDbContext DbContext { get; private set; }
        protected StudyGroupRepository Repository { get; private set; }

        [SetUp]
        public void SetUpFixture()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options;

            DbContext = new AppDbContext(options);
            DbContext.Database.OpenConnection();
            DbContext.Database.EnsureCreated();

            Repository = new StudyGroupRepository(DbContext);
        }

        [TearDown]
        public void TearDownFixture()
        {
            DbContext.Database.CloseConnection();
            DbContext.Dispose();
        }
    }
}
