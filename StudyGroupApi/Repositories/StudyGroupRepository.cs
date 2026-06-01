using Microsoft.EntityFrameworkCore;
using StudyGroupApi.Data;
using StudyGroupApi.Domain.Entities;

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
            if (string.IsNullOrEmpty(studyGroup.Name))
                throw new ArgumentException("Name cannot be null or empty.");
            if (studyGroup.Name.Length < 5)
                throw new ArgumentException("Name must be at least 5 characters.");
            if (studyGroup.Name.Length > 30)
                throw new ArgumentException("Name cannot exceed 30 characters.");

            var duplicate = await _dbContext.StudyGroups
                .AnyAsync(sg => sg.Name == studyGroup.Name);
            if (duplicate)
                throw new InvalidOperationException("A study group with this name already exists.");

            foreach (var user in studyGroup.Users)
            {
                var conflict = await _dbContext.StudyGroups
                    .Include(sg => sg.Users)
                    .AnyAsync(sg => sg.Subject == studyGroup.Subject
                                 && sg.Users.Any(u => u.UserId == user.UserId));
                if (conflict)
                    throw new InvalidOperationException("User cannot create multiple groups with the same subject.");
            }

            _dbContext.StudyGroups.Add(studyGroup);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<StudyGroup>> GetStudyGroups()
        {
            return await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .OrderBy(sg => sg.CreateDate)
                .ToListAsync();
        }

        public async Task<List<StudyGroup>> SearchStudyGroups(string subject)
        {
            if (!Enum.TryParse<Subject>(subject, out var subjectEnum))
                throw new ArgumentException($"Invalid subject: {subject}");

            return await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .Where(sg => sg.Subject == subjectEnum)
                .ToListAsync();
        }

        public async Task JoinStudyGroup(int studyGroupId, int userId)
        {
            var studyGroup = await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);

            if (studyGroup == null)
                throw new KeyNotFoundException($"StudyGroup with id {studyGroupId} not found.");

            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
                throw new ArgumentException($"User with id {userId} not found.");

            studyGroup.AddUser(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            var studyGroup = await _dbContext.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);

            if (studyGroup == null)
                throw new KeyNotFoundException($"StudyGroup with id {studyGroupId} not found.");

            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
                throw new ArgumentException($"User with id {userId} not found.");

            studyGroup.RemoveUser(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
