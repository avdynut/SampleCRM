//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SampleCRM.Web.Models
{
    using System;using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    
    public partial class OrderItem
    {
        [Key]public long OrderID { get; set; }
        [Key]public long OrderLine { get; set; }
        public decimal Discount { get; set; }
        public string ProductID { get; set; }
        public long Quantity { get; set; }
        public long TaxType { get; set; }
        public decimal UnitPrice { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }
    }
}