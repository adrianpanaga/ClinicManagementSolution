// Location: ClinicManagement.Data/Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Data.Models
{
    public partial class User : IdentityUser<int>
    {
        public User()
        {
            // IDE0028/IDE0305: Collection initialization can be simplified (use `{}` instead of `new HashSet<T>()` or `new List<T>()` if not adding initial elements)
            StaffDetails = new HashSet<StaffDetail>();
            UserRoles = new List<UserRole>();
        }

        public bool IsActive { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual ICollection<StaffDetail> StaffDetails { get; set; } // IDE0028/IDE0305: Collection initialization can be simplified
    }
}