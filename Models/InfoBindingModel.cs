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
}