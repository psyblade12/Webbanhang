//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Webbanhang
{
    using System;
    using System.Collections.Generic;
    
    public partial class SponsoredNewsOrder
    {
        public int SponsoredNewsOrderID { get; set; }
        public string UserID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> SumPrice { get; set; }
        public Nullable<System.DateTime> SponsoredNewsOrderDate { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
