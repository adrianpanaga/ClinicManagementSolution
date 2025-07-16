// Location: ClinicManagement.Data/Models/Role.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Data.Models;

public partial class Role : IdentityRole<int>
{
    public Role()
    {
        UserRoles = new List<UserRole>();
    }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Add this if you need explicit navigation from Role to UserRoles
    public virtual ICollection<UserRole> UserRoles { get; set; }
}