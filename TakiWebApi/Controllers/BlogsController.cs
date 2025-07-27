using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;
using TakiWebApi.Services;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BlogsController : ControllerBase
{
    private readonly IBlogRepository _blogRepository;
    private readonly IJwtService _jwtService;

    public BlogsController(IBlogRepository blogRepository, IJwtService jwtService)
    {
        _blogRepository = blogRepository;
        _jwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Blog>>> GetAllBlogs()
    {
        var blogs = await _blogRepository.GetAllBlogsAsync();
        return Ok(blogs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Blog>> GetBlog(int id)
    {
        var blog = await _blogRepository.GetBlogByIdAsync(id);
        
        if (blog == null)
        {
            return NotFound();
        }

        return Ok(blog);
    }

    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<Blog>>> GetPublishedBlogs()
    {
        var blogs = await _blogRepository.GetPublishedBlogsAsync();
        return Ok(blogs);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Blog>>> GetActiveBlogs()
    {
        var blogs = await _blogRepository.GetActiveBlogsAsync();
        return Ok(blogs);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<object>> GetBlogsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var blogs = await _blogRepository.GetBlogsPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _blogRepository.GetTotalBlogsCountAsync();
        
        var response = new
        {
            Blogs = blogs,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalBlogsCount()
    {
        var count = await _blogRepository.GetTotalBlogsCountAsync();
        return Ok(count);
    }

    [HttpGet("published/count")]
    public async Task<ActionResult<int>> GetPublishedBlogsCount()
    {
        var count = await _blogRepository.GetPublishedBlogsCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Blog>>> SearchBlogsByTitle([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        var blogs = await _blogRepository.SearchBlogsByTitleAsync(searchTerm);
        return Ok(blogs);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Blog>>> GetBlogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var blogs = await _blogRepository.GetBlogsByDateRangeAsync(startDate, endDate);
        return Ok(blogs);
    }
}
