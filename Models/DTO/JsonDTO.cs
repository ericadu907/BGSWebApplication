using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BGSWebApplication.Models.DTO
{
    public class JsonDTO
    {
        public string Id { get; set; }
        public string Category { get; set; }//key
        public string Title { get; set; }
        public string Date { get; set; }
        public string Task { get; set; }
        public string Details { get; set; }
        public string Color { get; set; }
        public string AddedDate { get; set; }



    }
    public class JsonDTONested
    {
        public List<JsonDTO> alldata { get; set; }

    }
}