using ClosedXML.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TaskManagment.Models;

namespace TaskManagment.Controllers
{
    public class EmployeeDetailsController : Controller
    {
        ProjectTrackConnection ptraconn = new ProjectTrackConnection();
        Decript decript = new Decript();

        // GET: Employee
        public ActionResult EmployeeList()
        {
            Employee model = new Employee();
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            Session["Role"] = GetRole(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);

            model.EmpCode = Session["Empcode"].ToString();
            model.GalaxyID = GalaxyID;
            model.GalaxyIDName = GetDisplayName(GalaxyID);
            model.EmployeeList = GetEmployeeList();
            return View(model);
        }
        [HttpGet]
        public ActionResult Admin()
        {
            Employee model = new Employee();
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);

            return View(model);
        }
        public ActionResult Create()
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            ViewBag.Page = "Create";
            Employee model = new Employee();
            model.EmpList = GetEmpList();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(Employee model)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
                model.Password = decript.Encrypt(model.EmpCode);
                SqlCommand cmd = new SqlCommand("InsertUpdateEmployeeInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Prefix", SqlDbType.VarChar).Value = model.Prefix;
                cmd.Parameters.Add("@FName", SqlDbType.VarChar).Value = model.FName;
                cmd.Parameters.Add("@MName", SqlDbType.VarChar).Value = model.MName;
                cmd.Parameters.Add("@LName", SqlDbType.VarChar).Value = model.LName;
                cmd.Parameters.Add("@DisplayName", SqlDbType.VarChar).Value = model.DisplayName;
                cmd.Parameters.Add("@Department", SqlDbType.VarChar).Value = model.Department;
                cmd.Parameters.Add("@Mobile", SqlDbType.VarChar).Value = model.Mobile;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = model.EmpCode;
                cmd.Parameters.Add("@ReptTo", SqlDbType.VarChar).Value = model.ReptTo;
                cmd.Parameters.Add("@Designation", SqlDbType.VarChar).Value = model.Designation;
                cmd.Parameters.Add("@Sex", SqlDbType.VarChar).Value = model.Sex;
                cmd.Parameters.Add("@EMail", SqlDbType.VarChar).Value = model.EMail;
                cmd.Parameters.Add("@Location", SqlDbType.VarChar).Value = model.Location;
                cmd.Parameters.Add("@Company", SqlDbType.VarChar).Value = model.Company;
                cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = model.EmpCode;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = model.Password;
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return RedirectToAction("EmployeeList", "EmployeeDetails");
            }
            catch
            {
                return View();
            }
        }
        //Edit
        [HttpGet]
        public ActionResult Edit(string EmpCode)
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            Employee model = new Employee();
            SqlConnection con = ptraconn.GetItem();

            try
            {
                SqlCommand cmd = new SqlCommand("GetEmployeeDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = EmpCode;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    model.EmpId = Convert.ToInt16(ds.Tables[0].Rows[0]["EmpId"].ToString());
                    model.EmpCode = ds.Tables[0].Rows[0]["EmpCode"].ToString();
                    model.Prefix = ds.Tables[0].Rows[0]["Prefix"].ToString();
                    model.FName = ds.Tables[0].Rows[0]["FName"].ToString();
                    model.MName = ds.Tables[0].Rows[0]["MName"].ToString();
                    model.LName = ds.Tables[0].Rows[0]["LName"].ToString();
                    model.DisplayName = ds.Tables[0].Rows[0]["DisplayName"].ToString();
                    model.JoinDate = ds.Tables[0].Rows[0]["JoinDate"].ToString();
                    if (model.JoinDate != "")
                    {
                        DateTime Date = Convert.ToDateTime(model.JoinDate);
                        model.JoinDate = Date.ToString("dd-MM-yyyy");
                    }
                    model.BirthDate = ds.Tables[0].Rows[0]["BirthDate"].ToString();
                    if (model.BirthDate != "")
                    {
                        DateTime Date = Convert.ToDateTime(model.BirthDate);
                        model.BirthDate = Date.ToString("dd-MM-yyyy");
                    }
                    model.ConfirmDate = ds.Tables[0].Rows[0]["ConfirmDate"].ToString();
                    if (model.ConfirmDate != "")
                    {
                        DateTime Date = Convert.ToDateTime(model.ConfirmDate);
                        model.ConfirmDate = Date.ToString("dd-MM-yyyy");
                    }
                    model.LeftDate = ds.Tables[0].Rows[0]["LeftDate"].ToString();
                    if (model.LeftDate != "")
                    {
                        DateTime Date = Convert.ToDateTime(model.LeftDate);
                        model.LeftDate = Date.ToString("dd-MM-yyyy");
                    }
                    else
                    {
                        model.LeftDate = "-";
                    }
                    model.Sex = ds.Tables[0].Rows[0]["Sex"].ToString().Trim();
                    model.EMail = ds.Tables[0].Rows[0]["EMail"].ToString();
                    model.FatherName = ds.Tables[0].Rows[0]["FatherName"].ToString();
                    model.PreAdd1 = ds.Tables[0].Rows[0]["PreAdd1"].ToString();
                    model.PreAdd2 = ds.Tables[0].Rows[0]["PreAdd2"].ToString();
                    model.PreCity = ds.Tables[0].Rows[0]["PreCity"].ToString();
                    model.PrePin = ds.Tables[0].Rows[0]["PrePin"].ToString();
                    model.PreState = ds.Tables[0].Rows[0]["PreState"].ToString();
                    model.PreTel = ds.Tables[0].Rows[0]["PreTel"].ToString();
                    model.Mobile = ds.Tables[0].Rows[0]["Mobile"].ToString();
                    if (model.Mobile == "")
                    {

                        model.Mobile = "-";
                    }
                    model.PerAdd1 = ds.Tables[0].Rows[0]["PerAdd1"].ToString();
                    model.PerAdd2 = ds.Tables[0].Rows[0]["PerAdd2"].ToString();
                    model.PerCity = ds.Tables[0].Rows[0]["PerCity"].ToString();
                    string PerPin = ds.Tables[0].Rows[0]["PerPin"].ToString();
                    if (PerPin != "")
                    {
                        model.PerPin = Convert.ToInt16(ds.Tables[0].Rows[0]["PerPin"].ToString());
                    }
                    model.PerState = ds.Tables[0].Rows[0]["PerState"].ToString();
                    model.PerTel = ds.Tables[0].Rows[0]["PerTel"].ToString();

                    model.Location = ds.Tables[0].Rows[0]["Location"].ToString();
                    model.Department = ds.Tables[0].Rows[0]["Department"].ToString();
                    model.Designation = ds.Tables[0].Rows[0]["Designation"].ToString();

                    model.Plant = ds.Tables[0].Rows[0]["Plant"].ToString();

                    model.Gender = ds.Tables[0].Rows[0]["Gender"].ToString();
                    model.Grade = ds.Tables[0].Rows[0]["Grade"].ToString();
                    model.ReptTo = ds.Tables[0].Rows[0]["ReptTo"].ToString();
                    model.GalaxyID = ds.Tables[0].Rows[0]["GalaxyId"].ToString();
                    model.Manager = GetDisplayName(model.ReptTo);
                    model.IsDisable = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsDisable"].ToString());
                    model.EmpList = GetEmpList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            ViewBag.Page = "Edit";
            return View("Create", model);
        }
        [HttpPost]
        public List<SelectListItem> GetEmpList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SqlConnection con = ptraconn.GetItem();
            try
            {

                SqlCommand cmd = new SqlCommand("GetEmployeeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
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
                            Text = @dr["DisplayName"].ToString() + " (" + @dr["EmpCode"].ToString() + ")",
                            Value = @dr["EmpCode"].ToString()
                        };
                        list.Add(s1);
                    }
                    SelectListItem s2 = new SelectListItem
                    {
                        Text = "NULL",
                        Value = "NULL"
                    };
                    list.Add(s2);
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
        public List<Employee> GetEmployeeList()
        {
            Employee model1 = new Employee();
            List<Employee> list = new List<Employee>();
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand("GetEmployeeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        Employee model = new Employee();
                        model.EmpId = Convert.ToInt16(ds.Tables[0].Rows[i]["EmpId"].ToString());
                        model.EmpCode = ds.Tables[0].Rows[i]["EmpCode"].ToString();
                        model.Prefix = ds.Tables[0].Rows[i]["Prefix"].ToString();
                        model.FName = ds.Tables[0].Rows[i]["FName"].ToString();
                        model.MName = ds.Tables[0].Rows[i]["MName"].ToString();
                        model.LName = ds.Tables[0].Rows[i]["LName"].ToString();
                        model.DisplayName = ds.Tables[0].Rows[i]["DisplayName"].ToString();
                        model.JoinDate = ds.Tables[0].Rows[i]["JoinDate"].ToString();
                        if (model.JoinDate != "")
                        {
                            DateTime Date = Convert.ToDateTime(model.JoinDate);
                            model.JoinDate = Date.ToString("dd-MM-yyyy");
                        }
                        model.BirthDate = ds.Tables[0].Rows[i]["BirthDate"].ToString();
                        if (model.BirthDate != "")
                        {
                            DateTime Date = Convert.ToDateTime(model.BirthDate);
                            model.BirthDate = Date.ToString("dd-MM-yyyy");
                        }
                        model.ConfirmDate = ds.Tables[0].Rows[i]["ConfirmDate"].ToString();
                        if (model.ConfirmDate != "")
                        {
                            DateTime Date = Convert.ToDateTime(model.ConfirmDate);
                            model.ConfirmDate = Date.ToString("dd-MM-yyyy");
                        }
                        model.LeftDate = ds.Tables[0].Rows[i]["LeftDate"].ToString();
                        if (model.LeftDate != "")
                        {
                            DateTime Date = Convert.ToDateTime(model.LeftDate);
                            model.LeftDate = Date.ToString("dd-MM-yyyy");
                        }
                        else
                        {
                            model.LeftDate = "-";
                        }

                        model.EMail = ds.Tables[0].Rows[i]["EMail"].ToString();
                        model.FatherName = ds.Tables[0].Rows[i]["FatherName"].ToString();
                        model.PreAdd1 = ds.Tables[0].Rows[i]["PreAdd1"].ToString();
                        model.PreAdd2 = ds.Tables[0].Rows[i]["PreAdd2"].ToString();
                        model.PreCity = ds.Tables[0].Rows[i]["PreCity"].ToString();
                        model.PrePin = ds.Tables[0].Rows[i]["PrePin"].ToString();
                        model.PreState = ds.Tables[0].Rows[i]["PreState"].ToString();
                        model.PreTel = ds.Tables[0].Rows[i]["PreTel"].ToString();
                        model.Mobile = ds.Tables[0].Rows[i]["Mobile"].ToString();
                        if (model.Mobile == "")
                        {

                            model.Mobile = "-";
                        }
                        model.PerAdd1 = ds.Tables[0].Rows[i]["PerAdd1"].ToString();
                        model.PerAdd2 = ds.Tables[0].Rows[i]["PerAdd2"].ToString();
                        model.PerCity = ds.Tables[0].Rows[i]["PerCity"].ToString();
                        string PerPin = ds.Tables[0].Rows[i]["PerPin"].ToString();
                        if (PerPin != "")
                        {
                            model.PerPin = Convert.ToInt16(ds.Tables[0].Rows[i]["PerPin"].ToString());
                        }
                        model.PerState = ds.Tables[0].Rows[i]["PerState"].ToString();
                        model.PerTel = ds.Tables[0].Rows[i]["PerTel"].ToString();

                        model.Location = ds.Tables[0].Rows[i]["Location"].ToString();
                        model.Department = ds.Tables[0].Rows[i]["Department"].ToString();
                        model.Designation = ds.Tables[0].Rows[i]["Designation"].ToString();

                        model.Plant = ds.Tables[0].Rows[i]["Plant"].ToString();

                        model.Gender = ds.Tables[0].Rows[i]["Gender"].ToString();
                        model.Grade = ds.Tables[0].Rows[i]["Grade"].ToString();
                        model.ReptTo = ds.Tables[0].Rows[i]["ReptTo"].ToString();
                        model.GalaxyID = ds.Tables[0].Rows[i]["GalaxyId"].ToString();
                        model.Manager = GetDisplayName(model.ReptTo);
                        model.IsDisable = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsDisable"].ToString());
                        list.Add(model);
                    }
                }
                var data = list.ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                con.Close();
            }

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
        [HttpPost]
        public JsonResult CheckEmpCodeExists(string EmpCode)
        {
            try
            {
                var isEmpCodeExists = false;

                if (EmpCode != null)
                {
                    isEmpCodeExists = EmpCodeExists(EmpCode);
                }

                if (isEmpCodeExists)
                {
                    return Json(data: true);
                }
                else
                {
                    return Json(data: false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool EmpCodeExists(string EmpCode)
        {
            var isExists = false;
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("CheckEmpCodeExists", con);
            con.Open();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = EmpCode;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                isExists = true;
            }
            return isExists;
        }
        [HttpGet]
        public ActionResult Upload()
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            Employee model = new Employee();
            return View(model);
        }
        [HttpPost]
        public ActionResult Upload(Employee model, HttpPostedFileBase file)
        {
            string Path1 = "";
            if (file != null)
            {
                try
                {
                    ISheet sheet;
                    DataFormatter formatter = new DataFormatter();
                    string filename = System.IO.Path.GetFileName(file.FileName);
                    var fileExt = Path.GetExtension(filename);


                    HSSFWorkbook hssfwb;
                    XSSFWorkbook hssfwb1 = null;
                    if (fileExt == ".xls")
                    {
                        hssfwb = new HSSFWorkbook(file.InputStream);
                        sheet = hssfwb.GetSheetAt(0);
                    }
                    else
                    {
                        hssfwb1 = new XSSFWorkbook(file.InputStream);
                        sheet = hssfwb1.GetSheetAt(0);
                    }
                    int row = 1;
                    var ExList1 = new List<Employee>();
                    String sDate = DateTime.Now.ToString();
                    DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));
                    try
                    {
                        for (int k = 0; k < hssfwb1.NumberOfSheets; k++)
                        {
                            sheet = hssfwb1.GetSheetAt(k);
                            for (int i = 1; i <= sheet.LastRowNum; i++)
                            {
                                if (!string.IsNullOrEmpty(formatter.FormatCellValue(sheet.GetRow(i).GetCell(0))))
                                {

                                    var det1 = new Employee();
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(0)) != "")
                                    {
                                        det1.Prefix = formatter.FormatCellValue(sheet.GetRow(i).GetCell(0)).Trim();
                                    }
                                    else
                                    {
                                        det1.Prefix = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(1)) != "")
                                    {
                                        det1.FName = formatter.FormatCellValue(sheet.GetRow(i).GetCell(1)).Trim();
                                    }
                                    else
                                    {
                                        det1.FName = "";
                                    }

                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(2)) != "")
                                    {
                                        det1.MName = formatter.FormatCellValue(sheet.GetRow(i).GetCell(2)).Trim();
                                    }
                                    else
                                    {
                                        det1.MName = "";
                                    }

                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(3)) != "")
                                    {
                                        det1.LName = formatter.FormatCellValue(sheet.GetRow(i).GetCell(3)).Trim();
                                    }
                                    else
                                    {
                                        det1.LName = "";
                                    }

                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(4)) != "")
                                    {
                                        det1.DisplayName = formatter.FormatCellValue(sheet.GetRow(i).GetCell(4)).Trim();
                                    }
                                    else
                                    {
                                        det1.DisplayName = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(5)) != "")
                                    {
                                        det1.Department = formatter.FormatCellValue(sheet.GetRow(i).GetCell(5)).Trim();
                                    }
                                    else
                                    {
                                        det1.Department = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(6)) != "")
                                    {
                                        det1.Mobile = formatter.FormatCellValue(sheet.GetRow(i).GetCell(6)).Trim();
                                    }
                                    else
                                    {
                                        det1.Mobile = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(7)) != "")
                                    {
                                        det1.EmpCode = formatter.FormatCellValue(sheet.GetRow(i).GetCell(7)).Trim();
                                    }
                                    else
                                    {
                                        det1.EmpCode = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(8)) != "")
                                    {
                                        det1.ReptTo = formatter.FormatCellValue(sheet.GetRow(i).GetCell(8)).Trim();
                                    }
                                    else
                                    {
                                        det1.ReptTo = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(9)) != "")
                                    {
                                        det1.Designation = formatter.FormatCellValue(sheet.GetRow(i).GetCell(9)).Trim();
                                    }
                                    else
                                    {
                                        det1.Designation = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(10)) != "")
                                    {
                                        det1.Sex = formatter.FormatCellValue(sheet.GetRow(i).GetCell(10)).Trim();
                                    }
                                    else
                                    {
                                        det1.Sex = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(11)) != "")
                                    {
                                        det1.EMail = formatter.FormatCellValue(sheet.GetRow(i).GetCell(11)).Trim();
                                    }
                                    else
                                    {
                                        det1.EMail = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(12)) != "")
                                    {
                                        det1.Location = formatter.FormatCellValue(sheet.GetRow(i).GetCell(12)).Trim();
                                    }
                                    else
                                    {
                                        det1.Location = "";
                                    }
                                    if (formatter.FormatCellValue(sheet.GetRow(i).GetCell(13)) != "")
                                    {
                                        det1.Company = formatter.FormatCellValue(sheet.GetRow(i).GetCell(13)).Trim();
                                    }
                                    else
                                    {
                                        det1.Company = "";
                                    }
                                    ExList1.Add(det1);

                                }

                            }
                        }
                        if (ExList1.Count > 0)
                        {
                            InsertEmployeeDetails(ExList1);
                            TempData["UploadMsg"] = "No.Of Row Uploaded:"+ ExList1.Count;
                        }
                    }
                    catch (Exception e)
                    {
                        TempData["UploadMsg"] = "No.Of Row Uploaded: 0";
                    }
                    finally
                    {
                    }
                }
                catch (Exception ex)
                {
                    TempData["UploadMsg"] = "No.Of Row Uploaded: 0";
                }
            }
            else
            {
                TempData["UploadMsg"] = "Please select valid file to upload !!";
                return View(model);
            }
            return RedirectToAction("EmployeeList", "EmployeeDetails", new { });
        }
        public int InsertEmployeeDetails(List<Employee> SM)
        {

            SqlConnection con = ptraconn.GetItem();
            for (int j = 0; j < SM.Count; j++)
            {
                SM[j].Password = decript.Encrypt(SM[j].EmpCode);

                try
                {
                    con.Open();
                    SqlCommand cmd = null;
                    cmd = new SqlCommand("InsertUpdateEmployeeInfo", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Prefix", SqlDbType.VarChar).Value = SM[j].Prefix;
                    cmd.Parameters.Add("@FName", SqlDbType.VarChar).Value = SM[j].FName;
                    cmd.Parameters.Add("@MName", SqlDbType.VarChar).Value = SM[j].MName;
                    cmd.Parameters.Add("@LName", SqlDbType.VarChar).Value = SM[j].LName;
                    cmd.Parameters.Add("@DisplayName", SqlDbType.VarChar).Value = SM[j].DisplayName;
                    cmd.Parameters.Add("@Department", SqlDbType.VarChar).Value = SM[j].Department;
                    cmd.Parameters.Add("@Mobile", SqlDbType.VarChar).Value = SM[j].Mobile;
                    cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = SM[j].EmpCode;
                    cmd.Parameters.Add("@ReptTo", SqlDbType.VarChar).Value = SM[j].ReptTo;
                    cmd.Parameters.Add("@Designation", SqlDbType.VarChar).Value = SM[j].Designation;
                    cmd.Parameters.Add("@Sex", SqlDbType.VarChar).Value = SM[j].Sex;
                    cmd.Parameters.Add("@EMail", SqlDbType.VarChar).Value = SM[j].EMail;
                    cmd.Parameters.Add("@Location", SqlDbType.VarChar).Value = SM[j].Location;
                    cmd.Parameters.Add("@Company", SqlDbType.VarChar).Value = SM[j].Company;
                    cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = SM[j].EmpCode;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = SM[j].Password;
                    cmd.Parameters.Add("@IsDisable", SqlDbType.Bit).Value = SM[j].IsDisable;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception", e.Message);

                }
                finally
                {
                    con.Close();
                }
            }
            return 1;

        }
        public ActionResult DownloadTemplate()
        {
            try
            {
                string ActualfileName = "EmployeeDetails.xlsx";
                System.IO.FileStream fs1 = null;
                fs1 = System.IO.File.Open(Server.MapPath("~/Template/EmployeeDetails.xlsx"), System.IO.FileMode.Open);
                byte[] btFile = new byte[fs1.Length];
                fs1.Read(btFile, 0, Convert.ToInt32(fs1.Length));
                fs1.Close(); Response.AddHeader("Content-disposition", "attachment; filename=" + ActualfileName);
                Response.ContentType = "application/octet-stream";
                Response.BinaryWrite(btFile);
                Response.End();

            }
            catch (Exception e)
            {
                string error = e.Message;
            }
            return View();
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
        public ActionResult Delete(string EmpCode)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand("DeleteEmployee", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = EmpCode;
                try
                {
                    con.Open();
                    int Result = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return RedirectToAction("EmployeeList", "EmployeeDetails", new { });
        }
        [HttpGet]
        public ActionResult EmployeeDetails(string Back)
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
            if (Back == "Back")
            {
                if (Session["Role"].ToString() == "Admin")
                {
                    return RedirectToAction("EmployeeList", "EmployeeDetails");
                }
                else
                {
                    return RedirectToAction("ViewProjects", "Initiator");
                }
            }
            else if(Back == "AdminBack")
            {
                if (Session["Role"].ToString() == "Admin")
                {
                    return RedirectToAction("Admin", "EmployeeDetails");
                }
                else
                {
                    return RedirectToAction("ViewProjects", "Initiator");
                }
            }
            return View();
        }
        [HttpGet]
        public void ExportToExcel()
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("EmployeeDetailsExportToExcel", con);
                cmd.CommandType = CommandType.StoredProcedure;
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
                            dt = new DataTable("EmployeeDetails");
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

                            Response.AddHeader("content-disposition", "attachment;filename=EmployeeDetails.xlsx");

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
        [HttpGet]
        public ActionResult SendPassword(string EmpCode, string Email)
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
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sRandomOTP = GenerateRandomOTP(6, saAllowedCharacters);
            int update = UpdateOTP(sRandomOTP, EmpCode);
            bool IsEmail = SendOTPMail(sRandomOTP, Email);
            return RedirectToAction("EmployeeList", "EmployeeDetails");
        }
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)

        {

            string sOTP = String.Empty;

            string sTempChars = String.Empty;

            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)

            {

                int p = rand.Next(0, saAllowedCharacters.Length);

                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];

                sOTP += sTempChars;

            }

            return sOTP;

        }
        public int UpdateOTP(string sRandomOTP, string EmpCode)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
                sRandomOTP = decript.Encrypt(sRandomOTP);
                con.Open();
                SqlCommand cmd = null;
                cmd = new SqlCommand("UpdateOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = EmpCode;
                cmd.Parameters.Add("@OTP", SqlDbType.VarChar).Value = sRandomOTP;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception", e.Message);
            }
            finally
            {
                con.Close();
            }
            return 1;
        }
        private bool SendOTPMail(string sRandomOTP, string Email)
        {
            bool IsSend = false;

            Email objCEmail = new Email();
            string recieverccemailid = "";
            string reciverEmailID = "";
            string strsubject = "";
            string strbody = "";
            string senderemailid = "";

            reciverEmailID = Email;

            if (reciverEmailID != null)
            {
                senderemailid = "";
                strsubject = "One Time Password Confirmation";

                strbody = "";
                strbody += "<p>Dear,</p>";

                strbody += "<p>Your One Time Password is <b>" + sRandomOTP + "</b>.Use this password to login to application.</p>";
                strbody += "<p>On successful login please get your password changed.</p>";

                strbody += "<p>Thank You</p>";
                strbody += "";
                strbody += "<p>IT Team</p>";
                strbody += "";
                strbody += "<p>This is auto generated email. Do not reply.</p>";
                string img = "";
                IsSend = objCEmail.emailWithCC(strsubject, strbody, senderemailid, reciverEmailID, recieverccemailid, img);
            }


            return IsSend;
        }
        [HttpGet]
        public void ExportToTask()
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("TaskDetailsExportToExcel", con);
                cmd.CommandType = CommandType.StoredProcedure;
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
        [HttpGet]
        public ActionResult ChangePassword()
        {
            Employee model = new Employee();
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            Session["Role"] = GetRole(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            model.EmpCode = Session["Empcode"].ToString();
            model.GalaxyID = GalaxyID;
            model.GalaxyIDName = GetDisplayName(GalaxyID);
            return View(model);
        }
        [HttpPost]
        public ActionResult ChangePassword(Employee model)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
                model.NewPassword = decript.Encrypt(model.NewPassword);
                model.OldPassword = decript.Encrypt(model.OldPassword);
                string storedPassword = GetPasswordbyEmpCode(model.EmpCode);
                if (storedPassword == model.OldPassword)
                {
                    SqlCommand cmd = new SqlCommand("ChangePassword", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NewPassword", SqlDbType.VarChar).Value = model.NewPassword;
                    cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = model.EmpCode;

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                else
                {
                    ViewBag.message = "Entered Wrong Old Password";
                    return View();
                }
                return RedirectToAction("EmployeeList", "EmployeeDetails");
            }
            catch
            {
                return View();
            }
        }
        public string GetPasswordbyEmpCode(string EmpCode)
        {
            string Password = "";
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("GetPasswordbyEmpCode", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = EmpCode;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Password = ds.Tables[0].Rows[0]["Password"].ToString();
            }
            return Password;
        }
        [HttpGet]
        public ActionResult EmailConfiguration()
        {
            Employee model = new Employee();
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            Session["Role"] = GetRole(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            model.EmpCode = Session["Empcode"].ToString();
            model.GalaxyID = GalaxyID;
            model.GalaxyIDName = GetDisplayName(GalaxyID);
            SqlConnection con = ptraconn.GetItem();

            try
            {
                SqlCommand cmd = new SqlCommand("GetEmailConfiguration", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    model.ID = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString());
                    model.EmailUserName = ds.Tables[0].Rows[0]["UserName"].ToString();
                    model.Password = ds.Tables[0].Rows[0]["Password"].ToString();
                    model.Port = ds.Tables[0].Rows[0]["Port"].ToString();
                    model.SmtpUrl = ds.Tables[0].Rows[0]["SmtpUrl"].ToString();

                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EmailConfiguration(Employee model)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {

                SqlCommand cmd = new SqlCommand("UpdateEmailConfiguration", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = model.ID;
                cmd.Parameters.Add("@EmailUserName", SqlDbType.VarChar).Value = model.EmailUserName;
                cmd.Parameters.Add("@EmailPassword", SqlDbType.VarChar).Value = model.EmailPassword;
                cmd.Parameters.Add("@Port", SqlDbType.VarChar).Value = model.Port;
                cmd.Parameters.Add("@SmtpUrl", SqlDbType.VarChar).Value = model.SmtpUrl;
                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = model.EmpCode;

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }

                return RedirectToAction("Admin", "EmployeeDetails");
            }
            catch
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult UserManual()
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
            Employee obj = new Employee();
            obj.EmpCode = Session["Empcode"].ToString();
            obj.GalaxyID = GalaxyID;
            obj.GalaxyIDName = GetDisplayName(GalaxyID);
            TempData["FromGrid"] = "False";
            Session["GalaxyID"] = GalaxyID;
            return View("Manual", obj);
        }
        [HttpGet]
        public ActionResult AdminManual()
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
            Employee obj = new Employee();
            obj.EmpCode = Session["Empcode"].ToString();
            obj.GalaxyID = GalaxyID;
            obj.GalaxyIDName = GetDisplayName(GalaxyID);
            TempData["FromGrid"] = "False";
            Session["GalaxyID"] = GalaxyID;
            return View("Manual", obj);
        }
        [HttpGet]
        public ActionResult EmailScheduler()
        {
            Employee model = new Employee();
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
            Session["Empcode"] = GetLoginEmpCode(GalaxyID);
            Session["Role"] = GetRole(GalaxyID);
            TempData["LoginEmpCode"] = Session["LoginEmpCode"];
            Session["LoginEmpName"] = GetDisplayName(GalaxyID);
            model.EmpCode = Session["Empcode"].ToString();
            model.GalaxyID = GalaxyID;
            model.GalaxyIDName = GetDisplayName(GalaxyID);
            model.ApplicationList = GetApplicationList();
            return View(model);
        }
        [HttpPost]
        public ActionResult EmailScheduler(Employee model)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {

                SqlCommand cmd = new SqlCommand("UpdateEmailScheduler", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Application", SqlDbType.VarChar).Value = model.Application;
                cmd.Parameters.Add("@FirstReminder", SqlDbType.VarChar).Value = model.FirstReminder;
                cmd.Parameters.Add("@SecondReminder", SqlDbType.VarChar).Value = model.SecondReminder;
                cmd.Parameters.Add("@RepeatReminder", SqlDbType.VarChar).Value = model.RepeatReminder;
                cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = model.EmpCode;
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }

                return RedirectToAction("Admin", "EmployeeDetails");
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public List<SelectListItem> GetApplicationList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SqlConnection con = ptraconn.GetItem();
            try
            {

                SqlCommand cmd = new SqlCommand("GetApplicationList", con);
                cmd.CommandType = CommandType.StoredProcedure;
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
                            Text = @dr["Application"].ToString(),
                            Value = @dr["ApplicationID"].ToString()
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
        public JsonResult GetSchedulerDetails(string Application)
        {
            SqlConnection con = ptraconn.GetItem();
            Employee model = new Employee();
            try
            {
                SqlCommand cmd = new SqlCommand("GetSchedulerDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Application", SqlDbType.VarChar).Value = Application;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    
                    model.FirstReminder = ds.Tables[0].Rows[0]["FirstReminder"].ToString();
                    model.SecondReminder = ds.Tables[0].Rows[0]["SecondReminder"].ToString();
                    model.RepeatReminder = ds.Tables[0].Rows[0]["RepeatReminder"].ToString();
                    
                }
                return Json(data: model);
               
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}