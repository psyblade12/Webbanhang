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
    
    public partial class Report
    {
        public int ReportID { get; set; }
        public string UserID { get; set; }
        public int ProductID { get; set; }
        public string Reason { get; set; }
        public Nullable<bool> IsRead { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Product Product { get; set; }
    }
}
