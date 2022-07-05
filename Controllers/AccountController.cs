using BGSWebApplication.Models.Database;
using BGSWebApplication.Models.DTO;
using BGSWebApplication.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BGSWebApplication.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public async Task<ActionResult> Login()
        {
            try
            {
                if (Session["Username"] != null && Session["Password"] != null)
                {
                    //quick use session to auth and log user in
                    var result = await AuthenticateUser2(Convert.ToString(Session["Password"]), Convert.ToString(Session["Username"]));

                    if (result.Equals(true)) 
                    {
                        return RedirectToAction("Index","Home");
                    }
                    else
                    {
                        return View();
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return View();
        }

        public async Task<ActionResult> AuthenticateUser(string pwrd, string usrn)
        {
            try
            {
                //main auth
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {
                    var userData = await (from usrs in bGSdb.TBL_Users where usrs.UserName.Equals(usrn) select usrs).ToListAsync();

                    var usrdata = userData.FirstOrDefault<TBL_Users>();

                    EncryptKey dencrypt = new EncryptKey();

                    string encryptKey = WebConfigurationManager.AppSettings["encryptKey"];

                    if (usrdata != null)
                    {
                        var pwdH = await dencrypt.DecryptString(encryptKey, usrdata.PasswordHash);

                        var userStatus = false;

                        if (pwrd.Equals(pwdH))
                        {
                            userStatus = true;
                        }

                        if (userStatus.Equals(true))
                        {
                            Session["Username"] = usrn;
                            Session["Password"] = pwrd;

                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
            }
            return View();
        }

        public async Task<bool> AuthenticateUser2(string pwrd, string usrn)
        {
            try
            {
                //quick auth on session avail
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {
                    var userData = await (from usrs in bGSdb.TBL_Users where usrs.UserName.Equals(usrn) select usrs).ToListAsync();

                    var usrdata = userData.FirstOrDefault<TBL_Users>();

                    EncryptKey dencrypt = new EncryptKey();

                    string encryptKey = WebConfigurationManager.AppSettings["encryptKey"];

                    var pwdH = await dencrypt.DecryptString(encryptKey, usrdata.PasswordHash);

                    var userStatus = false;

                    if (pwrd.Equals(pwdH))
                    {
                        userStatus = true;
                    }

                    if (userStatus.Equals(true))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }               

            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public async Task<ActionResult> RegisterUser(UsersRegDetailsDTO usersReg)
        {
            try
            {
                using (BGSdbEntities bGSdb = new BGSdbEntities())
                {
                    //check if username exists already no duplicates
                    var userStatus = await (from usrs in bGSdb.TBL_Users where usrs.UserName.Equals(usersReg.Username1) select usrs).FirstOrDefaultAsync();

                    if (userStatus != null)
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        int? maxAttPKu = bGSdb.TBL_Users.Max(x => (int?)x.Id);

                        long retvalu = Convert.ToInt16((maxAttPKu.HasValue ? maxAttPKu.Value + 1 : 1));

                        var tBLusrs = bGSdb.Set<TBL_Users>();

                        EncryptKey encrypt = new EncryptKey();

                        string encryptKey = WebConfigurationManager.AppSettings["encryptKey"];

                        var pwdH = await encrypt.EncryptString(encryptKey, usersReg.Password1);

                        var usrsID = Convert.ToString(Guid.NewGuid());
                        tBLusrs.Add(new TBL_Users { Id = Convert.ToInt32(retvalu), UserName = usersReg.Username1, UserId = usrsID, PasswordHash = pwdH });


                        //Create users record Json
                        int? maxAttPKud = bGSdb.TBL_UsersData.Max(x => (int?)x.Id);

                        long retvalud = Convert.ToInt16((maxAttPKud.HasValue ? maxAttPKud.Value + 1 : 1));

                        var tBLusrsdta = bGSdb.Set<TBL_UsersData>();
                        tBLusrsdta.Add(new TBL_UsersData { Id = Convert.ToInt32(retvalud), UserId = usrsID, dataJson = "" });

                        await bGSdb.SaveChangesAsync();

                        //log user in automatically
                        return RedirectToAction("AuthenticateUser", new { pwrd = usersReg.Password1, usrn = usersReg.Username1 });
                    }
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
            }
            return View();
        }
    }
}