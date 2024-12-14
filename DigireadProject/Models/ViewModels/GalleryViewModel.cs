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
    }
}