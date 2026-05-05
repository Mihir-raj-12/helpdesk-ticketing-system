using HelpDesk.Core.DTOs.KnowledgeBase;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpDesk.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All KB endpoints require the user to be logged in
    public class KbArticlesController : ControllerBase
    {
        private readonly IKbArticleService _kbArticleService;

        public KbArticlesController(IKbArticleService kbArticleService)
        {
            _kbArticleService = kbArticleService;
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery]int id)
        {
            var response = await _kbArticleService.GetArticleByIdAsync(id);
            if (!response.IsSuccess) return NotFound(response);

            return Ok(response);
        }

        [HttpGet("published")]
        public async Task<IActionResult> GetAllPublished()
        {
            var response = await _kbArticleService.GetAllPublishedAsync();
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SupportAgent")]
        public async Task<IActionResult> Create([FromBody] CreateKbArticleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Look how clean this is now!
            var response = await _kbArticleService.CreateArticleAsync(dto);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("updateArticle")]
        [Authorize(Roles = "Admin,SupportAgent")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateKbArticleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Completely abstracted away!
            var response = await _kbArticleService.UpdateArticleAsync(id, dto);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("feedback")]
        [Authorize(Roles = "RegularUser,SupportAgent")] // PRD says Regular Users can submit feedback
        public async Task<IActionResult> SubmitFeedback([FromQuery]int id, [FromQuery] bool isHelpful)
        {
            var response = await _kbArticleService.SubmitFeedbackAsync(id, isHelpful);
            if (!response.IsSuccess) return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var response = await _kbArticleService.SearchArticlesAsync(keyword);

            // If no articles are found, it returns a 404 Not Found (which is standard REST practice for empty searches)
            if (!response.IsSuccess) return NotFound(response);

            return Ok(response);
        }


    }
    
}
