using TakiWebApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserCreditCardsController : ControllerBase
{
    private readonly IUserCreditCardRepository _userCreditCardRepository;

    public UserCreditCardsController(IUserCreditCardRepository userCreditCardRepository)
    {
        _userCreditCardRepository = userCreditCardRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserCreditCard>>> GetAllUserCreditCards()
    {
        var userCreditCards = await _userCreditCardRepository.GetAllUserCreditCardsAsync();
        return Ok(userCreditCards);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserCreditCard>> GetUserCreditCard(int id)
    {
        var userCreditCard = await _userCreditCardRepository.GetUserCreditCardByIdAsync(id);
        
        if (userCreditCard == null)
        {
            return NotFound();
        }
        if (!User.IsPassenger(userCreditCard.UserID))
        {
            return Forbid();
        }

        return Ok(userCreditCard);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserCreditCard>>> GetUserCreditCardsByUserId(int userId)
    {
        var userCreditCards = await _userCreditCardRepository.GetUserCreditCardsByUserIdAsync(userId);
        return Ok(userCreditCards);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserCreditCard>>> GetActiveUserCreditCards()
    {
        var userCreditCards = await _userCreditCardRepository.GetActiveUserCreditCardsAsync();
        return Ok(userCreditCards);
    }

    [HttpGet("paginated")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserCreditCard>>> GetUserCreditCardsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var userCreditCards = await _userCreditCardRepository.GetUserCreditCardsPaginatedAsync(pageNumber, pageSize);
        return Ok(userCreditCards);
    }

    [HttpGet("count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetTotalUserCreditCardsCount()
    {
        var count = await _userCreditCardRepository.GetTotalUserCreditCardsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetActiveUserCreditCardsCount()
    {
        var count = await _userCreditCardRepository.GetActiveUserCreditCardsCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserCreditCard>>> SearchUserCreditCardsByName([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty.");
        }

        var userCreditCards = await _userCreditCardRepository.SearchUserCreditCardsByNameAsync(searchTerm);
        return Ok(userCreditCards);
    }

    [HttpGet("date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserCreditCard>>> GetUserCreditCardsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date.");
        }

        var userCreditCards = await _userCreditCardRepository.GetUserCreditCardsByCreatedDateRangeAsync(startDate, endDate);
        return Ok(userCreditCards);
    }

    [HttpPost]
    public async Task<ActionResult<UserCreditCard>> CreateUserCreditCard([FromBody] UserCreditCard userCreditCard)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!User.IsPassenger(userCreditCard.UserID))
        {
            return Forbid();
        }

        // CardType is now required in the model and will be passed from the client
        try
        {
            var cardId = await _userCreditCardRepository.CreateUserCreditCardAsync(userCreditCard);
            var createdUserCreditCard = await _userCreditCardRepository.GetUserCreditCardByIdAsync(cardId);
            return CreatedAtAction(nameof(GetUserCreditCard), new { id = cardId }, createdUserCreditCard);
        }
        catch
        {
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserCreditCard(int id, [FromBody] UserCreditCard userCreditCard)
    {
        if (id != userCreditCard.CardID)
        {
            return BadRequest("ID mismatch between route and body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // CardType is now required in the model and will be passed from the client
        try
        {
            var existingUserCreditCard = await _userCreditCardRepository.GetUserCreditCardByIdAsync(id);
            if (existingUserCreditCard == null)
            {
                return NotFound();
            }
            if (!User.IsPassenger(existingUserCreditCard.UserID))
            {
                return Forbid();
            }

            userCreditCard.UserID = existingUserCreditCard.UserID;
            userCreditCard.UpdatedDate = DateTime.UtcNow;
            var success = await _userCreditCardRepository.UpdateUserCreditCardAsync(userCreditCard);
            if (!success)
            {
                return StatusCode(500, "Failed to update user credit card.");
            }
            return NoContent();
        }
        catch
        {
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserCreditCard(int id)
    {
        try
        {
            var existingUserCreditCard = await _userCreditCardRepository.GetUserCreditCardByIdAsync(id);
            if (existingUserCreditCard == null)
            {
                return NotFound();
            }
            if (!User.IsPassenger(existingUserCreditCard.UserID))
            {
                return Forbid();
            }

            var success = await _userCreditCardRepository.DeleteUserCreditCardAsync(id);
            
            if (!success)
            {
                return StatusCode(500, "Failed to delete user credit card.");
            }

            return NoContent();
        }
        catch
        {
            throw;
        }
    }
}
