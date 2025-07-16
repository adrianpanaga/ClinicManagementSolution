Clinic Management API
Project Overview
The Clinic Management API serves as the central backend system for a comprehensive Clinic Management Solution. It is designed to handle all core business logic, data validation, and data persistence for both the public-facing Patient Portal and the secure internal Clinic Staff Portal. Built with ASP.NET Core, it provides a robust, scalable, and secure foundation for managing patient information, appointments, inventory, and staff operations.

Features
This API provides a wide range of functionalities to support clinic operations:

Appointment Management:

Create, retrieve, update, and delete appointments.

Intelligent handling of new or existing patients during appointment creation.

Dynamic calculation of available appointment slots based on clinic hours and existing bookings.

Retrieve appointments by patient contact details or PatientId.

Enhanced querying for staff, including filtering by date, status, patient, and doctor.

Patient Verification:

Securely request and verify OTP (One-Time Password) via contact details (email/phone) for patient identity verification without traditional logins.

Returns essential patient details upon successful verification.

Patient Record Management:

Comprehensive CRUD (Create, Read, Update, Delete) operations for patient demographic information.

Detailed medical records, encompassing past diagnoses, treatments, and medications.

Triage Records: Capture and manage vital signs, chief complaints, and other triage data during patient encounters.

Lab Results Storage: Store and retrieve various laboratory test results, including test names, values, units, and interpretations.

Additional Record Storage: Securely manage metadata for supplementary patient documents like consent forms, referral letters, and insurance policies, with links to their storage locations.

Inventory Management:

Manage medical supplies and medicines, including catalog, stock levels, batches, and transactions.

Staff Management:

Manage staff details, including a publicly accessible endpoint for specialist staff (doctors) available for booking.

User & Role Management (for Clinic Staff):

Endpoints for clinic staff user registration, login, and role assignment.

Designed for JWT-based authentication and flexible Role-Based Access Control (RBAC) with support for multiple roles per user.

Clinic Settings:

Database-driven configuration for clinic operational hours (Open Time, Close Time, Lunch Breaks). (Future: Admin API for managing these settings).

Core CRUD Operations:

Full CRUD support for essential entities: Appointments, Inventory Items, Item Batches, Medical Records, Patients, Services, Staff Details, Stock Transactions, Users, and Vendors.

Now includes Triage Records, Lab Results, and Patient Documents.

Technology Stack
Backend Framework: ASP.NET Core (.NET 9.0 Preview)

Database: Microsoft SQL Server

ORM (Object-Relational Mapper): Entity Framework Core

Authentication & Authorization: JWT (JSON Web Tokens) for API security, ASP.NET Core Identity for clinic staff user management.

API Documentation: Swagger/OpenAPI

API Endpoints
The API follows a RESTful architecture, providing clear and consistent endpoints for various resources. Detailed documentation of all available endpoints, their request/response schemas, and authentication requirements can be found via Swagger UI once the API is running.

Key Endpoint Categories:

/api/Appointments

/api/Auth (for staff login/registration)

/api/InventoryItems

/api/ItemBatches

/api/MedicalRecords

/api/Patients

/api/Services

/api/StaffDetails

/api/StockTransactions

/api/Users (for staff user management)

/api/Vendors

/api/PatientVerification (for patient OTP requests/verification)

/api/TriageRecords

/api/LabResults

/api/PatientDocuments

Refer to the generated Swagger documentation for comprehensive details on each endpoint, including HTTP methods, parameters, and response structures.

Authentication & Authorization
Clinic Staff Portal: Utilizes JWT (JSON Web Tokens) for secure API access. Clinic staff authentication is managed via ASP.NET Core Identity, supporting a many-to-many relationship between users and roles for granular, role-based access control.

Patient Portal (Public Access): Patient identity is verified through alternative mechanisms (e.g., OTP via email/SMS) without requiring persistent user sessions or traditional logins to access personal data. Publicly accessible endpoints are explicitly marked.

Database
The API interacts with a Microsoft SQL Server database. Entity Framework Core is used to manage database interactions, migrations, and object-relational mapping. The database schema includes entities for Appointments, Patients, Staff, Services, Inventory, Clinic Settings, Triage Records, Lab Results, and Patient Documents, all designed to support the described functionalities.

Setup and Running (Development)
To set up and run the API locally:

Prerequisites:

.NET 9.0 SDK (Preview)

SQL Server instance (LocalDB, SQL Express, or a full SQL Server)

Clone the Repository:

Bash

git clone <your-repository-url>
cd <your-repo-name>/src/ClinicManagementAPI
Configure Database Connection:

Open appsettings.Development.json (or appsettings.json) and update the ConnectionStrings:DefaultConnection to point to your SQL Server instance.

Apply Migrations and Seed Data:

Bash

dotnet ef database update
This command will create the database (if it doesn't exist), apply all pending migrations, and seed initial data for ClinicSetting, Service, StaffDetail, Patient, Appointments, Medical Records, Triage Records, Lab Results, and Patient Documents entities.

Run the API:

Bash

dotnet run
The API will typically start on https://localhost:<port> (check console output for exact URL). Swagger UI will be available at /swagger.

Error Handling
The API implements robust error handling to provide meaningful responses for various scenarios, including validation errors, not found resources, and server errors.

Future Enhancements
Dedicated API endpoints for managing ClinicSettings by Admin/HR staff.

Further expansion to support all features required by the Clinic Staff Portal (e.g., comprehensive HR management, detailed reporting, advanced user/role management screens).
