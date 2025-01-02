using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigireadProject.Models.ViewModels
{
    public class GalleryViewModel
    {
        public List<string> Genres { get; set; }
        public List<Books> Books { get; set; }
        public string SelectedGenre { get; set; }
        public IEnumerable<int> UserWishlist { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public GalleryViewModel()
        {
            Genres = new List<string>();
            Books = new List<Books>();
            UserWishlist = new List<int>();
        }
    }
}