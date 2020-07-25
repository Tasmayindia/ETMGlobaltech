using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Mvc;
using System.ComponentModel;

namespace TaskManagment.Models
{
    public class Request
    {
        public string status1 { get; set; }
        public string ReqStatus { get; set; } 
        public string ProjectType { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter user Id")]
        public string GalaxyID { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
        public string Role { get; set; }
        public string InitiatorStatus { get; set; }
        public string DoerbyName { get; set; }
        public string HistorySubmitedBy { get; set; }
        public string HistoryDoerProposedBy { get; set; }
        public string HistoryInitorAggredBy { get; set; }
        public string HistoryStatusUpdate { get; set; }
        public string HistoryStatusUpdateCompleted { get; set; }
        public string EmpCode { get; set; }   
        public string galaxy { get; set; }
        public string EmpName { get; set; }
        public string deptName { get; set; }
        public string DeptCode { get; set; }
        public string EmpCompCode { get; set; }
        public int ProjectNo { get; set; }
        public string FromDate { get; set; }
        public String ToDate { get; set; }
        public Int32 LastStatus { get; set; }
        public Int32 NextStatus { get; set; }
        public HttpPostedFileBase doerAttachment { get; set; }
        public string attachment1 { get; set; }
        public string DelegateTo { get; set; }
        public string DelegateToGalaxy { get; set; }
        public string HOODGalaxyId { get; set; }
        public string DoerNameGalaxy { get;set; }

        [Required(ErrorMessage = "Please Enter Task Name")]
        public string ProjectName { get; set; }
        public int OntimePer { get; set; }
        
        public string ProjectDescription { get; set; }
        
        public string InitiatorRemark { get; set; }

        
        public string ExpectedOutput1 { get; set; }
        public string AddedBy { get; set; }
        [Required(ErrorMessage = "Please Select Expected Target Date")]
        public string ExpectedTargetDate { get; set; }        
        public string ProposedDate { get; set; }
        public string ProposedDateRemark { get; set; }               
        public string AgreedDate { get; set; }
        public string AgreedDateRemark { get; set; }
        public string Remark { get; set; }
        
        public string DesignLead { get; set; }
        public string RemarkTrace { get; set; }
        public string NewDesignLead { get; set; }
        public List<ProjectList> ProjectList { get; set; } 
        public List<DoerName> DoerName { get; set; }
 
        public string InitiatorName { get; set; }
         public string pendingfor { get; set; }
        public string FinalStatus { get; set; }
        [Required(ErrorMessage = "Please Select Assignee")]
        public string DoerName1 { get; set; }
        
        public string ReviewBy { get; set; }
        public string ReviewByGalaxy { get; set; }
        public string ExpDetailpurposedate { get; set; }

       
        public string DoerStatus { get; set; }

        public string ReviewStatus { get; set; }
        public string RequestOn { get; set; }
        public string DelegateStatus { get; set; }
        public string pendingforGalaxy { get; set; }
        public string searchBy { get; set; }
        public string GalaxyIDName { get; set; }
        public string Initiator {get;set;}
        public string Doer      {get;set;}
        public string Delegate  {get;set;}
        public string Review { get; set; }
        public string oldExpDate { get; set; }
        public string oldProposedDate  { get; set; }

        public List<SelectListItem> AssigneeList { get; set; }
        public List<SelectListItem> AssigneeList1 { get; set; }
    }

    public class ProjectList
    {
        public int ProjectNo { get; set; }
        public string DelegateTo { get; set; }
        public string DelegateToGalaxy { get; set; }
        public string InitiatorStatus { get; set; }
        public string DoerStatus { get; set; }
        public string DelegateStatus { get; set; }
        public string ReviewBy { get; set; }
        public string ReviewByGalaxy { get; set; }
        public string ProjectName { get; set; }
        public string pendingfor { get; set; }
        public string ProjectDescription { get; set; }
        public string ExpectedOutput { get; set; }
        public string AddedBy { get; set; }
        public int? OntimePer { get; set; }
        public string ExpectedTargetDate { get; set; }
        public string ProposedDate { get; set; }
        public string AgreedDate { get; set; }
        public string DesignLead { get; set; }
        public string NewExpectedDate { get; set; }
        public string NewProposedDate { get; set; }
        public string NewDesignLead { get; set; }
        public string DoerName1 { get; set; }
        public string searchBy { get; set; }
        public string Status { get; set; }
        public string ComletedOn { get; set; }
        public string RequestOn { get; set; }
        public string DoerNameGalaxy { get; set; }
        public string GalaxyID { get; set; }
        public string AddedByGalaxy { get; set; }
    }

    

   
   
    public class DoerName
    {

        public string Key { get; set; }
        public string Display { get; set; }
    }

    public class AssigneeRoll
    {
        public string Key { get; set; }
        public string Display { get; set; }
    }
}