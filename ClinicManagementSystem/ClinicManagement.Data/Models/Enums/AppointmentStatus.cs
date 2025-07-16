using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Models/Enums/AppointmentStatus.cs
namespace ClinicManagement.Data.Models.Enums // Adjust namespace if different
{
    public enum AppointmentStatus
    {
        Scheduled,  // Appointment is booked and pending
        Confirmed,  // Patient has confirmed the appointment
        Completed,  // Appointment has taken place
        Cancelled,  // Appointment was cancelled before it occurred
        NoShow,      // Patient did not show up for the appointment
        Rescheduled // Appointment was moved to a different time/date
    }
}