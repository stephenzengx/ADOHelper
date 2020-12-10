using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class TB_UserAccount
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime Writetime { get; set; }
    }
}
