using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BGSWebApplication.Models.DTO
{
    public class DetailedCategoryDTO
    {
        public int Work { get; set; }
        public int Personal { get; set; }
        public int Shopping { get; set; }
        public int Uncategorized { get; set; }


    }
}