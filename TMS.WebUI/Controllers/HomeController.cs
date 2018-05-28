using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using TMS.WebApp.Models;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Web.Security;
using System.Web.Configuration;
using System;
using TMS.Domain.Common;
using System.Net;

namespace TMS.WebApp.Controllers
{
    public class HomeController : BaseController
    {
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;

        public HomeController(IAccountRepository accRepository, ICategoryRepository catRepository) : base()
        {
            this.catRepository = catRepository;
            this.accRepository = accRepository;
        }
        // GET: Home
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("ViewBoard", "Kanban");
            }
            else
            {
                return this.RedirectToAction("Login", "Home");
            }
        }


        [AllowAnonymous]
        public ActionResult Login()
        {
            ViewBag.ReturnUrl = Request.QueryString["ReturnUrl"];

            if (User.Identity != null && User.Identity.IsAuthenticated && Session != null)
            {
                return RedirectToAction("Index", "Kanban");
            }
            else
            {
                if (Request.IsAjaxRequest())
                {
                    TempData["message"] = string.Format("Phiên làm việc đã hết thời gian, xin vui lòng đăng nhập lại.");
                    return Json(new { taskId = 0, success = false, message = TempData["message"], redirectUrl = "/Home/Login" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(AccountViewModel accountvm, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(accountvm);
            }
            else
            {
                Account account = accRepository.Find(accountvm.UserName);
                if (null != account)
                {
                    if (!account.IsLocalUser)
                    {
                        try
                        {
                            // SET YOUR AD CONFIG BY WEBCONFIG HERE //
                            string sDirectoryService = WebConfigurationManager.AppSettings["DirectoryService"];
                            string sDomain = WebConfigurationManager.AppSettings["sDomain"];
                            string sDefaultOU = WebConfigurationManager.AppSettings["sDefaultOU"];
                            string sServiceUser = WebConfigurationManager.AppSettings["sServiceUser"];
                            string sServicePassword = WebConfigurationManager.AppSettings["sServicePassword"];

                            // END SET CONFIG AD //
                            if ("AD".Equals(sDirectoryService))
                            {

                                // Config Principal Context
                                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, sDomain, sDefaultOU, sServiceUser, sServicePassword))
                                {
                                    // validate the credentials
                                    bool isAuthentic = pc.ValidateCredentials(accountvm.UserName, accountvm.Password, ContextOptions.Negotiate);

                                    if (isAuthentic)
                                    {
                                        FormsAuthentication.SetAuthCookie(account.UserName, accountvm.RememberMe);
                                        var principal = UserPrincipal.FindByIdentity(pc, account.UserName);

                                        Session["UserName"] = account.UserName;
                                        Session["FullName"] = account.DisplayName2;
                                        Session["UID"] = account.UID;
                                        Session["isAdmin"] = accRepository.GetRoleForUser(account.UserName).ToString();


                                        if (this.Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                                        {
                                            return JavaScript("window.location = '" + returnUrl + "'");
                                        }
                                        else
                                        {
                                            return JavaScript("window.location = '" + Url.Action("Index", "Worktask") + "'");
                                        }
                                    }
                                }
                            }
                            else if ("LDAP".Equals(sDirectoryService))
                            {
                                bool isAuthentic = false;
                                int sPort = int.Parse(WebConfigurationManager.AppSettings["sPort"]);
                                try
                                {
                                    LdapDirectoryIdentifier ldi = new LdapDirectoryIdentifier(sDomain, sPort);
                                    LdapConnection ldapConnection = new LdapConnection(ldi);
                                    ldapConnection.AuthType = AuthType.Basic;
                                    ldapConnection.SessionOptions.ProtocolVersion = 3;
                                    NetworkCredential nc = new NetworkCredential(string.Format(sDefaultOU, accountvm.UserName), accountvm.Password);
                                    ldapConnection.Bind(nc);
                                    isAuthentic = true;
                                    ldapConnection.Dispose();
                                }
                                catch
                                {
                                    isAuthentic = false;
                                }
                                // validate the credentials
                                if (isAuthentic)
                                {
                                    FormsAuthentication.SetAuthCookie(account.UserName, accountvm.RememberMe);

                                    Session["UserName"] = account.UserName;
                                    Session["FullName"] = account.DisplayName2;
                                    Session["UID"] = account.UID;
                                    Session["isAdmin"] = accRepository.GetRoleForUser(account.UserName).ToString();

                                    if (this.Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                                    {
                                        return JavaScript("window.location = '" + returnUrl + "'");
                                    }
                                    else
                                    {
                                        return JavaScript("window.location = '" + Url.Action("Index", "Kanban") + "'");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {

                            TempData["Notify"] = e.ToString();

                            return PartialView("_LoginPartialView", accountvm);

                        }

                    }
                    else

                    {

                        //string passMD5 = AccountHelper.MD5Hash(accountvm.Password);
                        //if (account.Password != null && account.Password.Equals(passMD5))
                        {
                            FormsAuthentication.SetAuthCookie(account.UserName, accountvm.RememberMe);
                            Session["UserName"] = account.UserName;
                            Session["FullName"] = account.DisplayName2;
                            Session["UID"] = account.UID;
                            Session["isAdmin"] = accRepository.GetRoleForUser(account.UserName).ToString();
                            if (this.Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                       && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                            {
                                return JavaScript("window.location = '" + returnUrl + "'");
                            }
                            else
                            {
                                return JavaScript("window.location = '" + Url.Action("Index", "Kanban") + "'");
                            }
                        }

                    }

                }

                TempData["Notify"] = "Tài khoản hoặc mật khẩu không đúng";
                
                return PartialView("_LoginPartialView", accountvm);
            }
        }

        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();

            return this.RedirectToAction("Login", "Home");
        }
        [AllowAnonymous]
        public ActionResult Error()
        {
            string returnUrl = Request.QueryString["ReturnUrl"];
            //If user has not permisson to access the action
            if (User.Identity != null && User.Identity.IsAuthenticated && Session != null)
            {
                return View();
            }
            else //If user has not permisson to access the action
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

        }
    }
}