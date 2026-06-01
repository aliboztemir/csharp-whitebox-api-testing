using Microsoft.AspNetCore.Mvc;
using StudyGroupApi.Domain.Entities;
using StudyGroupApi.Repositories;

namespace StudyGroupApi.Controllers
{
    [Route("api/studygroups")]
    [ApiController]
    public class StudyGroupController : ControllerBase
    {
        private readonly IStudyGroupRepository _studyGroupRepository;

        public StudyGroupController(IStudyGroupRepository studyGroupRepository)
        {
            _studyGroupRepository = studyGroupRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudyGroup([FromBody] StudyGroup studyGroup)
        {
            if (studyGroup == null)
                return BadRequest();

            try
            {
                await _studyGroupRepository.CreateStudyGroup(studyGroup);
                return Ok();
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudyGroups()
        {
            var studyGroups = await _studyGroupRepository.GetStudyGroups();
            return Ok(studyGroups);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStudyGroups([FromQuery] string subject)
        {
            try
            {
                var studyGroups = await _studyGroupRepository.SearchStudyGroups(subject);
                return Ok(studyGroups);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinStudyGroup([FromQuery] int studyGroupId, [FromQuery] int userId)
        {
            try
            {
                await _studyGroupRepository.JoinStudyGroup(studyGroupId, userId);
                return Ok();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (ArgumentException) { return BadRequest(); }
        }

        [HttpPost("leave")]
        public async Task<IActionResult> LeaveStudyGroup([FromQuery] int studyGroupId, [FromQuery] int userId)
        {
            try
            {
                await _studyGroupRepository.LeaveStudyGroup(studyGroupId, userId);
                return Ok();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (ArgumentException) { return BadRequest(); }
            catch (InvalidOperationException) { return BadRequest(); }
        }
    }
}
