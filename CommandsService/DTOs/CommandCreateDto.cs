using System.ComponentModel.DataAnnotations;

namespace CommandsService.DTOs
{
    public class CommandCreateDto
    {
         [Required]
        public string HowTo { get; set; }

        [Required]
        public string CommandLine { get; set; }
    }
}