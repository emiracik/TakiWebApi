using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FAQsController : ControllerBase
{
    private readonly IFAQRepository _faqRepository;

    public FAQsController(IFAQRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FAQ>>> GetAllFAQs()
    {
        var faqs = await _faqRepository.GetAllFAQsAsync();
        return Ok(faqs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FAQ>> GetFAQ(int id)
    {
        var faq = await _faqRepository.GetFAQByIdAsync(id);
        
        if (faq == null)
        {
            return NotFound();
        }

        return Ok(faq);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<FAQ>>> GetActiveFAQs()
    {
        var faqs = await _faqRepository.GetActiveFAQsAsync();
        return Ok(faqs);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<object>> GetFAQsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var faqs = await _faqRepository.GetFAQsPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _faqRepository.GetTotalFAQsCountAsync();
        
        var response = new
        {
            FAQs = faqs,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalFAQsCount()
    {
        var count = await _faqRepository.GetTotalFAQsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveFAQsCount()
    {
        var count = await _faqRepository.GetActiveFAQsCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<FAQ>>> SearchFAQsByQuestion([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        var faqs = await _faqRepository.SearchFAQsByQuestionAsync(searchTerm);
        return Ok(faqs);
    }
}
