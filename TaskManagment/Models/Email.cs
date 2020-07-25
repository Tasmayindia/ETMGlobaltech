using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using TaskManagment.Models;
using TaskManagment.Controllers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Drawing;


namespace TaskManagment.Models 
{
    public class Email
    {
        DateTime RequestDate, CompletedOn, Cancelledon;
        ProjectTrackConnection ptraconn = new ProjectTrackConnection();
         
        public int email(string name, string Role, string reciverEmailID, string receiverCCEmailId, string ccGalaxy , string GalaxyID, string ProjectNo,Request R,string status1)
        {

            SqlConnection con = ptraconn.GetItem();
            string qry = "select * from [dbo].[ProjectList] where [ProjectNo]='" + ProjectNo + "'";
            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            int ProjectNo1 = Convert.ToInt32(ds.Tables[0].Rows[0]["ProjectNo"].ToString());          
            string ProjectName = ds.Tables[0].Rows[0]["ProjectName"].ToString();
            string ProjectDescription = ds.Tables[0].Rows[0]["ProjectDescription"].ToString();
            string ExpectedOutput1 = ds.Tables[0].Rows[0]["ExpectedOutput"].ToString();
            string AddedBy = ds.Tables[0].Rows[0]["AddedBy"].ToString();
            string ExpectedTargetDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["ExpectedTargDat"]).ToString("dd-MMM-yyyy");
            string ProposedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[0]["ProposedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[0]["proposeddate"]).ToString("dd-MMM-yyyy");
            //string AgreedDate = string.IsNullOrEmpty(ds.Tables[0].Rows[0]["AgreedDate"].ToString()) ? "" : Convert.ToDateTime(ds.Tables[0].Rows[0]["AgreedDate"]).ToString("dd-MMM-yyyy");
            string AddedByName = GetDisplayName(AddedBy);
            string DoerName1 = ds.Tables[0].Rows[0]["DoerName"].ToString();
            string DoerStatus = ds.Tables[0].Rows[0]["DoerStatus"].ToString();
            string InitilizerStatus = ds.Tables[0].Rows[0]["InitilizerStatus"].ToString();
            string DoerbyName = GetDisplayName(DoerName1);
           // string DoerByNameEmail = GetEmailId(ProjectNo);
            string ccGalaxyName = GetDisplayName(ccGalaxy);
            string ReviewByGalaxy = ds.Tables[0].Rows[0]["ReviewBy"].ToString();
            string ReviewBy = GetDisplayName(ReviewByGalaxy);
            string refId = ds.Tables[0].Rows[0]["refid"].ToString();
            string DelegateStatus = ds.Tables[0].Rows[0]["DelegateStatus"].ToString();
            string ReviewStatus = ds.Tables[0].Rows[0]["ReviewByStatus"].ToString();            
            string DelegateToGalaxy = ds.Tables[0].Rows[0]["DelegateTo"].ToString();
            string DelegateTo = "";
            if (!(string.IsNullOrEmpty(DelegateToGalaxy)))
            {
                  DelegateTo = GetDisplayName(DelegateToGalaxy);
            }
            else
            {
               DelegateTo="";
            }

             
            SqlCommand cmdnames = new SqlCommand("GetActivitesby", con);
            cmdnames.CommandType = CommandType.StoredProcedure;
            cmdnames.Parameters.Add("@TaskNo", SqlDbType.VarChar).Value = ProjectNo;
            SqlDataAdapter danames = new SqlDataAdapter(cmdnames);
            DataSet dsnames = new DataSet();
            danames.Fill(dsnames);

            for (int i = 0; i < dsnames.Tables[0].Rows.Count; i++)
            {
                string LastActivityRole = dsnames.Tables[0].Rows[i]["LastActivityRole"].ToString();
                string Names = dsnames.Tables[0].Rows[i]["LastActivityByName"].ToString();
                switch (LastActivityRole)
                {
                    case "Initiator": R.Initiator = Names; break;
                    case "Doer": R.Doer = Names; break;
                    case "Delegate": R.Delegate = Names; break;
                    case "Review": R.Review = Names; break;
                }
            }

            if (reciverEmailID.StartsWith(","))
            {
                reciverEmailID = reciverEmailID.Remove(0, 1);
            }
            if (reciverEmailID.EndsWith(","))
            {
                reciverEmailID = reciverEmailID.Remove(reciverEmailID.LastIndexOf(","), 1);
            }

            if (receiverCCEmailId.StartsWith(","))
            {
                receiverCCEmailId = receiverCCEmailId.Remove(0, 1);
            }
            if (receiverCCEmailId.EndsWith(","))
            {
                receiverCCEmailId = receiverCCEmailId.Remove(receiverCCEmailId.LastIndexOf(","), 1);
            }


            String Subject = "";
            if (reciverEmailID != "" || reciverEmailID != null)
            {
                try
                {
                    String strbody = "";
                    if (status1 == "New")
                    {
                        Subject = "Enterprise Task Management - " + ProjectNo + " New Task Created";
                        strbody = "Enterprise Task Management - " + ProjectNo + " New Task Created";
                        strbody = strbody + "<br>Pending for " + name + "<br/><br/>";
                    }
                    else if (status1 == "Reviewer Verified and Task Completed")
                    {
                        Subject = "Enterprise Task Management - " + ProjectNo + "  " + status1;
                        strbody = "Enterprise Task Management - " + ProjectNo + "  " + status1;                      
                    }
                    else if (status1 == "Task Cancelled")
                    {
                        Subject = "Enterprise Task Management - " + ProjectNo + "  " + status1;
                        strbody = "Enterprise Task Management - " + ProjectNo + "  " + status1;
                    }
                    else 
                    {
                        Subject = "Enterprise Task Management - " + ProjectNo + "  "+ status1;
                        strbody = "Enterprise Task Management - " + ProjectNo + "  "+ status1;
                        strbody = strbody + "<br>Pending for " + name + "<br/><br/>";
                    }
                      //(" + AddedByName + ")
                    strbody = strbody + "<table border='1' cellpadding='0' cellspacing='0' >";
                    strbody = strbody + "<tr class='heading'> Task Details: </tr><br/>";
                    strbody = strbody + "<tr><td> Task No :  </td><td>" + ProjectNo1 + "</td></tr>";
                    strbody = strbody + "<tr><td> Task Name :  </td><td>" + ProjectName + "</td></tr>";
                    strbody = strbody + "<tr><td> Task Description :  </td><td>" + ProjectDescription + "</td></tr>";
                    strbody = strbody + "<tr><td> Expected Output  </td><td>" + ExpectedOutput1 + "</td></tr>";
                    strbody = strbody + "<tr><td> Expected Target Date  :  </td><td>" + ExpectedTargetDate + "</td></tr>";                    
                    strbody = strbody + "<tr><td> Proposed Date :  </td><td>" + ProposedDate + "</td></tr>";
                    strbody = strbody + "<tr><td> Assiginee :  </td><td>" + DoerbyName + "</td></tr>";
                    strbody = strbody + "<tr><td> Doer :  </td><td>" + DelegateTo + "</td></tr>";
                    strbody = strbody + "<tr><td> Reviewer :  </td><td>" + ReviewBy + "</td></tr>";

                    if (DoerStatus == "")
                    {
                        strbody = strbody + "<tr><td> Doer status :  </td><td>" + DelegateStatus + "</td></tr>";
                    }
                    else
                    {
                        strbody = strbody + "<tr><td> Doer status :  </td><td>" + DoerStatus + "</td></tr>";
                    }

                    strbody = strbody + "<tr> </tr><tr> </tr>";

                     
            string RemarkFormated = "";
           
            SqlCommand cmd1 = new SqlCommand("GetProjectStatusRemark", con);
            cmd1.CommandType = CommandType.StoredProcedure;
            cmd1.Parameters.Add("@projectno", SqlDbType.Int).Value = ProjectNo;
            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
            DataSet ds1 = new DataSet();
            da1.Fill(ds1);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
                {

                    RemarkFormated = RemarkFormated + "\n" + (Convert.ToDateTime(ds1.Tables[0].Rows[i]["LastActivityOn"].ToString())).ToLongDateString().ToString() + " " + ds1.Tables[0].Rows[i]["LastActivityBy"].ToString();

                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Review" && ds1.Tables[0].Rows[i]["NextRole"].ToString() == "Delegate")
                    {
                        RemarkFormated = RemarkFormated + "\n Reviewer Submitted Recommendation\n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Review" && ds1.Tables[0].Rows[i]["NextRole"].ToString() == "Close")
                    {
                        RemarkFormated = RemarkFormated + "\n Reviewer Verified and Task Completed \n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Review" && ds1.Tables[0].Rows[i]["NextRole"].ToString() == "Review")
                    {
                        RemarkFormated = RemarkFormated + "\n Progress Updated by Reviewer \n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Doer")
                    {
                        if (R.DelegateToGalaxy == R.DoerNameGalaxy)
                        {
                            RemarkFormated = RemarkFormated + "\n Proposed Date Submitted \n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                        }
                        else
                        {
                            RemarkFormated = RemarkFormated + "\n Proposed Date Submitted and Delegated\n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                        }
                    }

                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Delegate" && ds1.Tables[0].Rows[i]["NextRole"].ToString() == "Delegate")
                    {
                        RemarkFormated = RemarkFormated + "\n Progress Updated by Doer\n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Delegate" && ds1.Tables[0].Rows[i]["NextRole"].ToString() != "Delegate")
                    {
                        if (R.DoerStatus == "Completed" || R.DelegateStatus == "Completed")
                        {
                            RemarkFormated = RemarkFormated + "\n Task Completed by Doer \n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                        }
                    }
                    if (ds1.Tables[0].Rows[i]["LastActivityRole"].ToString() == "Initiator")
                    {
                        RemarkFormated = RemarkFormated + "\n New Task Initiate \n" + ds1.Tables[0].Rows[i]["Remark"].ToString() + "\n";
                    }
                }
            }
            
        
                    strbody = strbody + "<tr> <td>Remark</td> <td>"+ RemarkFormated.Replace("\n","<br>")+ "<td> </tr>";
                    strbody = strbody + "</table> <br/><br/>";

                    string weburl = HttpContext.Current.Request.Url.OriginalString;
                    weburl = weburl.Remove(weburl.LastIndexOf("/"), weburl.Length - weburl.LastIndexOf("/"));
                    strbody = strbody + "<br/><font color=\"#0000FF\">This is auto generated email. Do not reply. </font>";


                    //Mail Server Info
                    var result = ServerInfo();
                    MailMessage message = new MailMessage(result.UserName.ToString(), reciverEmailID, Subject, strbody);
                    if (!string.IsNullOrEmpty(receiverCCEmailId))
                    {
                        string[] arrCCMessages = receiverCCEmailId.Split(',');


                        int i = 0;
                        for (i = 0; i <= arrCCMessages.Length - 1; i++)
                        {
                            if (arrCCMessages[i].ToString() != "")
                            {
                                MailAddress CCAddress = new MailAddress(arrCCMessages[i].ToString());
                                message.CC.Add(CCAddress);
                            }

                        }

                    }
                    //MailAddress CCAddress = new MailAddress(receiverCCEmailId.ToString());
                    //message.CC.Add(CCAddress);
                    message.IsBodyHtml = true;
                    string SMTPURL = result.SmtpUrl.ToString();
                    int port = Convert.ToInt16(result.Port);
                    string password = result.Password.ToString();
                    bool enableSSL = true;
                    SmtpClient emailClient = new SmtpClient(SMTPURL, port);
                    emailClient.Credentials = new NetworkCredential(result.UserName.ToString(), password);
                    emailClient.EnableSsl = enableSSL;
                    emailClient.Send(message);
                    foreach (System.Net.Mail.Attachment attachment in message.Attachments)
                    {
                        attachment.Dispose();
                    }
                   
                }
                catch (Exception ex)
                {
                    StreamWriter logFile;
                    if (!File.Exists("C:\\TaskMgmt\\emailFailLog.txt"))
                    {
                        logFile = File.CreateText("C:\\TaskMgmt\\emailFailLog.txt");
                    }
                    else
                    {
                        logFile = File.AppendText("C:\\TaskMgmt\\emailFailLog.txt");
                    }
                    logFile.Write(Environment.NewLine + System.DateTime.Now + " - To emailID as " + reciverEmailID + " CCEmailId " + receiverCCEmailId + " : Subject as " + Subject + " : " + ex.Message, 0, -1);
                    logFile.Close();
                    logFile.Dispose();
                    string msg = ex.Message;
                }
            }

            return 1;

        }
        public bool emailWithCC(string strSubject, string strBody, string senderEmailID, string reciverEmailID, string receiverCCEmailId, string Image)
        {
            var result = ServerInfo();
            senderEmailID = result.UserName;
            if (reciverEmailID.StartsWith(","))
            {
                reciverEmailID = reciverEmailID.Remove(0, 1);
            }
            if (reciverEmailID.EndsWith(","))
            {
                reciverEmailID = reciverEmailID.Remove(reciverEmailID.LastIndexOf(","), 1);
            }

            if (receiverCCEmailId.StartsWith(","))
            {
                receiverCCEmailId = receiverCCEmailId.Remove(0, 1);
            }
            if (receiverCCEmailId.EndsWith(","))
            {
                receiverCCEmailId = receiverCCEmailId.Remove(receiverCCEmailId.LastIndexOf(","), 1);
            }

            try
            {
                MailMessage message = new MailMessage(senderEmailID, reciverEmailID, strSubject, strBody);


                if (!string.IsNullOrEmpty(receiverCCEmailId))
                {
                    string[] arrCCMessages = receiverCCEmailId.Split(',');


                    int i = 0;
                    for (i = 0; i <= arrCCMessages.Length - 1; i++)
                    {
                        if (arrCCMessages[i].ToString() != "")
                        {
                            MailAddress CCAddress = new MailAddress(arrCCMessages[i].ToString());
                            message.CC.Add(CCAddress);
                        }

                    }

                }
                
                message.IsBodyHtml = true;
                string SMTPURL = result.SmtpUrl;
                int port = Convert.ToInt32(result.Port);
                string password = result.Password;
                bool enableSSL = true;
                SmtpClient emailClient = new SmtpClient(SMTPURL, port);
                emailClient.Credentials = new NetworkCredential(result.UserName, password);
                emailClient.EnableSsl = enableSSL;
                emailClient.Send(message);
                foreach (System.Net.Mail.Attachment attachment in message.Attachments)
                {
                    attachment.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                StreamWriter logFile = null;
                if (!File.Exists("C:\\TaskMgmt\\emailFailLog.txt"))
                {
                    logFile = File.CreateText("C:\\TaskMgmt\\emailFailLog.txt");
                }
                else
                {
                    logFile = File.AppendText("C:\\TaskMgmt\\emailFailLog.txt");
                }
                logFile.Write(Environment.NewLine + System.DateTime.Now + " - To emailID as " + reciverEmailID + " CCEmailId " + receiverCCEmailId + " : Subject as " + strSubject + " : " + ex.Message, 0, -1);
                logFile.Close();
                logFile.Dispose();
                return false;
            }
        }
        public String GetEmailId(string ProjectNo)
        {
            SqlConnection con = ptraconn.GetItem();
            SqlCommand cmd = new SqlCommand("EmailIdWhenTransfer", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ReqNo", SqlDbType.VarChar).Value = ProjectNo;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string EmailId = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                EmailId = ds.Tables[0].Rows[0]["EmailID"].ToString();
            }

            return EmailId;
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
        public MailServerInfo ServerInfo()
        {
            MailServerInfo model = new MailServerInfo();
            SqlConnection con = ptraconn.GetItem();
            try
            {

                SqlCommand cmd = new SqlCommand("GetMailServerInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    model.UserName = ds.Tables[0].Rows[0]["UserName"].ToString();
                    model.Password = ds.Tables[0].Rows[0]["Password"].ToString();
                    model.Port = ds.Tables[0].Rows[0]["Port"].ToString();
                    model.SmtpUrl = ds.Tables[0].Rows[0]["SmtpUrl"].ToString();
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                con.Close();
            }
            return model;
        }
    }
}