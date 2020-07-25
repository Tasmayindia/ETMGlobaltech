using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskManagment.Models
{
    public class Employee
    {
        public int EmpId { get; set; }
        [Required(ErrorMessage = "Please enter Emp Code")]
        public string EmpCode { get; set; }
        [Required(ErrorMessage = "Please select Prefix")]
        public string Prefix { get; set; }
        [Required(ErrorMessage = "Please enter First Name")]
        public string FName { get; set; }
        public string MName { get; set; }
        [Required(ErrorMessage = "Please enter Last Name")]
        public string LName { get; set; }
        [Required(ErrorMessage = "Please enter Display Name")]
        public string DisplayName { get; set; }

        public string JoinDate { get; set; }

        public string BirthDate { get; set; }
        public string ConfirmDate { get; set; }
        public string LeftDate { get; set; }
        [Required(ErrorMessage = "Please select Sex")]
        public string Sex { get; set; }
        [Required(ErrorMessage = "Please Enter Email Address")]
        [EmailAddress]
        [Display(Name = "Email")]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        public string EMail { get; set; }
        public string FatherName { get; set; }
        public string PreAdd1 { get; set; }
        public string PreAdd2 { get; set; }
        public string PreCity { get; set; }
        public string PrePin { get; set; }
        public string PreState { get; set; }
        public string PreTel { get; set; }
        [Required(ErrorMessage = "Please Enter Mobile No")]
        [Display(Name = "Mobile")]
        [RegularExpression("^[0-9]{10}", ErrorMessage = "Mobile must be 10 digits only.")]
        public string Mobile { get; set; }
        public string PerAdd1 { get; set; }
        public string PerAdd2 { get; set; }
        public string PerCity { get; set; }
        public int PerPin { get; set; }
        public string PerState { get; set; }
        public string PerTel { get; set; }
        public string blood { get; set; }
        public int Locid { get; set; }

        public string Location { get; set; }
        public string Compid { get; set; }
        public string Company { get; set; }
        public int Deptid { get; set; }
        [Required(ErrorMessage = "Please enter Department Name")]
        public string Department { get; set; }
        public int Catid { get; set; }
        public string Category { get; set; }
        public int Gradeid { get; set; }

        public string Grade { get; set; }
        public string LevGrade { get; set; }
        [Required(ErrorMessage = "Please select Reporting Manager")]
        public string ReptTo { get; set; }
        public string Designation { get; set; }

        public int HODId { get; set; }
        public int OTRepto { get; set; }
        public int OTHodId { get; set; }
        public int FunMgrId { get; set; }
        public string ShiftType { get; set; }
        public int Shiftid { get; set; }
        public int RotId { get; set; }
        public string Probation { get; set; }
        public int GroupId { get; set; }
        public int rept1 { get; set; }
        public int hod1 { get; set; }
        public string Plant { get; set; }
        public string Tag { get; set; }
        public string Gender { get; set; }
        public int FunctionId { get; set; }
        [Required(ErrorMessage = "Please enter UserId")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        public string Password { get; set; }
        public List<Employee> EmployeeList { get; set; }
        public List<SelectListItem> EmpList { get; set; }
        public List<SelectListItem> ApplicationList { get; set; }
        public string GalaxyID { get; set; }
        public string GalaxyIDName { get; set; }
        public bool IsDisable { get; set; }
        public string Manager { get; set; }
        [Required(ErrorMessage = "Please enter Old Password")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Please enter New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "Please enter Confirm Password")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        public string ConfirmPassword { get; set; }

        public int ID { get; set; }
        [Required(ErrorMessage = "Please enter User Name")]
        public string EmailUserName { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        public string EmailPassword { get; set; }
        [Required(ErrorMessage = "Please enter Port")]
        public string Port { get; set; }
        [Required(ErrorMessage = "Please enter SMTP Url")]
        public string SmtpUrl { get; set; }
        [Required(ErrorMessage = "Please select Application")]
        public string Application { get; set; }
      
      public string TaskStatus { get; set; }
      public string FirstReminder { get; set; }
      public string SecondReminder { get; set; }
      public string RepeatReminder { get; set; }
    }
}