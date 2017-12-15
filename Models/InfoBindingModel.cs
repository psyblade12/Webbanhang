using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Webbanhang.Models
{
    public class InfoBindingModel
    {
        [Required]
        [Display(Name = "Tên")]
        public string name { get; set; }

        [Required]
        [Display(Name = "Địa chỉ")]
        public string homeAddress { get; set; }

        [Required]
        [Display(Name = "Số điện thoại")]
        public string phoneNumber { get; set; }
    }

    public class UserinfoModel
    {
        [StringLength(100, ErrorMessage = "Trường {0} phải dài ít nhất {2} kí tự.", MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Trường {0} phải dài ít nhất {2} kí tự.", MinimumLength = 2)]
        public string HomeAddress { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        
        public string Cart { get; set; }

        [StringLength(9, ErrorMessage = "Chứng minh nhân dân không đúng", MinimumLength = 9)]
        public string CMND { get; set; }
    }

    public class ProductModel
    {
        public string UserID { get; set; }
        public int ProductTypeID { get; set; }
        public int BrandID { get; set; }

        [StringLength(100, ErrorMessage = "Trường {0} phải dài ít nhất {2} kí tự.", MinimumLength = 2)]
        public string ProductName { get; set; }
        
        public string Detail { get; set; }

        [Range(0, Int32.MaxValue)]
        public int Stock { get; set; }
        
        public string ProductImage { get; set; }

        [Range(0, Int32.MaxValue)]
        public Nullable<int> Price { get; set; }

        public Nullable<System.DateTime> CreationDate { get; set; }

        [Range(0, Int32.MaxValue)]
        public Nullable<int> OldPrice { get; set; }
    }

    public class BanAccountModel
    {
        public string UserID { get; set; }
        [StringLength(100, ErrorMessage = "Trường {0} phải dài ít nhất {2} kí tự.", MinimumLength = 2)]
        public string Reason { get; set; }
        public string LiftDate { get; set; }
    }
}