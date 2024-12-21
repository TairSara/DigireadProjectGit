using System; // חשוב להוסיף את זה בראש הקובץ!

namespace DigireadProject.Models.ViewModels
{
    public class MyLibraryViewModel
    {
        public int BookId { get; set; }  
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageSrc { get; set; }
        public string Type { get; set; }  // "רכישה" או "השאלה"
        public DateTime? ReturnDate { get; set; }  // רק עבור השאלות
    }
}