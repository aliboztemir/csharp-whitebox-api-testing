using Microsoft.EntityFrameworkCore;
using StudyGroupApi.Data;
using StudyGroupApi.Models;

namespace StudyGroupApi.Repositories
{
    public class StudyGroupRepository : IStudyGroupRepository
    {
        private readonly AppDbContext _dbContext;

        public StudyGroupRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateStudyGroup(StudyGroup studyGroup)
        {
            _dbContext.StudyGroups.Add(studyGroup);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<StudyGroup>> GetStudyGroups()
        {
            return await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .ToListAsync();
        }

        public async Task<List<StudyGroup>> SearchStudyGroups(string subject)
        {
            return await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .Where(sg => sg.Subject.ToString() == subject)
                .ToListAsync();
        }

        public async Task JoinStudyGroup(int studyGroupId, int userId)
        {
            var studyGroup = await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);

            var user = await _dbContext.Users.FindAsync(userId);

            studyGroup.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            var studyGroup = await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);

            var user = await _dbContext.Users.FindAsync(userId);

            studyGroup.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
