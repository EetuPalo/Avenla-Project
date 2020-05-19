using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Resources;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.ViewModels
{
    public class RegisterVM
    {
        public int Id { get; set; }

        //This is not typed in anymore. Instead it's constructed in AccController from First and last name
        [DataType(DataType.Text)]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
              ErrorMessageResourceName = "EmailRequired")]
        [RegularExpression(".+@.+\\..+", ErrorMessageResourceType = typeof(Resources.Resources),
                                     ErrorMessageResourceName = "EmailInvalid")]
        public string EMail { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone", ResourceType = typeof(Resources.Resources))]
        [StringLength(20, ErrorMessageResourceType = typeof(Resources.Resources),
                      ErrorMessageResourceName = "PhoneLong")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "FirstName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
              ErrorMessageResourceName = "FirstNameRequired")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources),
                      ErrorMessageResourceName = "FirstNameLong")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "LastName", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
              ErrorMessageResourceName = "LastNameRequired")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources),
                      ErrorMessageResourceName = "LastNameLong")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Company", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
              ErrorMessageResourceName = "CompanyRequired")]
        public string Company { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
            ErrorMessageResourceName = "PasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources.Resources))]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
           ErrorMessageResourceName = "ConfirmPasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resources.Resources))]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }

        public IList<SelectListItem> CompanyList { get; set; }

        public RegisterVM() { CompanyList = new List<SelectListItem>(); }
    }
}
