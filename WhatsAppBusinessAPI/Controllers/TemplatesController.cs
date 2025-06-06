using Microsoft.AspNetCore.Mvc;
using WhatsAppBusinessAPI.Models;
using WhatsAppBusinessAPI.Repositories;
using System.Text;
using System.Text.Json;
using Dapper;

namespace WhatsAppBusinessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ChatRepository _chatRepository;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(ChatRepository chatRepository, ILogger<TemplatesController> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        // GET: api/templates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageTemplate>>> GetTemplates()
        {
            try
            {
                var templates = await _chatRepository.GetMessageTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message templates");
                return StatusCode(500, "Internal server error while retrieving templates.");
            }
        }

        // GET: api/templates/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageTemplate>> GetTemplate(int id)
        {
            try
            {
                var template = await _chatRepository.GetMessageTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Template with ID {id} not found.");
                }
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
                return StatusCode(500, "Internal server error while retrieving template.");
            }
        }

        // GET: api/templates/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<MessageTemplate>> GetDefaultTemplateByCategory(string category)
        {
            try
            {
                var template = await _chatRepository.GetDefaultTemplateByCategory(category);
                if (template == null)
                {
                    return NotFound($"No default template found for category '{category}'.");
                }
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default template for category {Category}", category);
                return StatusCode(500, "Internal server error while retrieving template.");
            }
        }

        // POST: api/templates
        [HttpPost]
        public async Task<ActionResult<MessageTemplate>> CreateTemplate([FromBody] CreateTemplateRequest request)
        {
            try
            {
                var template = new MessageTemplate
                {
                    Name = request.Name,
                    Description = request.Description,
                    TemplateText = request.TemplateText,
                    Category = request.Category,
                    IsActive = request.IsActive,
                    IsDefault = request.IsDefault,
                    PlaceholderVariables = request.PlaceholderVariables
                };

                var templateId = await _chatRepository.SaveMessageTemplateAsync(template);
                template.Id = templateId;

                _logger.LogInformation("Created new message template: {TemplateName}", template.Name);
                return CreatedAtAction(nameof(GetTemplate), new { id = templateId }, template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message template");
                return StatusCode(500, "Internal server error while creating template.");
            }
        }

        // PUT: api/templates/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateTemplateRequest request)
        {
            try
            {
                var existingTemplate = await _chatRepository.GetMessageTemplateByIdAsync(id);
                if (existingTemplate == null)
                {
                    return NotFound($"Template with ID {id} not found.");
                }

                existingTemplate.Name = request.Name;
                existingTemplate.Description = request.Description;
                existingTemplate.TemplateText = request.TemplateText;
                existingTemplate.Category = request.Category;
                existingTemplate.IsActive = request.IsActive;
                existingTemplate.IsDefault = request.IsDefault;
                existingTemplate.PlaceholderVariables = request.PlaceholderVariables;

                await _chatRepository.SaveMessageTemplateAsync(existingTemplate);

                _logger.LogInformation("Updated message template: {TemplateName}", existingTemplate.Name);
                return Ok(existingTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template {TemplateId}", id);
                return StatusCode(500, "Internal server error while updating template.");
            }
        }

        // DELETE: api/templates/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try
            {
                var template = await _chatRepository.GetMessageTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Template with ID {id} not found.");
                }

                await _chatRepository.DeleteMessageTemplateAsync(id);

                _logger.LogInformation("Deleted message template: {TemplateName}", template.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {TemplateId}", id);
                return StatusCode(500, "Internal server error while deleting template.");
            }
        }

        // POST: api/templates/{id}/preview
        [HttpPost("{id}/preview")]
        public async Task<ActionResult<string>> PreviewTemplate(int id, [FromBody] Dictionary<string, string> variables)
        {
            try
            {
                var template = await _chatRepository.GetMessageTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Template with ID {id} not found.");
                }

                var previewText = ProcessTemplate(template.TemplateText, variables);
                return Ok(new { preview = previewText });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing template {TemplateId}", id);
                return StatusCode(500, "Internal server error while previewing template.");
            }
        }

        private string ProcessTemplate(string templateText, Dictionary<string, string> variables)
        {
            var result = templateText;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }
            return result;
        }
    }

    // Request models for template management
    public class CreateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TemplateText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public string? PlaceholderVariables { get; set; }
    }

    public class UpdateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TemplateText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public string? PlaceholderVariables { get; set; }
    }
} 