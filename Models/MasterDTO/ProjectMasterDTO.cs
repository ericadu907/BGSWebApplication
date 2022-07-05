using BGSWebApplication.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BGSWebApplication.Models.MasterDTO
{
    public class ProjectMasterDTO
    {
        public List<TBL_CategoriesDTO> Categories { get; set; }
        
        #region Todays Tasks pulled by date today
        public List<JsonDTO> JsonDataToday { get; set; }
        #endregion

        #region All Tasks pulling with no condition
        public List<JsonDTO> JsonDataAll { get; set; }
        #endregion       

        public List<DetailedCategoryDTO> CategoryInfo { get; set; }
        public List<AllTasksDTO> TasksInfo { get; set; }



        public int Id { get; set; }
        public string Category { get; set; }
    }
}