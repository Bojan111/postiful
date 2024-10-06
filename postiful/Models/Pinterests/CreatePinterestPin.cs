using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using postiful.Enums;

namespace postiful.Models.PinterestModels
{
	public class CreatePinterestPin
	{
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile ImageFile { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public string DestinationLink { get; set; }
        public string PinterestLink { get; set; }
        public PinCreationEnum SelectedCreationPin { get; set; }


        [DisplayName("Choose your creation")]
        public IEnumerable<SelectListItem> ListCreationPins { get; set; }
    }
}