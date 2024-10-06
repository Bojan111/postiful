using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace postiful.Core.Entities.PinterestEntitiy
{
	public class Pinterest
	{
        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public byte[] ImageFile { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DestinationLink { get; set; }

    }
}