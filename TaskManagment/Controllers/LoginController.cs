using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using TaskManagment.Models;
using TaskManagment.Controllers;
using System.IO;
using System.Web.Script.Serialization;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;
using System.Data.OleDb;
using System.Runtime.InteropServices;

namespace TaskManagment.Controllers
{
    public class LoginController : Controller
    {
        
        ProjectTrackConnection ptraconn = new ProjectTrackConnection();
        Decript decript = new Decript();
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.AlertMessage = "";
            if (TempData["AlrtMessage1"] != null)
            {
                TempData["AlrtMessage"] = TempData["AlrtMessage1"].ToString();
                ViewBag.AlertMessage = TempData["AlrtMessage1"].ToString();
                return View("SessionTimeOut");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(string GalaxyID ,Request R)
        {
            
            
            string ToDate = "01-03-2021";

            DateTime TDate = Convert.ToDateTime(ToDate);
            TDate = TDate.AddMonths(2);
            string TDate1 = TDate.ToString("dd-MM-yyyy");

            DateTime BeforeExpiredOneMonthDate = Convert.ToDateTime(ToDate);
            BeforeExpiredOneMonthDate = BeforeExpiredOneMonthDate.AddMonths(-2);
            string BeforeExpiredOneMonthDate1 = BeforeExpiredOneMonthDate.ToString("dd-MM-yyyy");
           
            DateTime BeforeExpiredFifteenDaysDate = Convert.ToDateTime(BeforeExpiredOneMonthDate);
            BeforeExpiredFifteenDaysDate = BeforeExpiredFifteenDaysDate.AddDays(14);
            string BeforeExpiredFifteenDaysDate1 = BeforeExpiredFifteenDaysDate.ToString("dd-MM-yyyy");

            DateTime BeforeExpiredSevenDaysDate = Convert.ToDateTime(BeforeExpiredFifteenDaysDate);
            BeforeExpiredSevenDaysDate = BeforeExpiredSevenDaysDate.AddDays(10);
            string BeforeExpiredSevenDaysDate1 = BeforeExpiredSevenDaysDate.ToString("dd-MM-yyyy");

            DateTime BeforeExpiredsixDaysDate = Convert.ToDateTime(BeforeExpiredSevenDaysDate);
            BeforeExpiredsixDaysDate = BeforeExpiredsixDaysDate.AddDays(6);
            string BeforeExpiredsixDaysDate1 = BeforeExpiredsixDaysDate.ToString("dd-MM-yyyy");

            DateTime Date = Convert.ToDateTime(DateTime.Now, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            string DateNow= Date.ToString("dd-MM-yyyy");
            if((BeforeExpiredOneMonthDate1== DateNow)||(BeforeExpiredFifteenDaysDate1== DateNow)||(BeforeExpiredSevenDaysDate1== DateNow) || (BeforeExpiredsixDaysDate1 == DateNow))
            {
                bool Ischeck = checkDate();
                if (Ischeck == false)
                {
                    UpdateSubsEmailDate();
                    SendMail("Before");
                }
            }
            string systemDate =Date.ToString("ddMMyyyy");
            int currentDateValues = Convert.ToInt32(systemDate);
            int inputDateValues = Convert.ToInt32(ToDate.Replace("-",""));
            if (inputDateValues < currentDateValues)
            {
                
                if (R.GalaxyID != null && R.Password != null)
                {
                    R.Password = decript.Encrypt(R.Password);
                    Session["GalaxyID"] = GalaxyID;
                    Session["Role"]= GetRole(GalaxyID);
                    Session["pendingfor"] = "";
                    SqlConnection con = ptraconn.GetItem();
                    SqlCommand cmd = new SqlCommand("UserPresentorNot", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = R.Password;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {

                        string str1 = GetDisplayName(GalaxyID);
                        if (str1 != "")
                        {
                            
                            R.DisplayName = "Welcome " + str1;
                            Session["DisplayName"] = "Welcome " + str1;
                            Session["LoginEmpName"] = str1;
                            string IPAddress = GetIp();
                            Updateinfo(IPAddress, GalaxyID);
                        }
                        string Role = GetRole(GalaxyID);
                        if(Role=="Admin")
                        {
                            return RedirectToAction("Admin", "EmployeeDetails");
                        }
                        else
                        {
                            return RedirectToAction("ViewProjects", "Initiator");
                        }
                        
                    }
                    else
                    {
                        ViewBag.errormessage = "UserId not exists.";
                        ModelState.Clear();
                        return View();
                    }
                }
                else
                {
                    ViewBag.errormessage = "Please enter valid userId and password !!";
                    ModelState.Clear();
                    return View();
                }
            }
            else
            {
                bool Ischeck = checkDate();
                if (Ischeck == false)
                {
                    UpdateSubsEmailDate();
                    SendMail("After");
                }
                ViewBag.errormessage = "Your subscription period is expired,please contact your IT Department";
            }
            return View();
        }
        public String GetDisplayName(string GalaxyId)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("DisplayName", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GalaxyId", SqlDbType.VarChar).Value = GalaxyId;
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
        public string GetIp()
        {
            IPHostEntry host;
            string localip = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localip = ip.ToString();
                }
            }
            return localip;
        }
        public int Updateinfo(string IPAddress, string GalaxyID)
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {
              
                SqlCommand cmd = null;
                cmd = new SqlCommand("UpdateInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                cmd.Parameters.Add("@IPAddress", SqlDbType.VarChar).Value = IPAddress;
                cmd.Parameters.Add("@GalaxyID", SqlDbType.VarChar).Value = GalaxyID;
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
        private bool SendMail(string Type)
        {
            bool IsSend = false;

            Email objCEmail = new Email();
            string recieverccemailid = "datta@tasmay.in";
            string reciverEmailID =GetAdminMail();
            string strsubject = "";
            string strbody = "";
            string senderemailid = "";
            if (reciverEmailID != null)
            {
                if(Type== "Before")
                { 
                senderemailid = "";
                strsubject = "Subscription Renewal Alert";

                strbody = "";
                strbody += "<p>Dear Admin,</p>";

                strbody += "<p>Your subscription is getting over soon,Please contact your IT vendor for renewal. </p>";
                strbody += "<p>Thank You</p>";
                }
                else
                {
                    senderemailid = "";
                    strsubject = "Subscription Expired Alert";

                    strbody = "";
                    strbody += "<p>Dear Admin,</p>";

                    strbody += "<p>Your subscription is getting over,Please contact your IT vendor for renewal. </p>";
                    strbody += "<p>Thank You</p>";
                }
                string img = "";
                IsSend = objCEmail.emailWithCC(strsubject, strbody, senderemailid, reciverEmailID, recieverccemailid, img);
            }
            return IsSend;
        }
        public bool checkDate()
        {
            bool IsCheck = false;
            SqlConnection con = ptraconn.GetItem();
            try
            {
               
                SqlCommand cmd = new SqlCommand("CheckDate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    IsCheck = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception", e.Message);
            }
            finally
            {
                con.Close();
            }
            return IsCheck;
        }
        public int UpdateSubsEmailDate()
        {
            SqlConnection con = ptraconn.GetItem();
            try
            {

                SqlCommand cmd = null;
                cmd = new SqlCommand("UpdateSubsEmailDate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
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
        public string GetAdminMail()
        {
            SqlConnection con = ptraconn.GetItem();
            string AdminEmail = "";
            try
            {
                
                SqlCommand cmd = new SqlCommand("GetAdminMail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    AdminEmail = ds.Tables[0].Rows[0]["Email"].ToString(); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception", e.Message);
            }
            finally
            {
                con.Close();
            }
            return AdminEmail;
        }
    }
}