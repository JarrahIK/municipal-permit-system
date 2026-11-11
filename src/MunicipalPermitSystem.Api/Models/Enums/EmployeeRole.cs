namespace MunicipalPermitSystem.Api.Models.Enums;

// The four Employee subtypes from the class diagram, stored as one Employee table + a Role column
// (this matches the ER diagram).
public enum EmployeeRole
{
    Surveyor,
    DepartmentHead,
    Manager,
    Accountant
}
