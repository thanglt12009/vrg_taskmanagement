using OfficeOpenXml;
using TMS.Domain.Common;
using TMS.WebApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;

namespace TMS.WebApp.Services
{
    public class TaskMonthlyReport
    {
        private string _reportPath;
        private string _reporttemplateFile;
        private DateTime _reportTime;
        private string strTempFile;
        ExcelPackage _reportExcelPck;
        public TaskMonthlyReport(DateTime month)
        {
            _reportTime = month;
            _reportPath = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["ReportDirectoryRoot"]);
            _reporttemplateFile = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["MonthlyReportTemplate"]);
            
        }
        public bool prepareReport()
        {
            // create temp folder and templ report
            string _tempPath = _reportPath;
            if (!Directory.Exists(_tempPath)) Directory.CreateDirectory(_tempPath);
            string strMonth = String.Format("{0:MM-yyyy}", _reportTime);
            strTempFile = Path.Combine(_tempPath, "Báo cáo công văn tháng " + strMonth + ".xlsx");
            if (File.Exists(strTempFile))
            {
                File.Delete(strTempFile);
            }
            if (File.Exists(_reporttemplateFile))
            {
                File.Copy(_reporttemplateFile, strTempFile);
                return true;
            }

            return false;
        }

        public string loadReportFile(List<TaskReportRecordModel> data, List<TaskReportRecordModel> dataPrev)
        {
            var fileinfo = new FileInfo(strTempFile);
            try
            {
                using (_reportExcelPck = new ExcelPackage(fileinfo))
                {
                    ExcelWorkbook excelWorkBook = _reportExcelPck.Workbook;
                    // generate sheet 1
                    ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.First();

                    String reporttitle = excelWorksheet.Cells[4, 1].Value.ToString();
                    reporttitle = reporttitle.Replace("[month]", _reportTime.Month.ToString()).Replace("[year]", _reportTime.Year.ToString());
                    excelWorksheet.Cells[4, 1].Value = reporttitle;
                    int index = 0;
                    int catid = -1;
                    bool newcat = false;
                    int groupcount = 0;
                    int startingrow = 10;
                    foreach (TaskReportRecordModel item in data)
                    {
                        newcat = false;
                        if (catid < 0 || catid != (int) item.TaskType)
                        {
                            catid = (int)item.TaskType;
                            newcat = true;
                        }
                        if (newcat)
                        {
                            excelWorksheet.InsertRow(startingrow + index + groupcount, 1, startingrow - 1);
                            excelWorksheet.Cells[startingrow + index + groupcount, 1].Value = Convert.ToChar(65 + groupcount).ToString();
                            excelWorksheet.Cells[startingrow + index + groupcount, 2].Value = CategoryService.GetInstance().GetCategoryName((int)item.TaskType, Contain.CatType.Category);
                            groupcount++;
                        }
                        excelWorksheet.InsertRow(startingrow + 1 + index + groupcount, 1, startingrow + index + groupcount);

                        excelWorksheet.Cells[startingrow + index + groupcount, 1].Value = index+1;
                        excelWorksheet.Cells[startingrow + index + groupcount, 2].Value = item.Title;
                        excelWorksheet.Cells[startingrow + index + groupcount, 3].Value = item.AssigneeName;
                        if (item.CreationDate.Month == _reportTime.Month && item.CreationDate.Year == _reportTime.Year)
                            excelWorksheet.Cells[startingrow + index + groupcount, 4].Value = "X";
                        if (item.LastUpdateDate.Month == _reportTime.Month && item.LastUpdateDate.Year == _reportTime.Year)
                            excelWorksheet.Cells[startingrow + index + groupcount, 5].Value = "X";

                        if (item.Status == 1 || item.Status == 2)
                            excelWorksheet.Cells[startingrow + index + groupcount, 6].Value = "X";
                        if (item.Status == 3 || item.Status == 4 || item.Status == 5)
                            excelWorksheet.Cells[startingrow + index + groupcount, 7].Value = "X";
                        if (item.Status == 6)
                            excelWorksheet.Cells[startingrow + index + groupcount, 8].Value = "X";

                        index++;
                    }

                    //_reportExcelPck.Save();

                    // generate sheet 2
                    excelWorksheet = excelWorkBook.Worksheets[2];
                    DateTime prevMonth = _reportTime.AddMonths(-1);
                    reporttitle = excelWorksheet.Cells[4, 1].Value.ToString();
                    reporttitle = reporttitle.Replace("[month]", prevMonth.Month.ToString()).Replace("[year]", prevMonth.Year.ToString());
                    excelWorksheet.Cells[4, 1].Value = reporttitle;
                    index = 0;
                    catid = -1;
                    newcat = false;
                    groupcount = 0;
                    foreach (TaskReportRecordModel item in dataPrev)
                    {
                        newcat = false;
                        if (catid < 0 || catid != (int)item.TaskType)
                        {
                            catid = (int)item.TaskType;
                            newcat = true;
                        }
                        if (newcat)
                        {
                            excelWorksheet.InsertRow(startingrow + index + groupcount, 1, startingrow - 1);
                            excelWorksheet.Cells[startingrow + index + groupcount, 1].Value = Convert.ToChar(65 + groupcount).ToString();
                            excelWorksheet.Cells[startingrow + index + groupcount, 2].Value = CategoryService.GetInstance().GetCategoryName((int)item.TaskType, Contain.CatType.Category);
                            groupcount++;
                        }
                        excelWorksheet.InsertRow(startingrow+1 + index + groupcount, 1, startingrow + index + groupcount);

                        excelWorksheet.Cells[startingrow + index + groupcount, 1].Value = index + 1;
                        excelWorksheet.Cells[startingrow + index + groupcount, 2].Value = item.Title;
                        excelWorksheet.Cells[startingrow + index + groupcount, 3].Value = item.AssigneeName;
                        excelWorksheet.Cells[startingrow + index + groupcount, 4].Value = item.CreationDate.ToString("dd-MM-yyyy");
                        excelWorksheet.Cells[startingrow + index + groupcount, 5].Value = item.LastUpdateDate.ToString("dd-MM-yyyy");

                        if (item.Status == 1 || item.Status == 2)
                            excelWorksheet.Cells[startingrow + index + groupcount, 6].Value = "X";
                        if (item.Status == 3 || item.Status == 4 || item.Status == 5)
                            excelWorksheet.Cells[startingrow + index + groupcount, 7].Value = "X";
                        if (item.Status == 6)
                            excelWorksheet.Cells[startingrow + index + groupcount, 8].Value = "X";

                        index++;
                    }

                    _reportExcelPck.Save();
                    _reportExcelPck.Dispose();
                    return strTempFile;
                }
            }
            catch
            {

            }
            return String.Empty;

        }
    }
}