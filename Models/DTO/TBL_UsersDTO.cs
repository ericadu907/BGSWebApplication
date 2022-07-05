using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BGSWebApplication.Models.DTO
{
    public class TBL_UsersDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string UserId { get; set; }
    }
    
    public class UsersRegDetailsDTO
    {       
        public string Username1 { get; set; }
        public string Password1 { get; set; }
    }
}