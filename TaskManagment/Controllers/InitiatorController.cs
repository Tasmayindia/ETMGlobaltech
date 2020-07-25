
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagment.Models;
using TaskManagment.Controllers;
using System.Web.Services;
using System.Configuration;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace TaskManagment.Controllers
{
    public class InitiatorController : Controller
    {
        ProjectTrackConnection ptraconn = new ProjectTrackConnection();
        SqlConnection con = new SqlConnection();

        [HttpGet]
        public ActionResult Welcome(string para1)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                Decript decript = new Decript();
                GalaxyID = decript.Decrypt(HttpUtility.UrlDecode(para1.ToString()));
                Session["GalaxyID"] = GalaxyID;
            }

            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            Request r = new Request();
            r.EmpCode = Session["Empcode"].ToString();
            r.GalaxyID = GalaxyID;
            TempData["FromGrid"] = "False";
            Session["GalaxyID"] = GalaxyID;
            r.GalaxyIDName = GetDisplayName(GalaxyID);
            return View("Welcome", r);
        }

        [HttpGet]
        public ActionResult Initiator(string Back)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            Session["Role"] = GetRole(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            SqlConnection con = ptraconn.GetItem();
            Request obj = new Request();
            obj.DoerName = GetDoer(obj.EmpCode);
            obj.ReviewBy = GetDisplayName(GalaxyID);
            obj.ReviewByGalaxy = GalaxyID;
            obj.EmpCode = Session["Empcode"].ToString();
            obj.GalaxyID = GalaxyID;
            obj.GalaxyIDName = GetDisplayName(GalaxyID);
            obj.ProjectList = ViewProjectList();
            obj.AssigneeList = GetAssigneeList();
            if (Back == "Back")
            {
                return RedirectToAction("ViewProjects", "Initiator");
            }
            else
            {
                return View("Initiator", obj);
            }
            
        }
        [HttpPost]
        public List<SelectListItem> GetAssigneeList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand("GetAssigneeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = Session["Empcode"].ToString();
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        SelectListItem s1 = new SelectListItem
                        {
                            Text = @dr["DisplayName"].ToString() +" ("+ @dr["GalaxyId"].ToString() + ")",
                            Value = @dr["GalaxyId"].ToString()
                        };
                        list.Add(s1);
                    }
                }
                else
                {
                    SelectListItem s1 = new SelectListItem
                    {
                        Text = "",
                        Value = "0"
                    };
                    list.Add(s1);
                }
                return list;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }
        [HttpPost]
        public ActionResult Initiator(Request R, string Submit, string Back)
        {
            var model = new List<Request>();
            
            if (Submit == "Create Task")
            {
                if (R.DoerNameGalaxy.Contains(","))
                {
                    string[] names = R.DoerNameGalaxy.Split(',');
                    for (int i = 0; i < names.Length; i++)
                    {
                        R.DoerNameGalaxy = names[i].ToString();
                        string prjno = ProjectSubmit(R);
                        con.Open();
                        SqlCommand cmd1 = new SqlCommand("UpdateProjectStatus", con);
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@projectno", prjno);
                        cmd1.Parameters.Add("@LastActivityRole", "Initiator");
                        cmd1.Parameters.Add("@NextRole", "Doer");
                        cmd1.Parameters.Add("@Status", "O");
                        cmd1.Parameters.Add("@Remark", R.InitiatorRemark);
                        cmd1.Parameters.Add("@LastActivityBy", Session["GalaxyID"].ToString());
                        cmd1.Parameters.Add("@LastActivityOn", DateTime.Now);
                        cmd1.ExecuteNonQuery();
                        con.Close();
                        SendEmail(prjno, R, "New"); 
                    }
                }
                else
                {
                    string prjno = ProjectSubmit(R);
                    model.Add(R);
                    //============updating project status===================
                    con.Open();
                    SqlCommand cmd1 = new SqlCommand("UpdateProjectStatus", con);
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.Add("@projectno", prjno);
                    cmd1.Parameters.Add("@LastActivityRole", "Initiator");
                    cmd1.Parameters.Add("@NextRole", "Doer");
                    cmd1.Parameters.Add("@Status", "O");
                    cmd1.Parameters.Add("@Remark", R.InitiatorRemark);
                    cmd1.Parameters.Add("@LastActivityBy", Session["GalaxyID"].ToString());
                    cmd1.Parameters.Add("@LastActivityOn", DateTime.Now);
                    cmd1.ExecuteNonQuery();
                    con.Close();
                    SendEmail(prjno, R, "New");
                }
            }
            else if (Back == "Back")
            {
                return RedirectToAction("ViewProjects", "Initiator");
            }
            return RedirectToAction("ViewProjects", "Initiator");
        }


        [HttpGet]
        public ActionResult ViewProjects(string para1)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            Session["Role"] = GetRole(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            Request obj = new Request();
            obj.ProjectList = ViewProjectList(); 

            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);

            obj.EmpCode = Session["Empcode"].ToString();
            obj.GalaxyID = GalaxyID;
            obj.GalaxyIDName = GetDisplayName(GalaxyID);
            
            TempData["FromGrid"] = "False";
            Session["GalaxyID"] = GalaxyID;


            return View("../Initiator/ViewProjects", obj);
        }

        [HttpPost]
        public ActionResult ViewProjects(string submit, string searchBy, Request R)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);

            if (submit == "Search")
            {
                List<ProjectList> objentry = new List<ProjectList>();
                var AllList1 = new List<ProjectList>();
                AllList1 = GetRequestDetails("submit", R);
                R.ProjectList = AllList1;
                //if (R.status1 == "Open")
                //{
                //    R.ProjectList = ViewProjectListOpenBydate();
                //}
                //else if (R.status1 == "Completed") //if (searchBy == "Completed")
                //{
                //    R.ProjectList = ViewProjectList1();
                //}
                //else if (R.status1 == "Cancel")
                //{
                //    R.ProjectList = ViewProjectList2();
                //}
            }
            if (submit == "Export")
            {
                SqlCommand cmd = null;
                SqlConnection con = ptraconn.GetItem();
                SqlDataAdapter da = null;
                if (R.status1 == "Open")
                {
                    cmd = new SqlCommand("SPGetInitializerExport", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = R.GalaxyID;
                    da = new SqlDataAdapter(cmd);
                }
                else if (R.status1 == "Completed")
                {
                    cmd = new SqlCommand("SPGetInitializerCompliteRequest", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = R.GalaxyID;
                    da = new SqlDataAdapter(cmd);
                }
                else if (R.status1 == "Cancel")
                {
                    cmd = new SqlCommand("SPGetInitializerCancelRequest", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = R.GalaxyID;
                    da = new SqlDataAdapter(cmd);

                }
                System.Data.DataTable dtexcel = new System.Data.DataTable();
                da.Fill(dtexcel);
                if (dtexcel.Rows.Count > 0)
                {
                    Microsoft.Office.Interop.Excel.Application app = null;
                    Microsoft.Office.Interop.Excel.Workbook wb = null;
                    Microsoft.Office.Interop.Excel.Worksheet ws = null;

                    app = new Microsoft.Office.Interop.Excel.Application();
                    string dirPath = Server.MapPath("~/DownloadFiles/");
                    DirectoryInfo dir = new DirectoryInfo(dirPath);
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }
                    string filename1 = Path.Combine(dirPath, "Project Track_" + R.status1 + "_Request.xlsx");
                    string sheetName = R.status1 + "_Request";
                    app = new Microsoft.Office.Interop.Excel.Application();
                    app.DisplayAlerts = false;
                    wb = app.Workbooks.Add(1);
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets[1];
                    wb.SaveAs(filename1);
                    ws.Name = sheetName;
                    int i = 0;
                    int j = 0;
                    ws.Columns.ColumnWidth = 40;
                    ws.Cells[i + 1, 1] = "Project No";
                    ws.Range[ws.Cells[i + 1, 1], ws.Cells[i + 1, 1]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 1], ws.Cells[i + 1, 1]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 1], ws.Cells[i + 1, 1]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng.Font.Bold = true;

                    ws.Cells[i + 1, 2] = "Project Category Type";
                    ws.Range[ws.Cells[i + 1, 2], ws.Cells[i + 1, 2]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 2], ws.Cells[i + 1, 2]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 2], ws.Cells[i + 1, 2]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng1 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng1.Font.Bold = true;

                    ws.Cells[i + 1, 3] = "Project Name";
                    ws.Range[ws.Cells[i + 1, 3], ws.Cells[i + 1, 3]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 3], ws.Cells[i + 1, 3]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 3], ws.Cells[i + 1, 3]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng2 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng2.Font.Bold = true;

                    ws.Cells[i + 1, 4] = "Project Description";
                    ws.Range[ws.Cells[i + 1, 4], ws.Cells[i + 1, 4]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 4], ws.Cells[i + 1, 4]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 4], ws.Cells[i + 1, 4]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng3 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng3.Font.Bold = true;

                    ws.Cells[i + 1, 5] = "Assiginee(doer)";
                    ws.Range[ws.Cells[i + 1, 5], ws.Cells[i + 1, 5]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 5], ws.Cells[i + 1, 5]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 5], ws.Cells[i + 1, 5]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng4 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng4.Font.Bold = true;

                    ws.Cells[i + 1, 6] = "Expected Output";
                    ws.Range[ws.Cells[i + 1, 6], ws.Cells[i + 1, 6]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 6], ws.Cells[i + 1, 6]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 6], ws.Cells[i + 1, 6]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng5 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng5.Font.Bold = true;

                    ws.Cells[i + 1, 7] = "Expected Target Date";
                    ws.Range[ws.Cells[i + 1, 7], ws.Cells[i + 1, 7]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 7], ws.Cells[i + 1, 7]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 7], ws.Cells[i + 1, 7]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng6 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng6.Font.Bold = true;

                    ws.Cells[i + 1, 8] = "Proposed Date";
                    ws.Range[ws.Cells[i + 1, 8], ws.Cells[i + 1, 8]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 8], ws.Cells[i + 1, 8]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 8], ws.Cells[i + 1, 8]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng7 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng7.Font.Bold = true;

                    ws.Cells[i + 1, 9] = "Agreed Date";
                    ws.Range[ws.Cells[i + 1, 9], ws.Cells[i + 1, 9]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 9], ws.Cells[i + 1, 9]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 9], ws.Cells[i + 1, 9]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng8 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng8.Font.Bold = true;

                    ws.Cells[i + 1, 10] = "Initiator Status";
                    ws.Range[ws.Cells[i + 1, 10], ws.Cells[i + 1, 10]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 10], ws.Cells[i + 1, 10]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 10], ws.Cells[i + 1, 10]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng9 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng9.Font.Bold = true;

                    ws.Cells[i + 1, 11] = "Doer status";
                    ws.Range[ws.Cells[i + 1, 11], ws.Cells[i + 1, 11]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 11], ws.Cells[i + 1, 11]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 11], ws.Cells[i + 1, 11]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng10 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng10.Font.Bold = true;

                    //ws.Cells[i + 1, 12] = "Remark Trace";
                    //ws.Range[ws.Cells[i + 1, 12], ws.Cells[i + 1, 12]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    //ws.Range[ws.Cells[i + 1, 12], ws.Cells[i + 1, 12]].Font.Color = System.Drawing.Color.White;
                    //ws.Range[ws.Cells[i + 1, 12], ws.Cells[i + 1, 12]].Borders.Color = System.Drawing.Color.Black;
                    //Microsoft.Office.Interop.Excel.Range rng11 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    //rng11.Font.Bold = true;

                    ws.Cells[i + 1, 12] = "Request Date";
                    ws.Range[ws.Cells[i + 1, 12], ws.Cells[i + 1, 12]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 12], ws.Cells[i + 1, 12]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 12], ws.Cells[i + 1, 12]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng11 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng11.Font.Bold = true;

                    ws.Cells[i + 1, 13] = "Requestor Name";
                    ws.Range[ws.Cells[i + 1, 13], ws.Cells[i + 1, 13]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    ws.Range[ws.Cells[i + 1, 13], ws.Cells[i + 1, 13]].Font.Color = System.Drawing.Color.White;
                    ws.Range[ws.Cells[i + 1, 13], ws.Cells[i + 1, 13]].Borders.Color = System.Drawing.Color.Black;
                    Microsoft.Office.Interop.Excel.Range rng12 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    rng12.Font.Bold = true;

                    //ws.Cells[i + 1, 14] = "Requestor Name";
                    //ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    //ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Font.Color = System.Drawing.Color.White;
                    //ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Borders.Color = System.Drawing.Color.Black;
                    //Microsoft.Office.Interop.Excel.Range rng13 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    //rng13.Font.Bold = true;

                    //ws.Cells[i + 1, 15] = "Completed Date";
                    //ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    //ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Font.Color = System.Drawing.Color.White;
                    //ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Borders.Color = System.Drawing.Color.Black;
                    //Microsoft.Office.Interop.Excel.Range rng14 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    //rng14.Font.Bold = true;

                    //ws.Cells[i + 1, 16] = "Completed On";
                    //ws.Range[ws.Cells[i + 1, 16], ws.Cells[i + 1, 16]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    //ws.Range[ws.Cells[i + 1, 16], ws.Cells[i + 1, 16]].Font.Color = System.Drawing.Color.White;
                    //ws.Range[ws.Cells[i + 1, 16], ws.Cells[i + 1, 16]].Borders.Color = System.Drawing.Color.Black;
                    //Microsoft.Office.Interop.Excel.Range rng15 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    //rng15.Font.Bold = true;

                    //ws.Cells[i + 1, 17] = "Requester Head Approval Remark";
                    //ws.Range[ws.Cells[i + 1, 17], ws.Cells[i + 1, 17]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    //ws.Range[ws.Cells[i + 1, 17], ws.Cells[i + 1, 17]].Font.Color = System.Drawing.Color.White;
                    //ws.Range[ws.Cells[i + 1, 17], ws.Cells[i + 1, 17]].Borders.Color = System.Drawing.Color.Black;
                    //Microsoft.Office.Interop.Excel.Range rng16 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    //rng16.Font.Bold = true;

                    //ws.Cells[i + 1, 18] = " Engineering Remark";
                    //ws.Range[ws.Cells[i + 1, 18], ws.Cells[i + 1, 18]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                    //ws.Range[ws.Cells[i + 1, 18], ws.Cells[i + 1, 18]].Font.Color = System.Drawing.Color.White;
                    //ws.Range[ws.Cells[i + 1, 18], ws.Cells[i + 1, 18]].Borders.Color = System.Drawing.Color.Black;
                    //Microsoft.Office.Interop.Excel.Range rng17 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                    //rng17.Font.Bold = true;

                    if (R.status1 == "Open")
                    {
                        ws.Cells[i + 1, 14] = "Pending For";
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng13 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng13.Font.Bold = true;

                        ws.Cells[i + 1, 15] = "Status";
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng14 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng14.Font.Bold = true;
                    }

                    if (R.status1 == "Completed")
                    {
                        ws.Cells[i + 1, 14] = "Doer Comleted Time";
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng13 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng13.Font.Bold = true;

                        ws.Cells[i + 1, 15] = "Doer Comleted By";
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng14 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng14.Font.Bold = true;


                        ws.Cells[i + 1, 16] = "Initiator Comleted Time";
                        ws.Range[ws.Cells[i + 1, 16], ws.Cells[i + 1, 16]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 16], ws.Cells[i + 1, 16]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 16], ws.Cells[i + 1, 16]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng15 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng15.Font.Bold = true;

                        ws.Cells[i + 1, 17] = "Initiator Comleted By";
                        ws.Range[ws.Cells[i + 1, 17], ws.Cells[i + 1, 17]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 17], ws.Cells[i + 1, 17]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 17], ws.Cells[i + 1, 17]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng16 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng16.Font.Bold = true;

                        //ws.Cells[i + 1, 18] = "";
                        //ws.Range[ws.Cells[i + 1, 18], ws.Cells[i + 1, 18]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        //ws.Range[ws.Cells[i + 1, 18], ws.Cells[i + 1, 18]].Font.Color = System.Drawing.Color.White;
                        //ws.Range[ws.Cells[i + 1, 18], ws.Cells[i + 1, 18]].Borders.Color = System.Drawing.Color.Black;
                        //Microsoft.Office.Interop.Excel.Range rng17 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        //rng17.Font.Bold = true;

                        //ws.Cells[i + 1, 19] = "Comleted By-Initiator";
                        //ws.Range[ws.Cells[i + 1, 19], ws.Cells[i + 1, 19]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        //ws.Range[ws.Cells[i + 1, 19], ws.Cells[i + 1, 19]].Font.Color = System.Drawing.Color.White;
                        //ws.Range[ws.Cells[i + 1, 19], ws.Cells[i + 1, 19]].Borders.Color = System.Drawing.Color.Black;
                        //Microsoft.Office.Interop.Excel.Range rng18 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        //rng18.Font.Bold = true;
                    }
                    if (R.status1 == "Cancel")
                    {
                        ws.Cells[i + 1, 14] = "Cancel on";
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 14], ws.Cells[i + 1, 14]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng13 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng13.Font.Bold = true;


                        ws.Cells[i + 1, 15] = "Cancel By";
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Interior.Color = System.Drawing.ColorTranslator.FromHtml("#0099FF");
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Font.Color = System.Drawing.Color.White;
                        ws.Range[ws.Cells[i + 1, 15], ws.Cells[i + 1, 15]].Borders.Color = System.Drawing.Color.Black;
                        Microsoft.Office.Interop.Excel.Range rng14 = (Microsoft.Office.Interop.Excel.Range)ws.Rows[i + 1];
                        rng14.Font.Bold = true;

                    }
                    int ii = 0;

                    for (ii = 0; ii <= dtexcel.Rows.Count - 1; ii++)
                    {
                        for (j = 1; j <= dtexcel.Columns.Count; j++)
                        {
                            int dtCell = j - 1;
                            //if (j == 3)
                            //{
                            //    ws.Cells[i + 2, j] = Convert.ToDateTime(dtexcel.Rows[ii][dtCell]).ToString("dd/MM/yyyy");
                            //}
                            { ws.Cells[i + 2, j] = "'" + dtexcel.Rows[ii][dtCell].ToString(); }
                            ws.Range[ws.Cells[i + 2, j], ws.Cells[i + 2, j]].Borders.Color = System.Drawing.Color.Black;
                        }
                        i++;
                    }

                    ws.Columns.EntireColumn.AutoFit();
                    wb.Save();
                    wb.Close();
                    app.Quit();

                    string fileName2 = "Project Track_" + R.status1 + "_Request.xlsx";
                    System.IO.FileStream fs = null;
                    fs = System.IO.File.Open(Server.MapPath("~/DownloadFiles/" + fileName2), System.IO.FileMode.Open);
                    byte[] btFile = new byte[fs.Length];
                    fs.Read(btFile, 0, Convert.ToInt32(fs.Length));
                    fs.Close(); Response.AddHeader("Content-disposition", "attachment; filename=" + fileName2);
                    Response.ContentType = "application/octet-stream";
                    Response.BinaryWrite(btFile);
                    Response.End();

                    return View("../Initiator/ViewProjects");

                }
            }
            
            R.EmpCode = Session["Empcode"].ToString();
            R.GalaxyID = GalaxyID;
            return View("../Initiator/ViewProjects", R);
        }

        [HttpGet]
        public ActionResult Edit(int id, Request R)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            Session["Role"] = GetRole(GalaxyID);
            R.GalaxyIDName = GetDisplayName(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            Session["username"] = (GalaxyID);
            con = ptraconn.GetItem();
            TempData["ID"] = id;
            Session["ID"] = TempData["ID"];
            SqlCommand cmd1 = new SqlCommand("GetTransferProject", con);
            cmd1.CommandType = CommandType.StoredProcedure;
            cmd1.Parameters.Add("@projectno", SqlDbType.Int).Value = id;
            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
            DataSet ds1 = new DataSet();
            da1.Fill(ds1);
            for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
            {
                R.ProjectNo = Convert.ToInt32(ds1.Tables[0].Rows[i]["ProjectNo"].ToString());
                R.ProjectName = ds1.Tables[0].Rows[i]["ProjectName"].ToString();
                R.ProjectDescription = ds1.Tables[0].Rows[i]["ProjectDescription"].ToString();
                R.ExpectedOutput1 = ds1.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                R.AddedBy = ds1.Tables[0].Rows[i]["AddedBy"].ToString();
                R.NextStatus = Convert.ToInt32(ds1.Tables[0].Rows[0]["CurrentStatus"].ToString());
                TempData["NextStatus"] = R.NextStatus;
                //R.InitiatorName = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["InitilizerRemark"].ToString()) ? R.InitiatorStatus : (ds1.Tables[0].Rows[0]["InitilizerRemark"]).ToString();               
                // R.FinalStatus = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["FinalStatus"].ToString()) ? R.FinalStatus : (ds1.Tables[0].Rows[0]["FinalStatus"]).ToString();
                R.ExpectedTargetDate = Convert.ToDateTime(ds1.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                R.ProposedDate = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["proposeddate"].ToString()) ? R.ExpectedTargetDate : Convert.ToDateTime(ds1.Tables[0].Rows[0]["proposeddate"]).ToString("dd-MMM-yyyy");

                R.GalaxyID = GalaxyID;
                R.DoerName1 = ds1.Tables[0].Rows[i]["DoerName"].ToString();
                R.DoerNameGalaxy = R.DoerName1;
                R.DoerbyName = GetDisplayName(ds1.Tables[0].Rows[i]["DoerName"].ToString());
                R.InitiatorName = GetDisplayName(ds1.Tables[0].Rows[i]["AddedBy"].ToString());
                R.pendingfor = ds1.Tables[0].Rows[i]["pendingfor"].ToString();
                R.pendingforGalaxy = ds1.Tables[0].Rows[i]["pendingforGalaxy"].ToString();
                R.DoerStatus = ds1.Tables[0].Rows[i]["DoerStatus"].ToString();
                R.InitiatorStatus = ds1.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                R.ReviewByGalaxy = ds1.Tables[0].Rows[i]["ReviewBy"].ToString();
                R.ReviewBy = GetDisplayName(R.ReviewByGalaxy);
                R.attachment1 = ds1.Tables[0].Rows[i]["Attachment"].ToString();
                if (!(string.IsNullOrEmpty(ds1.Tables[0].Rows[i]["DelegateToName"].ToString())))
                {
                    R.DelegateTo = ds1.Tables[0].Rows[i]["DelegateToName"].ToString();
                    R.DelegateToGalaxy = ds1.Tables[0].Rows[i]["DelegateTo"].ToString();
                }
                else
                {
                    R.DelegateTo = R.DoerbyName;
                    R.DelegateToGalaxy = R.DoerNameGalaxy;
                }
                R.oldExpDate = Convert.ToDateTime(ds1.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                R.oldProposedDate = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["proposeddate"].ToString()) ? R.ExpectedTargetDate : Convert.ToDateTime(ds1.Tables[0].Rows[0]["proposeddate"]).ToString("dd-MMM-yyyy");
                R.InitiatorRemark = ds1.Tables[0].Rows[i]["InitiatorRemark"].ToString();
                R.DelegateStatus = ds1.Tables[0].Rows[i]["DelegateStatus"].ToString();
                R.ReviewStatus = ds1.Tables[0].Rows[i]["ReviewByStatus"].ToString();
                R.RequestOn = Convert.ToDateTime(ds1.Tables[0].Rows[i]["AddedOn"]).ToString("dd-MMM-yyyy");
                R.ReqStatus = ds1.Tables[0].Rows[i]["Status"].ToString();

                if (ds1.Tables[0].Rows[0]["AddedBy"].ToString() != "")
                {
                    R.HistorySubmitedBy = "(" + GetDisplayName(ds1.Tables[0].Rows[0]["AddedBy"].ToString()) + "), " + Convert.ToDateTime(ds1.Tables[0].Rows[0]["AddedOn"].ToString()).ToString("dd-MMM-yyyy hh:mm tt");
                }
                if (R.AddedBy == Session["GalaxyID"].ToString())
                {
                    R.Role = "Initiator";
                }
                else if (R.DoerName1 == Session["GalaxyID"].ToString() || R.DelegateToGalaxy == Session["GalaxyID"].ToString())
                {
                    R.Role = "Doer";
                }
                else if (R.DelegateToGalaxy == Session["GalaxyID"].ToString())
                {
                    R.Role = "Delegate";
                }
                else if (R.ReviewByGalaxy == Session["GalaxyID"].ToString())
                {
                    R.Role = "Review";
                }

                TempData["Role"] = R.Role;
            }
            R.DoerName = GetDoer(R.EmpCode);
            R.RemarkTrace = GetAllRemarks(id,R);           
            R.EmpCode = Session["Empcode"].ToString();
            R.GalaxyID = GalaxyID;
            R.AssigneeList = GetDelegatAssigneeList();
            R.AssigneeList1 = GetAssigneeList();
            SqlCommand cmdnames = new SqlCommand("GetActivitesby", con);
            cmdnames.CommandType = CommandType.StoredProcedure;
            cmdnames.Parameters.Add("@TaskNo", SqlDbType.VarChar).Value = id;
            SqlDataAdapter danames = new SqlDataAdapter(cmdnames);
            DataSet dsnames = new DataSet();
            danames.Fill(dsnames);

            for (int i = 0; i < dsnames.Tables[0].Rows.Count; i++)
            {
                string LastActivityRole = dsnames.Tables[0].Rows[i]["LastActivityRole"].ToString();
                string Names = dsnames.Tables[0].Rows[i]["LastActivityByName"].ToString();
                switch (LastActivityRole)
                {
                    case "Initiator": R.Initiator   = Names; break;
                    case "Doer"     : R.Doer        = Names; break;
                    case "Delegate" : R.Delegate    = Names; break;
                    case "Review"   : R.Review      = Names; break;                    
                }
            }
            return View("../Initiator/edit", R);
        }
        [HttpPost]
        public List<SelectListItem> GetDelegatAssigneeList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand("GetDelegatAssigneeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = Session["Empcode"].ToString();
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        //if (@dr["EmpCode"].ToString() == Session["Empcode"].ToString())
                        //{
                        //    SelectListItem s1 = new SelectListItem
                        //    {
                        //        Text = @dr["DisplayName"].ToString(),
                        //        Value = @dr["GalaxyId"].ToString(),
                        //        Selected=true
                        //    };
                        //    list.Add(s1);
                        //}
                        //else
                        //{
                            SelectListItem s1 = new SelectListItem
                            {
                                Text = @dr["DisplayName"].ToString() +" ("+ @dr["GalaxyId"].ToString() + ")",
                                Value = @dr["GalaxyId"].ToString() 
                            };
                            list.Add(s1);
                        //}
                    }
                }
                else
                {
                    SelectListItem s1 = new SelectListItem
                    {
                        Text = "",
                        Value = "0"
                    };
                    list.Add(s1);
                }
                return list;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }
        [HttpPost]
        public ActionResult Edit(Request R, string Submit, string StartTracking, string Back)
        {
            
            var model = new List<Request>();
            if (Submit == "Accept Task" || Submit == "Accept & Delegate Task" || Submit=="Submit")
            {
                Boolean updateflag;
                updateflag = UpdateProject(R);

            }else if(Submit == "Cancel Task")
            {
                int i = UpdateCancelTask(R);
                UpdateProjectStatus1(R, "Initiator", "Initiator", R.InitiatorRemark, "Cancel", R.oldExpDate, R.oldProposedDate);
                SendEmail(R.ProjectNo.ToString(), R, "Task Cancelled");
            }
            else if (Back == "Back")
            {
                ModelState.Clear();
                return RedirectToAction("ViewProjects", "Initiator");
            }
            else if (Submit == "Update Task")
            {
                int i = UpdateTask(R);
                UpdateProjectStatus1(R, "Initiator", "Initiator", R.InitiatorRemark, "IS",R.oldExpDate,R.oldProposedDate);
                SendEmail(R.ProjectNo.ToString(), R, "Task Details Changed");

            }
            return RedirectToAction("ViewProjects", "Initiator");
        }
        [HttpPost]
        public ActionResult Back()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            Session["Role"] = GetRole(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            return RedirectToAction("ViewProjects", "Initiator");
        }
        [HttpGet]
        public ActionResult ViewReportDetails(int id, Request R)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            Session["Role"] = GetRole(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            Session["username"] = (GalaxyID);
            con = ptraconn.GetItem();
            TempData["ID"] = id;
            Session["ID"] = TempData["ID"];
            SqlCommand cmd1 = new SqlCommand("GetTransferProject", con);
            cmd1.CommandType = CommandType.StoredProcedure;
            cmd1.Parameters.Add("@projectno", SqlDbType.Int).Value = id;
            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
            DataSet ds1 = new DataSet();
            da1.Fill(ds1);
            for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
            {
                R.ProjectNo = Convert.ToInt32(ds1.Tables[0].Rows[i]["ProjectNo"].ToString());

                R.ProjectName = ds1.Tables[0].Rows[i]["ProjectName"].ToString();
                R.ProjectDescription = ds1.Tables[0].Rows[i]["ProjectDescription"].ToString();
                R.ExpectedOutput1 = ds1.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                R.AddedBy = ds1.Tables[0].Rows[i]["AddedBy"].ToString();
                R.NextStatus = Convert.ToInt32(ds1.Tables[0].Rows[0]["CurrentStatus"].ToString());
                TempData["NextStatus"] = R.NextStatus;
                //R.InitiatorName = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["InitilizerRemark"].ToString()) ? R.InitiatorStatus : (ds1.Tables[0].Rows[0]["InitilizerRemark"]).ToString();               
                R.FinalStatus = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["FinalStatus"].ToString()) ? R.FinalStatus : (ds1.Tables[0].Rows[0]["FinalStatus"]).ToString();
                R.ExpectedTargetDate = Convert.ToDateTime(ds1.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                R.ProposedDate = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["proposeddate"].ToString()) ? R.ExpectedTargetDate : Convert.ToDateTime(ds1.Tables[0].Rows[0]["proposeddate"]).ToString("dd-MMM-yyyy");
                R.AgreedDate = string.IsNullOrEmpty(ds1.Tables[0].Rows[0]["AgreedDate"].ToString()) ? R.ProposedDate : Convert.ToDateTime(ds1.Tables[0].Rows[0]["AgreedDate"]).ToString("dd-MMM-yyyy");
                R.GalaxyID = GalaxyID;
                R.DoerName1 = ds1.Tables[0].Rows[i]["DoerName"].ToString();
                R.DoerbyName = GetDisplayName(ds1.Tables[0].Rows[i]["DoerName"].ToString());
                R.InitiatorName = GetDisplayName(ds1.Tables[0].Rows[i]["AddedBy"].ToString());
                R.pendingfor = ds1.Tables[0].Rows[i]["pendingfor"].ToString();
                R.DoerStatus = ds1.Tables[0].Rows[i]["DoerStatus"].ToString();
                R.InitiatorStatus = ds1.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                if (ds1.Tables[0].Rows[0]["AddedBy"].ToString() != "")
                {
                    R.HistorySubmitedBy = "(" + GetDisplayName(ds1.Tables[0].Rows[0]["AddedBy"].ToString()) + "), " + Convert.ToDateTime(ds1.Tables[0].Rows[0]["AddedOn"].ToString()).ToString("dd-MMM-yyyy hh:mm tt");
                }
                if (R.AddedBy == Session["GalaxyID"].ToString())
                {
                    R.Role = "Initiator";
                }
                else if (R.DoerName1 == Session["GalaxyID"].ToString())
                {
                    R.Role = "Doer";
                }
                TempData["Role"] = R.Role;
            }
            R.DoerName = GetDoer(R.EmpCode);
            R.RemarkTrace = GetAllRemarks(id,R);
            //R.Role = GetGrade(GalaxyID);
            R.EmpCode = Session["Empcode"].ToString();
            R.GalaxyID = GalaxyID;
            return View("ViewHODReportDetails", R);
        }

        [HttpPost]
        public ActionResult ViewReportDetails(Request R, string Submit, string StartTracking, string Back)
        {
            if (Back == "Back")
            {
                return RedirectToAction("ReportForHOD", "Initiator");
            }
            return RedirectToAction("ReportForHOD", "Initiator");
        }

        [HttpPost]
        public JsonResult GetAutoDoer(string Prefix, string EmpCode)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetAssigneeList", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = Session["Empcode"].ToString();
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DoerName> allEmp = new List<DoerName>();
            //Emp names
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    DoerName s1 = new DoerName
                    {
                        Key = @dr["EmpId"].ToString(),
                        Display = @dr["DisplayName"].ToString()
                    };
                    allEmp.Add(s1);
                }
            }

            var DoerName1 = (from N in allEmp
                             where N.Display.StartsWith(Prefix)
                             select new { N.Display });
            return Json(DoerName1, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReportForHOD()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            Request obj = new Request();
            obj.ProjectList = ReportForHODList();
            obj.EmpCode = Session["Empcode"].ToString();
            obj.GalaxyID = GalaxyID;
            return View("ReportForHOD", obj);
        }

        public List<ProjectList> ReportForHODList()
        {
            Session["pendingfor"] = "";
            var AllList = new List<ProjectList>();
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("ReportForHODList", con);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var model = new ProjectList();
                model.ProjectNo = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectNo"].ToString());

                model.ProjectName = ds.Tables[0].Rows[i]["ProjectName"].ToString();
                model.ProjectDescription = ds.Tables[0].Rows[i]["ProjectDescription"].ToString();
                model.ExpectedOutput = ds.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                model.AddedBy = ds.Tables[0].Rows[i]["AddedByName"].ToString();
                model.InitiatorStatus = ds.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                model.DoerName1 = ds.Tables[0].Rows[i]["DoerByName"].ToString();
                model.DoerStatus = ds.Tables[0].Rows[i]["DoerStatus"].ToString();
                model.pendingfor = ds.Tables[0].Rows[i]["pendingfor"].ToString();
                model.Status = ds.Tables[0].Rows[i]["Status"].ToString();
                model.ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                model.ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["proposeddate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["proposeddate"]).ToString("dd-MMM-yyyy");
                model.AgreedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["AgreedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["AgreedDate"]).ToString("dd-MMM-yyyy");

                model.ComletedOn = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["InitiatorCompletedOn"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["InitiatorCompletedOn"]).ToString("dd-MMM-yyyy");


                model.RequestOn = Convert.ToDateTime(ds.Tables[0].Rows[i]["AddedOn"].ToString()).ToString("dd-MMM-yyyy");
                model.OntimePer = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["OntimePercent"].ToString()) ? 0 : Convert.ToInt32(ds.Tables[0].Rows[i]["OntimePercent"].ToString());
                AllList.Add(model);
                TempData["AllList"] = AllList;
                Session["AllList"] = TempData["AllList"];
            }
            return AllList;
        }

        public string ProjectSubmit(Request Req)
        {
            DateTime Date = Convert.ToDateTime(Req.ExpectedTargetDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            con = ptraconn.GetItem();           
            SqlCommand cmd = new SqlCommand("InsertNewProject", con);
            cmd.CommandType = CommandType.StoredProcedure;
            con.Open();
            cmd.Parameters.Add("@projectname", SqlDbType.VarChar).Value = Req.ProjectName;
            cmd.Parameters.Add("@projectdescription", SqlDbType.VarChar).Value = Req.ProjectDescription;
            cmd.Parameters.Add("@expectedoutput", SqlDbType.VarChar).Value = Req.ExpectedOutput1;
            cmd.Parameters.Add("@addedby", SqlDbType.VarChar).Value = Session["GalaxyID"].ToString();
            cmd.Parameters.Add("@expecteddate", SqlDbType.DateTime).Value = Date;
            cmd.Parameters.Add("@DoerName", SqlDbType.VarChar).Value = Req.DoerNameGalaxy.ToString();
            cmd.Parameters.Add("@ReviewBy", SqlDbType.VarChar).Value = Req.ReviewByGalaxy.ToString();
            SqlParameter ReqNo = new SqlParameter("@PrjNo", SqlDbType.VarChar, 50);
            ReqNo.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(ReqNo).Value = 0;

            cmd.ExecuteNonQuery();
            string Prjno = (string)cmd.Parameters["@PrjNo"].Value;
            con.Close();
            return Prjno;
        }

        public string TransferProjectSubmit(Request Req)
        {
            con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("InsertTransferProject", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();

            cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = Req.ProjectNo;
            cmd.Parameters.Add("@TransferTo", SqlDbType.VarChar).Value = Req.DoerNameGalaxy;
            SqlParameter ReqNo = new SqlParameter("@PrjNo", SqlDbType.VarChar, 50);
            ReqNo.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(ReqNo).Value = 0;
            cmd.ExecuteNonQuery();
            string Prjno = (string)cmd.Parameters["@PrjNo"].Value;
            con.Close();
            return Prjno;

        }

        public bool UpdateProject(Request Req)
        {
            DateTime Date = Convert.ToDateTime(Req.ProposedDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            Boolean issuccessfull = false;
            if (Req.NextStatus == 1)
            {
                con = ptraconn.GetItem();
                SqlCommand cmd = new SqlCommand("UpdateStatusProject", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = Req.ProjectNo;
                cmd.Parameters.Add("@CurrStatus", SqlDbType.VarChar).Value = Req.NextStatus;
                cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = Req.NextStatus + 1;
                cmd.Parameters.Add("@SubmittedDate", SqlDbType.DateTime).Value = Date;
                cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = null;
                cmd.Parameters.Add("@Attachment", SqlDbType.VarChar).Value = null;
                if (!(string.IsNullOrEmpty(Req.DelegateToGalaxy)))
                {
                    cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = Req.DelegateToGalaxy;
                }
                else
                {
                    cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = null;
                }
                cmd.Parameters.Add("@SubmitedBy", SqlDbType.VarChar).Value = Req.Role;

                cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = Req.GalaxyID;
                if (Convert.ToBoolean(cmd.ExecuteNonQuery()))
                {
                    issuccessfull = true;
                }
                con.Close();
                if (!(string.IsNullOrEmpty(Req.DelegateToGalaxy)))
                {
                    UpdateProjectStatus(Req, "Doer", "Delegate", Req.ProposedDateRemark, "O");
                    if (Req.DoerNameGalaxy != Req.DelegateToGalaxy)
                    {
                      SendEmail(Req.ProjectNo.ToString(), Req, "Proposed Date Submitted and Delegated");
                    }
                    else
                    {
                        SendEmail(Req.ProjectNo.ToString(), Req, "Proposed Date Submitted");
                    }
                }
                //else if ((string.IsNullOrEmpty(Req.DelegateToGalaxy)))
                //{
                //    UpdateProjectStatus(Req, "Doer", "Doer", Req.ProposedDateRemark, "O");
                //    int i = SendEmail(Req.ProjectNo.ToString(), Req, "Progress Updated by Doer");
                //}
                //else if (!(string.IsNullOrEmpty(Req.ReviewByGalaxy)))
                //{
                //    UpdateProjectStatus(Req, "Doer", "Review", Req.ProposedDateRemark, "O");
                //    int i = SendEmail(Req.ProjectNo.ToString(), Req, "");
                //}
            }
            else if (Req.NextStatus == 2)
            {
                con = ptraconn.GetItem();
                SqlCommand cmd = new SqlCommand("UpdateStatusProject", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = Req.ProjectNo;
                cmd.Parameters.Add("@CurrStatus", SqlDbType.VarChar).Value = Req.NextStatus;
                if (Req.doerAttachment != null)
                {
                    cmd.Parameters.Add("@Attachment", SqlDbType.VarChar).Value = Path.GetFileName(Req.doerAttachment.FileName);
                }
                else
                {
                    cmd.Parameters.Add("@Attachment", SqlDbType.VarChar).Value = null;
                }
                if (!(string.IsNullOrEmpty(Req.DelegateStatus)))
                {
                    cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = Req.NextStatus + 1;
                    cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = Req.DelegateStatus;
                    cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = null;
                }
                else if ((Req.DelegateToGalaxy == Req.DoerNameGalaxy) || (Req.DelegateToGalaxy != Req.DoerNameGalaxy))
                {
                    if (Req.DoerStatus == "Completed") // not null
                    {
                        cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = "Same";
                        cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = Req.NextStatus + 1;
                        cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = Req.DoerStatus;
                    }
                    else if (Req.DoerStatus=="Pending")//null
                    {
                        cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = null;
                        cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = null;
                        cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = null;
                    }
                }
                else
                {
                    cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = null;
                    cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = null;
                    cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = null;
                }
                cmd.Parameters.Add("@SubmittedDate", SqlDbType.DateTime).Value = null;
                cmd.Parameters.Add("@SubmitedBy", SqlDbType.VarChar).Value = Req.Role;
               
                cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = Req.GalaxyID;
                if (Convert.ToBoolean(cmd.ExecuteNonQuery()))
                {
                    issuccessfull = true;
                }
                con.Close();

                string path1 = @"" + Server.MapPath("~/Uploads/" + Req.ProjectNo + "/") + "";
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(path1);
                }

                string[] Files = Directory.GetFiles(path1);

                if (Req.doerAttachment != null)
                {
                    bool exists = Directory.EnumerateFiles(path1, "Attachment_" + "*.*").Any();
                    if (exists == true)
                    {
                        foreach (string file in Files)
                        {
                            if (file.Contains(path1 + "Attachment_"))
                            {
                                System.IO.File.Delete(file);

                            }
                        }
                    }
                    if (Req.doerAttachment != null)
                    {
                        if (Req.doerAttachment.ContentLength > 0)
                        {
                            string filenameTestReport = Path.Combine(path1, "Attachment__" + Path.GetFileName(Req.doerAttachment.FileName));
                            Req.doerAttachment.SaveAs(filenameTestReport);
                        }
                    }
                }


                if (!(string.IsNullOrEmpty(Req.DelegateStatus)))
                {
                    UpdateProjectStatus(Req, "Delegate", "Review", Req.Remark, "C");
                    SendEmail(Req.ProjectNo.ToString(), Req, "Task Completed by Doer");
                }
                else if (Req.DoerStatus=="Completed")
                {
                    UpdateProjectStatus(Req, "Delegate", "Review", Req.Remark, "C");
                    SendEmail(Req.ProjectNo.ToString(), Req, "Task Completed by Doer");
                }
                else
                {
                    UpdateProjectStatus(Req, "Delegate", "Delegate", Req.Remark, "O");
                    SendEmail(Req.ProjectNo.ToString(), Req,  "Progress Updated by Doer");
                }



            }
            else if (Req.NextStatus == 3)
            {
                con = ptraconn.GetItem();
                SqlCommand cmd = new SqlCommand("UpdateStatusProject", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = Req.ProjectNo;
                cmd.Parameters.Add("@Attachment", SqlDbType.VarChar).Value = null;
                if (!(string.IsNullOrEmpty(Req.ReviewStatus)))
                {
                    if (Req.ReviewStatus == "Not Completed")
                    {
                        cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = Req.NextStatus - 1;
                        cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = Req.ReviewStatus;

                    }
                    else if (Req.ReviewStatus == "Verified")
                    {
                        cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = Req.NextStatus + 1;
                        cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = Req.ReviewStatus;
                    }
                }
                else
                {
                    cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = Req.NextStatus;
                    cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = null;

                }
                cmd.Parameters.Add("@CurrStatus", SqlDbType.VarChar).Value = Req.NextStatus;

                cmd.Parameters.Add("@SubmittedDate", SqlDbType.DateTime).Value = null;
                cmd.Parameters.Add("@delegateTo", SqlDbType.VarChar).Value = null;
                cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = Req.GalaxyID;
                if (Convert.ToBoolean(cmd.ExecuteNonQuery()))
                {
                    issuccessfull = true;
                }
                con.Close();

                if (!(string.IsNullOrEmpty(Req.ReviewStatus)))
                {
                    if (Req.ReviewStatus == "Not Completed")
                    {
                        if (!(string.IsNullOrEmpty(Req.DelegateToGalaxy)))
                        {
                            UpdateProjectStatus(Req, "Review", "Delegate", Req.Remark, "O");
                            SendEmail(Req.ProjectNo.ToString(), Req, "Reviewer Submitted Recommendation");
                        }
                        else
                        {
                            UpdateProjectStatus(Req, "Review", "Doer", Req.Remark, "O");
                            SendEmail(Req.ProjectNo.ToString(), Req, "Reviewer Submitted Recommendation");
                        }
                    }
                    else if (Req.ReviewStatus == "Verified")
                    {
                        UpdateProjectStatus(Req, "Review", "Close", Req.Remark, "C");
                        SendEmail(Req.ProjectNo.ToString(), Req, "Reviewer Verified and Task Completed");
                    }
                }
                else
                {
                    UpdateProjectStatus(Req, "Review", "Review", Req.Remark, "O");
                    SendEmail(Req.ProjectNo.ToString(), Req, "Progress Updated by Reviewer");
                }
            }
            //else if (Req.NextStatus == 4)
            //{
            //    con = ptraconn.GetItem();
            //    SqlCommand cmd = new SqlCommand("UpdateStatusProject", con);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    con.Open();
            //    cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = Req.ProjectNo;
            //    cmd.Parameters.Add("@CurrStatus", SqlDbType.VarChar).Value = Req.NextStatus;
            //    if (Req.InitiatorStatus == "Completed")
            //    {
            //        cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = 5;
            //    }
            //    else if (Req.InitiatorStatus == "Not Completed")
            //    {
            //        cmd.Parameters.Add("@NextStatus", SqlDbType.VarChar).Value = 2;
            //    }
            //    cmd.Parameters.Add("@TaskStatus", SqlDbType.VarChar).Value = Req.InitiatorStatus;
            //    cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = Req.GalaxyID;
            //    cmd.Parameters.Add("@SubmitedBy", SqlDbType.VarChar).Value = Req.Role;
            //    if (Convert.ToBoolean(cmd.ExecuteNonQuery()))
            //    {
            //        issuccessfull = true;
            //    }
            //    con.Close();
            //}

            return issuccessfull;
        }

        public bool UpdateProjectStatus(Request Req, string LastActivityRole, string NextRole, string Remark, string Status)
        {
            Boolean issuccessfull = false;
            con = ptraconn.GetItem();
            con.Open();
            SqlCommand cmd = new SqlCommand("UpdateProjectStatus", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@projectno", Req.ProjectNo);
            cmd.Parameters.Add("@LastActivityRole", LastActivityRole);
            cmd.Parameters.Add("@NextRole", NextRole);
            cmd.Parameters.Add("@Status", Status);
            cmd.Parameters.Add("@LastActivityBy", Session["GalaxyID"].ToString());
            cmd.Parameters.Add("@LastActivityOn", DateTime.Now);
            cmd.Parameters.Add("@Remark", Remark);
            if (Convert.ToBoolean(cmd.ExecuteNonQuery()))
            {
                issuccessfull = true;
            }
            con.Close();
            return issuccessfull;
        }

        public bool UpdateProjectStatus1(Request Req, string LastActivityRole, string NextRole, string Remark, string Status, string oldExpDate, string oldProposedDate)
        {
            Boolean issuccessfull = false;
            con = ptraconn.GetItem();
            con.Open();
            SqlCommand cmd = new SqlCommand("UpdateProjectStatusTask", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@projectno", Req.ProjectNo);
            cmd.Parameters.Add("@LastActivityRole", LastActivityRole);
            cmd.Parameters.Add("@NextRole", NextRole);
            cmd.Parameters.Add("@Status", Status);
            cmd.Parameters.Add("@LastActivityBy", Session["GalaxyID"].ToString());
            cmd.Parameters.Add("@LastActivityOn", DateTime.Now);
            cmd.Parameters.Add("@Remark", Remark);
            cmd.Parameters.Add("@OldExDate", oldExpDate);
            cmd.Parameters.Add("@OldProposedDate", oldProposedDate);

            if (Convert.ToBoolean(cmd.ExecuteNonQuery()))
            {
                issuccessfull = true;
            }
            con.Close();
            return issuccessfull;
        }

        public String GetGrade(string GalaxyID)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetGrade", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = GalaxyID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string Grade = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                Grade = ds.Tables[0].Rows[0]["Grade"].ToString();
            }
            return Grade;
        }

        public String GetDisplayName(string GalaxyID)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("DisplayName", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = GalaxyID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string DisplayName = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                DisplayName = ds.Tables[0].Rows[0]["DisplayName"].ToString();
            }
            return DisplayName;
        }

        public String GetLoginEmpCode(string GalaxyID)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetLoginEmpCode", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string LoginEmpCode = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                LoginEmpCode = ds.Tables[0].Rows[0]["EmpCode"].ToString();
            }

            return LoginEmpCode;
        }

        public List<ProjectList> ViewProjectList()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";

            }
            Session["pendingfor"] = "";
            var AllList = new List<ProjectList>();
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("SPGetInitializer", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var model = new ProjectList();
                model.ProjectNo = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectNo"].ToString());
                model.ProjectName = ds.Tables[0].Rows[i]["ProjectName"].ToString();
                model.ProjectDescription = ds.Tables[0].Rows[i]["ProjectDescription"].ToString();
                model.ExpectedOutput = ds.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                model.AddedBy = ds.Tables[0].Rows[i]["AddedByName"].ToString();
                model.AddedByGalaxy = ds.Tables[0].Rows[i]["AddedBy"].ToString();
                model.InitiatorStatus = ds.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                model.DoerName1 = ds.Tables[0].Rows[i]["DoerByName"].ToString();
                model.DoerStatus = ds.Tables[0].Rows[i]["DoerStatus"].ToString();
                model.pendingfor = ds.Tables[0].Rows[i]["pendingfor"].ToString();
                model.Status = ds.Tables[0].Rows[i]["Status"].ToString();
                model.ReviewBy = ds.Tables[0].Rows[i]["ReviewByName"].ToString();
                model.ReviewByGalaxy = ds.Tables[0].Rows[i]["ReviewBy"].ToString();
                model.DelegateTo = ds.Tables[0].Rows[i]["DelegateToName"].ToString();
                model.DelegateToGalaxy = ds.Tables[0].Rows[i]["DelegateTo"].ToString();
                model.ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                model.ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["proposeddate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["proposeddate"]).ToString("dd-MMM-yyyy");
                model.DoerNameGalaxy= ds.Tables[0].Rows[i]["DoerName"].ToString();
                model.GalaxyID = GalaxyID;
                model.RequestOn = Convert.ToDateTime(ds.Tables[0].Rows[i]["AddedOn"]).ToString("dd-MMM-yyyy");
                AllList.Add(model);
                TempData["AllList"] = AllList;
                Session["AllList"] = TempData["AllList"];
            }
            return AllList;
        }

        public List<ProjectList> ViewProjectListOpenBydate()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                
            }
            Session["GalaxyID"] = GalaxyID;
            Session["pendingfor"] = "";

            var AllList = new List<ProjectList>();
            //if(Session["Role"].ToString()=="Initiator")
            //{
            SqlConnection con = ptraconn.GetItem();

            SqlCommand cmd = new SqlCommand("SPGetInitializerByDate", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var model = new ProjectList();
                model.ProjectNo = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectNo"].ToString());

                model.ProjectName = ds.Tables[0].Rows[i]["ProjectName"].ToString();
                model.ProjectDescription = ds.Tables[0].Rows[i]["ProjectDescription"].ToString();
                model.ExpectedOutput = ds.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                model.AddedBy = ds.Tables[0].Rows[i]["AddedByName"].ToString();
                model.InitiatorStatus = ds.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                model.DoerName1 = ds.Tables[0].Rows[i]["DoerByName"].ToString();
                model.DoerStatus = ds.Tables[0].Rows[i]["DoerStatus"].ToString();
                model.pendingfor = ds.Tables[0].Rows[i]["pendingfor"].ToString();
                model.Status = ds.Tables[0].Rows[i]["Status"].ToString();
                model.ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                model.ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["proposeddate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["proposeddate"]).ToString("dd-MMM-yyyy");
                model.AgreedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["AgreedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["AgreedDate"]).ToString("dd-MMM-yyyy");
                AllList.Add(model);
                TempData["AllList"] = AllList;
                Session["AllList"] = TempData["AllList"];
            }
            return AllList;
        }

        public List<ProjectList> ViewProjectList1()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                
            }
            Session["GalaxyID"] = GalaxyID;
            Session["pendingfor"] = "";

            var AllList = new List<ProjectList>();
            //if(Session["Role"].ToString()=="Initiator")
            //{
            SqlConnection con = ptraconn.GetItem();

            SqlCommand cmd = new SqlCommand("SPGetInitializerCompliteRequest", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var model = new ProjectList();
                model.ProjectNo = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectNo"].ToString());

                model.ProjectName = ds.Tables[0].Rows[i]["ProjectName"].ToString();
                model.ProjectDescription = ds.Tables[0].Rows[i]["ProjectDescription"].ToString();
                model.ExpectedOutput = ds.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                model.AddedBy = ds.Tables[0].Rows[i]["AddedBy"].ToString();
                model.InitiatorStatus = ds.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                model.DoerName1 = ds.Tables[0].Rows[i]["DoerName"].ToString();

                model.DoerStatus = ds.Tables[0].Rows[i]["DoerStatus"].ToString();

                model.pendingfor = ds.Tables[0].Rows[i]["pendingfor"].ToString();


                model.ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");

                model.ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["proposeddate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["proposeddate"]).ToString("dd-MMM-yyyy");
                model.AgreedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["AgreedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["AgreedDate"]).ToString("dd-MMM-yyyy");


                AllList.Add(model);


                TempData["AllList"] = AllList;
                Session["AllList"] = TempData["AllList"];


            }
            return AllList;
        }

        public List<ProjectList> ViewProjectList2()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";

            }
            Session["GalaxyID"] = GalaxyID;
            Session["pendingfor"] = "";

            var AllList = new List<ProjectList>();
            //if(Session["Role"].ToString()=="Initiator")
            //{
            SqlConnection con = ptraconn.GetItem();

            SqlCommand cmd = new SqlCommand("SPGetInitializerCompliteRequest", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var model = new ProjectList();
                model.ProjectNo = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectNo"].ToString());

                model.ProjectName = ds.Tables[0].Rows[i]["ProjectName"].ToString();
                model.ProjectDescription = ds.Tables[0].Rows[i]["ProjectDescription"].ToString();
                model.ExpectedOutput = ds.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                model.AddedBy = ds.Tables[0].Rows[i]["AddedBy"].ToString();
                model.InitiatorStatus = ds.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                model.DoerName1 = ds.Tables[0].Rows[i]["DoerName"].ToString();

                model.DoerStatus = ds.Tables[0].Rows[i]["DoerStatus"].ToString();

                model.pendingfor = ds.Tables[0].Rows[i]["pendingfor"].ToString();


                model.ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");

                model.ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["proposeddate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["proposeddate"]).ToString("dd-MMM-yyyy");
                model.AgreedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["AgreedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["AgreedDate"]).ToString("dd-MMM-yyyy");


                AllList.Add(model);


                TempData["AllList"] = AllList;
                Session["AllList"] = TempData["AllList"];


            }
            return AllList;
        }

        public List<DoerName> GetDoer(string EmpCode)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetAssigneeList", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = Session["Empcode"].ToString();
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DoerName> allEmp = new List<DoerName>();

            //Emp names
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    DoerName s1 = new DoerName
                    {
                        Key = @dr["EmpId"].ToString(),
                        Display = @dr["DisplayName"].ToString()
                    };
                    allEmp.Add(s1);
                }
            }

            con.Close();
            return allEmp;

        }

        public string GetAllRemarks(int Projid,Request R)
        {
            string RemarkFormated = "";
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetProjectStatusRemark", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@projectno", SqlDbType.Int).Value = Projid;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {

                    RemarkFormated = RemarkFormated + "\n" + (Convert.ToDateTime(ds.Tables[0].Rows[i]["LastActivityOn"].ToString())).ToLongDateString().ToString() + " " + ds.Tables[0].Rows[i]["LastActivityBy"].ToString();

                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Review" && ds.Tables[0].Rows[i]["NextRole"].ToString() == "Delegate")
                    {
                        RemarkFormated = RemarkFormated + "\n Reviewer Submitted Recommendation\n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Review" && ds.Tables[0].Rows[i]["NextRole"].ToString() == "Close")
                    {
                        RemarkFormated = RemarkFormated + "\n Reviewer Verified and Task Completed \n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Review" && ds.Tables[0].Rows[i]["NextRole"].ToString() == "Review")
                    {
                        RemarkFormated = RemarkFormated + "\n Progress Updated by Reviewer \n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Doer")
                    {
                        if (R.DelegateToGalaxy == R.DoerNameGalaxy)
                        {
                            RemarkFormated = RemarkFormated + "\n Proposed Date Submitted \n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                        }
                        else {
                            RemarkFormated = RemarkFormated + "\n Proposed Date Submitted and Delegated\n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                        }
                    }

                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Delegate" && ds.Tables[0].Rows[i]["NextRole"].ToString() == "Delegate")
                    {
                        RemarkFormated = RemarkFormated + "\n Progress Updated by Doer\n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Delegate" && ds.Tables[0].Rows[i]["NextRole"].ToString() != "Delegate")
                    {
                    if (R.DoerStatus == "Completed" || R.DelegateStatus == "Completed")
                    {
                        RemarkFormated = RemarkFormated + "\n Task Completed by Doer \n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";                    
                    }
                    }


                    if (ds.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Initiator")
                    {
                        RemarkFormated = RemarkFormated + "\n New Task Initiate \n" + ds.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                }
            }
            return RemarkFormated;
        }

        public void ProjectTransfer(Request Req, int id)
        {
            con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("InsertTransferProject", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = id;
            cmd.Parameters.Add("@ExpDetailpurposedate", SqlDbType.VarChar).Value = Req.ExpDetailpurposedate;
            try
            {
                con.Open();

                cmd.Parameters.Add("@proposedDate", SqlDbType.DateTime).Value = Convert.ToDateTime(Req.ProposedDate.ToString());

                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Response.Write("<Script>alert('Record Not Updated')</script>");
            }
            finally
            {
                con.Close();
            }
            //return View(Req);
        }

        public void ProjectStatus(Request Req, int id)
        {
            con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("SPUpdate_Status", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = id;
            try
            {
                con.Open();

                cmd.Parameters.Add("@InitiatorStatus", SqlDbType.VarChar).Value = Req.InitiatorStatus;

                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Response.Write("<Script>alert('Record Not Updated')</script>");
            }
            finally
            {
                con.Close();
            }


        }

        public void ProjectStatusDoer(Request Req, int id)
        {
            con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("SPUpdate_Status_Doer", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@projectno", SqlDbType.VarChar).Value = id;
            try
            {
                con.Open();

                cmd.Parameters.Add("@DoerStatus", SqlDbType.VarChar).Value = Req.DoerStatus;

                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Response.Write("<Script>alert('Record Not Updated')</script>");
            }
            finally
            {
                con.Close();
            }


        }

        //public int SendEmail(string ReqNo, Request R , string status1)
        //{
        //    Email mail = new Email();
        //    SqlCommand cmdmail = new SqlCommand("GetEmailId", con);
        //    cmdmail.CommandType = CommandType.StoredProcedure;
        //    cmdmail.Parameters.Add("@ReqNo", ReqNo);
        //    SqlDataAdapter da1 = new SqlDataAdapter(cmdmail);
        //    DataSet ds1 = new DataSet();
        //    da1.Fill(ds1);
        //    string PendingEmailId = "", Names = "", GalaxtIds = "", ccemailId = "", ccGalaxy = "";

        //    if (ds1.Tables[0].Rows.Count > 0)
        //    {
        //        PendingEmailId = ds1.Tables[0].Rows[0]["EmailId"].ToString();
        //        Names = ds1.Tables[0].Rows[0]["pendingforName"].ToString();
        //        GalaxtIds = ds1.Tables[0].Rows[0]["pendingfor"].ToString();
        //        ccemailId = ds1.Tables[0].Rows[0]["CCEmail"].ToString();
        //       // ccGalaxy = ds1.Tables[0].Rows[0]["CCGalaxy"].ToString();
        //    }
        //    int ab = mail.email(Names, "", PendingEmailId, ccemailId, "", Session["GalaxyID"].ToString(), ReqNo, R, status1);
        //    return 1;
        //}
        public async Task<string> SendEmail(string ReqNo, Request R, string status1)
        {
            //return await Task.Run(() =>
            //{
                Email mail = new Email();
                SqlCommand cmdmail = new SqlCommand("GetEmailId", con);
                cmdmail.CommandType = CommandType.StoredProcedure;
                cmdmail.Parameters.Add("@ReqNo", ReqNo);
                SqlDataAdapter da1 = new SqlDataAdapter(cmdmail);
                DataSet ds1 = new DataSet();
                da1.Fill(ds1);
                string PendingEmailId = "", Names = "", GalaxtIds = "", ccemailId = "", ccGalaxy = "";

                if (ds1.Tables[0].Rows.Count > 0)
                {
                    PendingEmailId = ds1.Tables[0].Rows[0]["EmailId"].ToString();
                    Names = ds1.Tables[0].Rows[0]["pendingforName"].ToString();
                    GalaxtIds = ds1.Tables[0].Rows[0]["pendingfor"].ToString();
                    ccemailId = ds1.Tables[0].Rows[0]["CCEmail"].ToString();
                    // ccGalaxy = ds1.Tables[0].Rows[0]["CCGalaxy"].ToString();
                }
                mail.email(Names, "", PendingEmailId, ccemailId, "", Session["GalaxyID"].ToString(), ReqNo, R, status1);
                return "ok";
            //});
        }

        public List<ProjectList> GetRequestDetails(string submit, Request EM)
        {
            var AllList1 = new List<ProjectList>();
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = null;

            if (EM.status1 == "Open")
            {
                cmd = new SqlCommand("SPGetInitializerByDate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = EM.FromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = EM.ToDate;
                cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = EM.GalaxyID;
            }
            if (EM.status1 == "Completed")
            {
                cmd = new SqlCommand("SPGetInitializerCompliteRequestDate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = EM.FromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = EM.ToDate;
                cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = EM.GalaxyID;
                Session["FromDate"] = EM.FromDate;
                Session["ToDate"] = EM.ToDate;
                Session["Chkstatus"] = "Completed";
            }
            if (EM.status1 == "Cancel")
            {
                cmd = new SqlCommand("SPGetInitializerCancelRequestDate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = EM.FromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = EM.ToDate;
                cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = EM.GalaxyID;
                EM.FromDate = Session["FromDate"].ToString();
                EM.ToDate = Session["ToDate"].ToString();
                Session["Chkstatus"] = "Cancel";
            }
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var model = new ProjectList();
                model.ProjectNo = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectNo"].ToString());
                model.ProjectName = ds.Tables[0].Rows[i]["ProjectName"].ToString();
                model.ProjectDescription = ds.Tables[0].Rows[i]["ProjectDescription"].ToString();
                model.ExpectedOutput = ds.Tables[0].Rows[i]["ExpectedOutput"].ToString();
                model.AddedBy = ds.Tables[0].Rows[i]["AddedByName"].ToString();
                model.InitiatorStatus = ds.Tables[0].Rows[i]["InitilizerStatus"].ToString();
                model.DoerName1 = ds.Tables[0].Rows[i]["DoerByName"].ToString();
                model.DoerStatus = ds.Tables[0].Rows[i]["DoerStatus"].ToString();
                model.pendingfor = ds.Tables[0].Rows[i]["pendingfor"].ToString();
                model.Status = ds.Tables[0].Rows[i]["Status"].ToString();
                model.ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
                model.ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["proposeddate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["proposeddate"]).ToString("dd-MMM-yyyy");
                model.AgreedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[i]["AgreedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[i]["AgreedDate"]).ToString("dd-MMM-yyyy");
                AllList1.Add(model);
                TempData["AllList"] = AllList1;
                Session["AllList"] = TempData["AllList"];
            }
            return AllList1;
        }

        [HttpGet]
        public ActionResult Help(String Para1)
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            else
            {
                TempData["AlrtMessage1"] = "Session Expired";
                return RedirectToAction("Login", "Login");
            }
            Session["Role"] = GetRole(GalaxyID);
            TempData["Status"] = "Open";
            TempData["DisplayName"] = GetDisplayName(GalaxyID);
            Session["DisplayName"] = TempData["DisplayName"];
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            TempData["DisplayName"] = "Welcome " + GetDisplayName(GalaxyID);
            Request obj = new Request(); 
            obj.EmpCode = Session["Empcode"].ToString();
            obj.GalaxyID = GalaxyID;
            obj.GalaxyIDName = GetDisplayName(GalaxyID);
            TempData["FromGrid"] = "False";
            Session["GalaxyID"] = GalaxyID;
            return View("Help" ,obj);
        }

        public ActionResult DownloadAttachment(int id) 
        {
            string path1 = @"" + Server.MapPath("~/Uploads/" + id + "/") + "";
            string[] fileEntries = Directory.GetFiles(path1);
            string ActualfileName = "";
            foreach (string fileName in fileEntries)
            {
                string lastItemOfSplit = fileName.Split("\\".ToCharArray()).Last();
                if (lastItemOfSplit.StartsWith("Attachment_"))
                {
                    ActualfileName = lastItemOfSplit;
                    break;
                }
            }
            if (ActualfileName != "")
            {
                System.IO.FileStream fs1 = null;
                fs1 = System.IO.File.Open(Server.MapPath("~/Uploads/" + id + "/" + ActualfileName), System.IO.FileMode.Open);
                byte[] btFile = new byte[fs1.Length];
                fs1.Read(btFile, 0, Convert.ToInt32(fs1.Length));
                fs1.Close(); Response.AddHeader("Content-disposition", "attachment; filename=" + ActualfileName);
                Response.ContentType = "application/octet-stream";
                Response.BinaryWrite(btFile);
                Response.End();
            }

            return View("Request");
        }

        [HttpGet]
        public ActionResult LogOff()
        {
            Session["GalaxyID"] = null;
            //it's my session variable
            Session.Clear();
            Session.Abandon();
            System.Web.Security.FormsAuthentication.SignOut(); //you write this when you use FormsAuthentication
            return RedirectToAction("Login", "Login");
            //return Json(new { result = "Yes" }, JsonRequestBehavior.AllowGet);
        }

        public int UpdateTask(Request Req)
        {
            DateTime Date = Convert.ToDateTime(Req.ExpectedTargetDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("UpdateTaskDetails", con);
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.Add("@projectname", SqlDbType.VarChar).Value = Req.ProjectName;
            cmd.Parameters.Add("@projectdescription", SqlDbType.VarChar).Value = Req.ProjectDescription;
            cmd.Parameters.Add("@expectedoutput", SqlDbType.VarChar).Value = Req.ExpectedOutput1;         
            cmd.Parameters.Add("@expecteddate", SqlDbType.DateTime).Value = Date;
            cmd.Parameters.Add("@DoerName", SqlDbType.VarChar).Value = Req.DoerNameGalaxy.ToString();
            cmd.Parameters.Add("@ReviewBy", SqlDbType.VarChar).Value = Req.ReviewByGalaxy.ToString();
            cmd.Parameters.Add("@PrjNo", SqlDbType.VarChar).Value = Req.ProjectNo.ToString();
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            { 
            
            }
            finally
            {
                con.Close();
            }  
            return 1;
        }

        public int UpdateCancelTask(Request Req)
        {
            con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("UpdateCancelTask", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = 0;
            cmd.Parameters.Add("@PrjNo", SqlDbType.VarChar).Value = Req.ProjectNo.ToString();
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            finally
            {
                con.Close();
            }
            return 1;
        }

        public ActionResult GetDisplayNameAjax(string galaxy)
        {            
            var name="";
            if (!(string.IsNullOrEmpty(galaxy)))
            {
                SqlConnection con = ptraconn.GetItem();
                SqlCommand cmd = new SqlCommand("GetDisplayNameAjax", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Galaxy", SqlDbType.VarChar).Value = galaxy;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                name = ds.Tables[0].Rows[0]["DisplayName"].ToString();
            }
            return Json(name, JsonRequestBehavior.AllowGet);
        
        }
        public String GetRole(string GalaxyId)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetRole", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = GalaxyId;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string Role = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                Role = ds.Tables[0].Rows[0]["Role"].ToString();

            }

            return Role;
        }
        [HttpGet]
        public void ExportToTask()
        {
            string GalaxyID = "";
            if (Session["GalaxyID"] != null)
            {
                GalaxyID = Session["GalaxyID"].ToString();
            }
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("ExportToTask", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    GridView GridView1 = new GridView();
                    using (DataTable dt = new DataTable())
                    {
                        da.Fill(dt);
                    }
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        DataTable dt = new DataTable();
                        DataView dataset = new DataView(ds.Tables[0]);
                        DataView dataView = ds.Tables[0].DefaultView;
                        if (dataView.Count > 0)
                        {
                            GridView1.DataSource = dataView;
                            GridView1.DataBind();
                            dt = new DataTable("TaskDetails");
                            foreach (TableCell cell in GridView1.HeaderRow.Cells)
                            {

                                dt.Columns.Add(cell.Text);

                            }
                            foreach (GridViewRow row in GridView1.Rows)
                            {

                                dt.Rows.Add();

                                for (int j = 0; j < row.Cells.Count; j++)
                                {

                                    dt.Rows[dt.Rows.Count - 1][j] = row.Cells[j].Text;

                                }

                            }
                            wb.Worksheets.Add(dt);
                            Response.Clear();

                            Response.Buffer = true;

                            Response.Charset = "";

                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                            Response.AddHeader("content-disposition", "attachment;filename=TaskDetails.xlsx");

                            using (MemoryStream MyMemoryStream = new MemoryStream())
                            {

                                wb.SaveAs(MyMemoryStream);

                                MyMemoryStream.WriteTo(Response.OutputStream);

                                Response.Flush();

                                Response.End();

                            }
                        }

                    }
                }
               
            }
            catch (Exception ex)
            {

            }
            finally
            {
                con.Close();
            }

        }
    }

}


