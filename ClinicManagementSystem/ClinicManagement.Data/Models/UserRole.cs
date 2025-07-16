using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagement.Data.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        // Add navigation properties to your custom User and Role models
        // These are the properties that EF Core will look for when configuring relationships
        public virtual User User { get; set; } = null!; // Non-nullable if FK is non-nullable
        public virtual Role Role { get; set; } = null!; // Non-nullable if FK is non-nullable
    }
}
