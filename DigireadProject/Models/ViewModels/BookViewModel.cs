using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace DigireadProject.Models.ViewModels
{
    public class BookViewModel
    {
        public int BookID { get; set; }

        [Required(ErrorMessage = "שדה כותרת הינו חובה")]
        [Display(Name = "כותרת")]
        public string Title { get; set; }

        [Required(ErrorMessage = "שדה מחבר הינו חובה")]
        [Display(Name = "מחבר")]
        public string MainAuthor { get; set; }

        [Required(ErrorMessage = "שדה הוצאה לאור הינו חובה")]
        [Display(Name = "הוצאה לאור")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "שדה שנת הוצאה הינו חובה")]
        [Display(Name = "שנת הוצאה")]
        [Range(1800, 2024, ErrorMessage = "שנת ההוצאה חייבת להיות בין 1800 ל-2024")]
        public int PublishYear { get; set; }

        [Display(Name = "מחיר השכרה")]
        [DataType(DataType.Currency)]
        [Range(0, 1000, ErrorMessage = "המחיר חייב להיות בין 0 ל-1000")]
        public decimal? RentalPrice { get; set; }

        [Required(ErrorMessage = "שדה מחיר רכישה הינו חובה")]
        [Display(Name = "מחיר רכישה")]
        [DataType(DataType.Currency)]
        [Range(0, 1000, ErrorMessage = "המחיר חייב להיות בין 0 ל-1000")]
        public decimal? PurchasePrice { get; set; }

        [Display(Name = "הגבלת גיל")]
        [Range(0, 18, ErrorMessage = "הגבלת הגיל חייבת להיות בין 0 ל-18")]
        public int? AgeRestriction { get; set; }

        [Required(ErrorMessage = "שדה ז'אנר הינו חובה")]
        [Display(Name = "ז'אנר")]
        public string Genre { get; set; }

        [Display(Name = "זמין")]
        public bool? IsAvailable { get; set; }

        [Display(Name = "ניתן להשכרה")]
        public bool? IsForRent { get; set; }

        [Display(Name = "מחיר מקורי")]
        [DataType(DataType.Currency)]
        public decimal? OriginalPrice { get; set; }

        [Display(Name = "תאריך סיום הנחה")]
        [DataType(DataType.Date)]
        public DateTime? DiscountEndDate { get; set; }

        [Display(Name = "מושכר כרגע")]
        public bool? IsRented { get; set; }

        [Display(Name = "זמין בפורמט EPUB")]
        public bool? IsEPUBAvailable { get; set; }

        [Display(Name = "זמין בפורמט F2B")]
        public bool? IsF2BAvailable { get; set; }

        [Display(Name = "זמין בפורמט Mobi")]
        public bool? IsMobiAvailable { get; set; }

        [Display(Name = "זמין בפורמט PDF")]
        public bool? IsPDFAvailable { get; set; }

        [Display(Name = "תמונת ספר")]
        public byte[] BookImage { get; set; }

        [Display(Name = "כמות במלאי")]
        [Range(0, 1000, ErrorMessage = "הכמות חייבת להיות בין 0 ל-1000")]
        public Nullable<int> StockQuantity { get; set; }
        private string _imageSrc;

        [Display(Name = "כתובת תמונה")]
        public string ImageSrc
        {
            get
            {
                if (!string.IsNullOrEmpty(_imageSrc))
                {
                    return _imageSrc;
                }
                if (BookImage != null)
                {
                    var base64 = Convert.ToBase64String(BookImage);
                    return $"data:image/jpeg;base64,{base64}";
                }
                return null;
            }
            set
            {
                _imageSrc = value;
            }
        }
        [Display(Name = "תיאור")]
        public string Description { get; set; }

    }
}