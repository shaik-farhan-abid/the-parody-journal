using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace theParodyJournal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextSummarizationController : ControllerBase
    {
        private readonly TextSummarizerService _textSummarizerService;

        public TextSummarizationController(TextSummarizerService textSummarizerService)
        {
            _textSummarizerService = textSummarizerService;
        }

        [HttpPost("summarize")]
        public IActionResult SummarizeText([FromBody] SummarizeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Text cannot be empty.");
            }

            var summary = _textSummarizerService.SummarizeText(request.Text);
            return Ok(new { summary });
        }
    }

    // Define request model
    public class SummarizeRequest
    {
        public string Text { get; set; }
    }

    // Define the service for text summarization
    public class TextSummarizerService
    {
        public List<string> SummarizeText(string inputText, int numberOfKeywords = 5)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return new List<string>();
            }

            var words = inputText.Split(new[] { ' ', '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

            var wordFrequency = words.GroupBy(w => w.ToLower())
                                     .Select(g => new { Word = g.Key, Count = g.Count() })
                                     .OrderByDescending(w => w.Count)
                                     .Take(numberOfKeywords)
                                     .Select(w => w.Word)
                                     .ToList();

            return wordFrequency;
        }
    }
}


