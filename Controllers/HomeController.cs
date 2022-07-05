using BGSWebApplication.Models.Database;
using BGSWebApplication.Models.DTO;
using BGSWebApplication.Models.MasterDTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BGSWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            try
            {
                if (Session["Username"] != null && Session["Password"] != null)
                {
                    var sessionUname = Convert.ToString(Session["Username"]);

                    ViewBag.username = sessionUname;

                    //Todays Tasks
                    ProjectMasterDTO Model = new ProjectMasterDTO();

                    using (BGSdbEntities bGSdb = new BGSdbEntities())
                    {
                        var result = await (from a in bGSdb.TBL_Categories
                                            select new TBL_CategoriesDTO()
                                            {
                                                Id = a.Id,
                                                Category = a.Category

                                            }).ToListAsync();
                        Model.Categories = result;

                        var usridResult = await bGSdb.TBL_Users.FirstOrDefaultAsync(a => a.UserName.Equals(sessionUname));

                        var datas = await bGSdb.TBL_UsersData.FirstOrDefaultAsync(a => a.UserId.Equals(usridResult.UserId));

                        var values = new Dictionary<string, JsonDTO>();

                        Model.JsonDataToday = new List<JsonDTO>();
                        Model.JsonDataAll = new List<JsonDTO>();
                        Model.CategoryInfo = new List<DetailedCategoryDTO>();
                        Model.TasksInfo = new List<AllTasksDTO>();

                        if (!datas.dataJson.Equals(""))
                        {
                            values = JsonConvert.DeserializeObject<Dictionary<string, JsonDTO>>(datas.dataJson);

                            if (values != null)
                            {
                                var data = values.Values.ToList();

                                #region Todays Tasks pulled by date
                                foreach (var item in data)
                                {
                                    if (item.Date.Equals(DateTime.Now.ToString("MM/dd/yyyy")))
                                    {
                                        Model.JsonDataToday.Add(new JsonDTO { Id = item.Id, Category = item.Category, Date = item.Date, Details = item.Details, Task = item.Task, Title = item.Title, Color = item.Color, AddedDate = item.AddedDate });
                                    }
                                }
                                #endregion

                                #region All Tasks pulling with no condition
                                foreach (var item in data)
                                {
                                    Model.JsonDataAll.Add(new JsonDTO { Id = item.Id, Category = item.Category, Date = item.Date, Details = item.Details, Task = item.Task, Title = item.Title, Color = item.Color, AddedDate = item.AddedDate });
                                }
                                #endregion

                                var red = 0;
                                foreach (var v in values)
                                {
                                    if (Convert.ToString(v.Value.Color) == "red")
                                    {
                                        var key = v.Key;
                                        red++;

                                    }
                                }

                                var orange = 0;
                                foreach (var v in values)
                                {
                                    if (Convert.ToString(v.Value.Color) == "orangered")
                                    {
                                        var key = v.Key;
                                        orange++;
                                    }
                                }

                                var blue = 0;
                                foreach (var v in values)
                                {
                                    if (Convert.ToString(v.Value.Color) == "steelblue")
                                    {
                                        var key = v.Key;
                                        blue++;

                                    }
                                }

                                Model.TasksInfo.Add(new AllTasksDTO() { TotalTasks = values.Keys.Count() });

                                Model.CategoryInfo.Add(new DetailedCategoryDTO() { Work = red, Shopping = orange, Personal = blue });
                            }
                        }
                        
                    }
                    return PartialView(Model);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

            return View();
        }

        public async Task<ActionResult> AddTask(JsonDTO taskDetails)
        {
            try
            {
                //***Important
                var bokColour = "";
                if (taskDetails.Category.Equals("1")) { bokColour = "red"; }
                if (taskDetails.Category.Equals("2")) { bokColour = "steelblue"; }
                if (taskDetails.Category.Equals("3")) { bokColour = "orangered"; }

                taskDetails.Color = bokColour;

                var usrsDataJson = new Dictionary<string, JsonDTO>()
            {
               { Convert.ToString(Guid.NewGuid()), new JsonDTO { Id=Convert.ToString(Guid.NewGuid()), Category =taskDetails.Category, Date=taskDetails.Date, Details= taskDetails.Details.Equals(null)?"":taskDetails.Details, Task=taskDetails.Task, Title = taskDetails.Title, Color = taskDetails.Color, AddedDate = DateTime.Now.ToString("yyyy MMMM") } }
            };

                string usersNewTask = JsonConvert.SerializeObject(usrsDataJson);

                var sessionUname = Convert.ToString(Session["Username"]);
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {

                    var usridResult = await bGSdb.TBL_Users.FirstOrDefaultAsync(a => a.UserName.Equals(sessionUname));

                    var datas = await bGSdb.TBL_UsersData.FirstOrDefaultAsync(a => a.UserId.Equals(usridResult.UserId));

                    if (datas.dataJson.Equals(null) || datas.dataJson.Equals("") || datas.dataJson.Length < 5)
                    {
                        datas.dataJson = usersNewTask;
                    }
                    else
                    {
                        var values = JsonConvert.DeserializeObject<Dictionary<string, JsonDTO>>(datas.dataJson);
                        values.Add(Convert.ToString(Guid.NewGuid()), new JsonDTO { Id = Convert.ToString(Guid.NewGuid()), Category = taskDetails.Category, Date = taskDetails.Date, Details = taskDetails.Details.Equals(null) ? "" : taskDetails.Details, Task = taskDetails.Task, Title = taskDetails.Title, Color = taskDetails.Color, AddedDate = DateTime.Now.ToString("yyyy MMMM") });

                        string alteredJson = JsonConvert.SerializeObject(values);

                        datas.dataJson = alteredJson;

                    }

                    await bGSdb.SaveChangesAsync();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

            return View();
        }

        public async Task<ActionResult> DeleteTask(string guidId)
        {
            try
            {
                var sessionUname = Convert.ToString(Session["Username"]);
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {

                    var usridResult = await bGSdb.TBL_Users.FirstOrDefaultAsync(a => a.UserName.Equals(sessionUname));

                    var datas = await bGSdb.TBL_UsersData.FirstOrDefaultAsync(a => a.UserId.Equals(usridResult.UserId));

                    var values = JsonConvert.DeserializeObject<Dictionary<string, JsonDTO>>(datas.dataJson);
                    bool done = false;

                    foreach (var v in values)
                    {
                        if (Convert.ToString(v.Value.Id) == guidId)
                        {
                            var key = v.Key;
                            values.Remove(key);
                            done = true;
                        }

                        if (done.Equals(true))
                        {

                            break;
                        }
                    }
                    done = false;

                    string alteredJson = JsonConvert.SerializeObject(values);

                    datas.dataJson = alteredJson;

                    await bGSdb.SaveChangesAsync();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

            return View();
        }

        public async Task<ActionResult> EditTask(string guidId)
        {
            try
            {
                var sessionUname = Convert.ToString(Session["Username"]);
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {

                    var usridResult = await bGSdb.TBL_Users.FirstOrDefaultAsync(a => a.UserName.Equals(sessionUname));

                    var datas = await bGSdb.TBL_UsersData.FirstOrDefaultAsync(a => a.UserId.Equals(usridResult.UserId));

                    var values = JsonConvert.DeserializeObject<Dictionary<string, JsonDTO>>(datas.dataJson);
                    bool done = false;
                    var jsonDTOResult = new JsonDTO();
                    foreach (var v in values)
                    {
                        if (Convert.ToString(v.Value.Id) == guidId)
                        {
                            var key = v.Key;

                            var taskEdit = values.TryGetValue(key, out JsonDTO jsonDTO);
                            jsonDTOResult = jsonDTO;
                            done = true;
                        }

                        if (done.Equals(true))
                        {

                            break;
                        }
                    }
                    done = false;

                    return Json(jsonDTOResult);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

            return View();
        }

        public async Task<ActionResult> EditTaskFromForm(JsonDTO taskDetails)
        {
            try
            {
                //***Important
                var bokColour = "";
                if (taskDetails.Category.Equals("1")) { bokColour = "red"; }
                if (taskDetails.Category.Equals("2")) { bokColour = "steelblue"; }
                if (taskDetails.Category.Equals("3")) { bokColour = "orangered"; }

                taskDetails.Color = bokColour;

                var sessionUname = Convert.ToString(Session["Username"]);
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {
                    var usridResult = await bGSdb.TBL_Users.FirstOrDefaultAsync(a => a.UserName.Equals(sessionUname));

                    var datas = await bGSdb.TBL_UsersData.FirstOrDefaultAsync(a => a.UserId.Equals(usridResult.UserId));


                    var values = JsonConvert.DeserializeObject<Dictionary<string, JsonDTO>>(datas.dataJson);

                    bool done = false;
                    var jsonDTOResult = new JsonDTO();
                    foreach (var v in values)
                    {
                        if (Convert.ToString(v.Value.Id) == taskDetails.Id)
                        {
                            var key = v.Key;

                            values[key] = new JsonDTO() { Id = taskDetails.Id, Category = taskDetails.Category, Date = taskDetails.Date, Details = taskDetails.Details, Task = taskDetails.Task, Title = taskDetails.Title, Color = taskDetails.Color }; //myNewValue;
                            done = true;


                            string alteredJson = JsonConvert.SerializeObject(values);

                            datas.dataJson = alteredJson;

                        }

                        if (done.Equals(true))
                        {
                            break;
                        }
                    }
                    done = false;

                    await bGSdb.SaveChangesAsync();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);

            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Logout()
        {
            try
            {
                if (Session["Username"] != null && Session["Password"] != null)
                {
                    Session.Clear();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("Login", "Account");

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);

            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}