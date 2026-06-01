using StudyGroupApi.Domain.Entities;

namespace StudyGroupApi.Contracts.Requests
{
    public class CreateStudyGroupRequest
    {
        public int StudyGroupId { get; init; }
        public string? Name { get; init; }
        public Subject Subject { get; init; }
        public DateTime CreateDate { get; init; }
        public List<CreateStudyGroupUserRequest>? Users { get; init; }
    }

    public class CreateStudyGroupUserRequest
    {
        public int UserId { get; init; }
        public string? Name { get; init; }
    }
}
