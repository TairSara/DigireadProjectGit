//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigireadProject
{
    using System;
    using System.Collections.Generic;
    
    public partial class ShoppingCart
    {
        public int CartID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> BookID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }
        public Nullable<bool> typeBook { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<bool> IsRental { get; set; }
    
        public virtual Users Users { get; set; }
        public virtual Books Books { get; set; }
    }
}