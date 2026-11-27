using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PotetoBot.API.Data;
using PotetoBot.API.Entities;
using PotetoBot.API.Services;
using PotetoBot.Shared.DTOs;

namespace PotetoBot.API.Controller;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly DBContext _dbcontext;
    private readonly ChatServices _chatService;

    public ChatController(DBContext dbcontext, ChatServices chatService)
    {
        _dbcontext = dbcontext;
        _chatService = chatService;
    }


    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation()
    {
        Conversation conversation = new Conversation
        { 
            Title = "New Conversation"
        };

        _dbcontext.Conversations.Add(conversation);

        await _dbcontext.SaveChangesAsync();

        return Ok(new { conversation.Id });
    }


    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        if (request.Message.Length > 4000)
        {
            return BadRequest("Message is too long.");
        }

        try
        {
            string response = await _chatService.SendMessageAsync(request.ConversationId, request.Message);

            return Ok(new ChatResponseDto
            {
                ConversationId = request.ConversationId,
                UserMessage = request.Message,
                AssistantMessage = response
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        var messages = await _dbcontext.ChatMessages
            .Where(x => x.ConversationId == id)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(messages);
    }

}
