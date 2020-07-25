using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
namespace TaskManagment.Controllers
{
    //public class ProjectTrackConnController : Controller
    //{
    //    // GET: ProjectTrackConn
    //    public ActionResult Index()
    //    {
    //        return View();
    //    }
    //}

    public class ProjectTrackConnection
    {
    public   SqlConnection GetItem()
            {

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connProjectTrack"].ConnectionString);
            return conn;
           
           }                             
    }

}