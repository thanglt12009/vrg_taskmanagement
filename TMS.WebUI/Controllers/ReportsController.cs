using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Configuration;
using TMS.WebApp.Models;
using TMS.Domain.Abstract;
using TMS.WebApp.Services;
using System.Net.Mail;
using TMS.Domain.Entities;
using TMS.Domain.Common;

namespace TMS.WebApp.Controllers
{
    [Authorize]
    public class ReportsController : BaseController
    {
        private IWorktaskRepository repository;
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;
        private IDepartmentRepository depRepository;

        // Constructor
        public ReportsController(IWorktaskRepository worktaskRepository, IAccountRepository accountRepository, ICategoryRepository catRepository, IDepartmentRepository depRepository) : base()
        {
            this.repository = worktaskRepository;
            this.catRepository = catRepository;
            this.accRepository = accountRepository;
            this.depRepository = depRepository;
        }
        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }

        // GET api/<controller> 
        [HttpGet]
        [AllowAnonymous]
        public JsonResult SendMonthlyReport(string month)
        {
            try
            {
                if (null == month || month.Trim().Length == 0)
                {
                    DateTime m = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                    month = m.ToString("MM-yyyy");
                }
                string[] val = month.Split('-');

                DateTime start = new DateTime(Convert.ToInt32(val[1]), Convert.ToInt32(val[0]), 1);
                DateTime end = start.AddMonths(1).AddDays(-1);
                List<TaskReportRecordModel> data = this.LoadMonthlyData(start, end);
                List<TaskReportRecordModel> dataPrev = this.LoadTillThisTimeData(start);
                TaskMonthlyReport rp = new TaskMonthlyReport(start);
                rp.prepareReport();
                string outputfile = rp.loadReportFile(data, dataPrev);
                if (outputfile.Length > 0)
                {
                    string filename = Path.GetFileName(outputfile);

                    MailMessage mail = new MailMessage();
                    SmtpClient smtpClient = new SmtpClient(WebConfigurationManager.AppSettings["EmailReportServer"]);
                    mail.From = new MailAddress(WebConfigurationManager.AppSettings["EmailReportFrom"], "No Reply");
                    mail = this.addUserEmails(mail);
                    mail.Subject = Path.GetFileNameWithoutExtension(outputfile);
                    mail.Body = Path.GetFileNameWithoutExtension(outputfile);

                    System.Net.Mail.Attachment attachment;
                    attachment = new System.Net.Mail.Attachment(outputfile);
                    mail.Attachments.Add(attachment);

                    smtpClient.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["EmailReportPort"]);
                    smtpClient.Credentials = new System.Net.NetworkCredential(WebConfigurationManager.AppSettings["EmailReportFrom"], WebConfigurationManager.AppSettings["EmailReportPass"]);
                    smtpClient.EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["EmailReportSSL"]);

                    smtpClient.Send(mail);
                    smtpClient.Dispose();
                }
            }
            catch
            {

            }
            return Json(new { message = "Đã tạo xong báo cáo tháng" }, JsonRequestBehavior.AllowGet);
        }

        // GET api/<controller> 
        [HttpGet]
        [AllowAnonymous]
        public JsonResult SendDailyReport(string date)
        {
            try
            {
                if (null == date || date.Trim().Length == 0)
                {
                    date = DateTime.Now.ToString("dd-MM-yyyy");
                }
                string[] val = date.Split('-');

                DateTime start = new DateTime(Convert.ToInt32(val[2]), Convert.ToInt32(val[1]), Convert.ToInt32(val[0]));

                Account[] lstAccount = accRepository.Accounts.ToArray();

                foreach (Account item in lstAccount)
                {
                    if ((!String.IsNullOrEmpty(item.Email)) && !UserPermissionService.GetInstance().isAdmin(item.Role))
                    {
                        List<TaskReportRecordModel> data = this.LoadDailyData(start, item);
                        List<TaskReportRecordModel> dataPrev = this.LoadTillThisTimeData(start);
                        TaskDailyReport rp = new TaskDailyReport(start, item);
                        rp.prepareReport();
                        string outputfile = rp.loadReportFile(data, dataPrev);
                        if (outputfile.Length > 0)
                        {
                            string filename = Path.GetFileName(outputfile);

                            MailMessage mail = new MailMessage();
                            SmtpClient smtpCLient = new SmtpClient(WebConfigurationManager.AppSettings["EmailReportServer"]);
                            mail.From = new MailAddress(WebConfigurationManager.AppSettings["EmailReportFrom"], "No Reply");
                            mail.To.Add(item.Email);
                            mail.Subject = "Báo cáo công văn ngày " + String.Format("{0:dd-MM-yyyy}", start);
                            mail.Body = Path.GetFileNameWithoutExtension(outputfile);

                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(outputfile);
                            mail.Attachments.Add(attachment);

                            smtpCLient.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["EmailReportPort"]);
                            smtpCLient.Credentials = new System.Net.NetworkCredential(WebConfigurationManager.AppSettings["EmailReportFrom"], WebConfigurationManager.AppSettings["EmailReportPass"]);
                            smtpCLient.EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["EmailReportSSL"]);

                            smtpCLient.Send(mail);
                            smtpCLient.Dispose();

                        }
                    }
                }
            }
            catch
            {

            }
            return Json(new { message = "Đã tạo xong báo cáo ngày" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [AllowAnonymous]
        public JsonResult SendDailyReportForManager(string date)
        {
            try
            {
                if (null == date || date.Trim().Length == 0)
                {
                    date = DateTime.Now.ToString("dd-MM-yyyy");
                }
                string[] val = date.Split('-');

                DateTime start = new DateTime(Convert.ToInt32(val[2]), Convert.ToInt32(val[1]), Convert.ToInt32(val[0]));
                List<Department> deps = depRepository.Departments.ToList();

                foreach (Department dep in deps)
                {
                    List<Account> lstAccount = dep.Accounts.ToList();
                    List<Account> managers = lstAccount.Where(a => a.Role == 2).ToList();
                    List<TaskReportRecordModel> data = this.LoadDailyDepartmentData(start, dep);
                    List<TaskReportRecordModel> dataPrev = this.LoadTillThisTimeDepartmentData(start, dep);
                    TaskDepartmentDailyReport rp = new TaskDepartmentDailyReport(start, dep);
                    rp.prepareReport();
                    string outputfile = rp.loadReportFile(data, dataPrev);
                    if (outputfile.Length > 0)
                    {
                        string filename = Path.GetFileName(outputfile);

                        MailMessage mail = new MailMessage();
                        SmtpClient smtpCLient = new SmtpClient(WebConfigurationManager.AppSettings["EmailReportServer"]);
                        mail.From = new MailAddress(WebConfigurationManager.AppSettings["EmailReportFrom"], "No Reply");
                        foreach (var item in managers)
                        {
                            mail.To.Add(item.Email);
                        }
                        
                        mail.Subject = "[VRG KHDT] Báo Cáo Tình Hình Công việc Ngày " + String.Format("{0:dd-MM-yyyy}", start);
                        mail.Body = Path.GetFileNameWithoutExtension(outputfile);

                        System.Net.Mail.Attachment attachment;
                        attachment = new System.Net.Mail.Attachment(outputfile);
                        mail.Attachments.Add(attachment);

                        smtpCLient.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["EmailReportPort"]);
                        smtpCLient.Credentials = new System.Net.NetworkCredential(WebConfigurationManager.AppSettings["EmailReportFrom"], WebConfigurationManager.AppSettings["EmailReportPass"]);
                        smtpCLient.EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["EmailReportSSL"]);

                        smtpCLient.Send(mail);
                        smtpCLient.Dispose();

                    }
                }
            }
            catch
            {

            }
            return Json(new { message = "Đã tạo xong báo cáo ngày cho ban" }, JsonRequestBehavior.AllowGet);
        }

        private MailMessage addUserEmails(MailMessage mail)
        {
            Account[] lstAccount = accRepository.Accounts.ToArray();
            foreach(Account item in lstAccount)
            {
                if (!String.IsNullOrEmpty(item.Email))
                {
                    mail.To.Add(item.Email);
                }
            }
            return mail;
        }

        // GET api/<controller> 
        [HttpGet]
        public FileResult GetXLSReport(string month)
        {
            try
            {
                string[] val = month.Split('-');

                DateTime start = new DateTime(Convert.ToInt32(val[1]), Convert.ToInt32(val[0]), 1);
                DateTime end = start.AddMonths(1).AddDays(-1);
                List<TaskReportRecordModel> data = this.LoadMonthlyData(start, end);
                List<TaskReportRecordModel> dataPrev = this.LoadTillThisTimeData(start);
                TaskMonthlyReport rp = new TaskMonthlyReport(start);
                rp.prepareReport();
                string outputfile = rp.loadReportFile(data, dataPrev);
                if (outputfile.Length > 0)
                {
                    string filename = Path.GetFileName(outputfile);
                    return File(outputfile, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
                }
            }
            catch
            {

            }

            MemoryStream mStreamMain = new MemoryStream();

            return File(mStreamMain.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet, "EmptyReport.xlsx");
        }

        private List<TaskReportRecordModel> LoadMonthlyData(DateTime start, DateTime end)
        {
            // Initialization.   
            List<TaskReportRecordModel> lst = new List<TaskReportRecordModel>();
            try
            {
                List<TaskReportRecordModel> dataList = (from wt in repository.Worktasks
                                where (wt.CreationDate >= start && wt.CreationDate < end || wt.LastUpdateDateTime >= start && wt.LastUpdateDateTime < end)
                                orderby wt.TaskType
                                select new TaskReportRecordModel()
                                {
                                    TaskType = wt.TaskType,
                                    Title = wt.Title,
                                    AssigneeName = wt.AssigneeAcc.DisplayName2,
                                    Status = wt.Status,
                                    CreationDate = wt.CreationDate,
                                    LastUpdateDate = (wt.LastUpdateDateTime ?? wt.CreationDate)
                                }).ToList< TaskReportRecordModel>();
                return dataList;
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }

        private List<TaskReportRecordModel> LoadTillThisTimeData(DateTime end)
        {
            // Initialization.   
            List<TaskReportRecordModel> lst = new List<TaskReportRecordModel>();
            try
            {
                List<TaskReportRecordModel> dataList = (from wt in repository.Worktasks
                                                           where ((wt.CreationDate < end || wt.LastUpdateDateTime < end) && wt.WorkflowInstance.CurrentState.Type != 2)
                                                           orderby wt.TaskType
                                                           select new TaskReportRecordModel()
                                                           {
                                                               TaskType = wt.TaskType,
                                                               Title = wt.Title,
                                                               AssigneeName = wt.AssigneeAcc.DisplayName2,
                                                               Status = wt.Status,
                                                               CreationDate = wt.CreationDate,
                                                               LastUpdateDate = (wt.LastUpdateDateTime??wt.CreationDate)
                                                           }).ToList<TaskReportRecordModel>();
                return dataList;
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }
        private List<TaskReportRecordModel> LoadDailyData(DateTime start, Account user)
        {
            // Initialization.   
            List<TaskReportRecordModel> lst = new List<TaskReportRecordModel>();
            try
            {
                DateTime end = start.AddDays(1);
                List<TaskReportRecordModel> dataList = (from wt in repository.Worktasks
                                                           where (
                                                           (wt.CreationDate >= start && wt.CreationDate < end || wt.LastUpdateDateTime >= start && wt.LastUpdateDateTime < end)
                                                           && (UserPermissionService.GetInstance().isManager(user.Role) || (UserPermissionService.GetInstance().isUser(user.Role) && user.UID == wt.Assignee))
                                                           )
                                                           orderby wt.TaskType
                                                           select new TaskReportRecordModel()
                                                           {
                                                               TaskType = wt.TaskType,
                                                               Title = wt.Title,
                                                               AssigneeName = wt.AssigneeAcc.DisplayName2,
                                                               Status = wt.Status,
                                                               CreationDate = wt.CreationDate,
                                                               LastUpdateDate = (wt.LastUpdateDateTime ?? wt.CreationDate)
                                                           }).ToList<TaskReportRecordModel>();
                return dataList;
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }
        private List<TaskReportRecordModel> LoadDailyDepartmentData(DateTime start, Department dep)
        {
            // Initialization.   
            List<TaskReportRecordModel> lst = new List<TaskReportRecordModel>();
            try
            {
                DateTime end = start.AddDays(1);
                List<TaskReportRecordModel> dataList = (from wt in repository.Worktasks
                                                        where (
                                                        (wt.CreationDate >= start && wt.CreationDate < end || wt.LastUpdateDateTime >= start && wt.LastUpdateDateTime < end)
                                                        && wt.AssigneeAcc.DeptID == dep.DeptID
                                                        )
                                                        orderby wt.TaskType
                                                        select new TaskReportRecordModel()
                                                        {
                                                            TaskType = wt.TaskType,
                                                            Title = wt.Title,
                                                            AssigneeName = wt.AssigneeAcc.DisplayName2,
                                                            Status = wt.Status,
                                                            CreationDate = wt.CreationDate,
                                                            LastUpdateDate = (wt.LastUpdateDateTime ?? wt.CreationDate)
                                                        }).ToList<TaskReportRecordModel>();
                return dataList;
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }
        private List<TaskReportRecordModel> LoadTillThisTimeDepartmentData(DateTime end, Department dep)
        {
            // Initialization.   
            List<TaskReportRecordModel> lst = new List<TaskReportRecordModel>();
            try
            {
                List<TaskReportRecordModel> dataList = (from wt in repository.Worktasks
                                                        where ((wt.CreationDate < end || wt.LastUpdateDateTime < end) && wt.WorkflowInstance.CurrentState.Type != 2 && wt.AssigneeAcc.DeptID == dep.DeptID)
                                                        orderby wt.TaskType
                                                        select new TaskReportRecordModel()
                                                        {
                                                            TaskType = wt.TaskType,
                                                            Title = wt.Title,
                                                            AssigneeName = wt.AssigneeAcc.DisplayName2,
                                                            Status = wt.Status,
                                                            CreationDate = wt.CreationDate,
                                                            LastUpdateDate = (wt.LastUpdateDateTime ?? wt.CreationDate)
                                                        }).ToList<TaskReportRecordModel>();
                return dataList;
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }


    }
}