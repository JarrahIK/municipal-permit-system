namespace MunicipalPermitSystem.Api.Models;

/// <summary>
/// Role names as they appear inside the JWT. Use them in controllers, e.g.
/// <c>[Authorize(Roles = AppRoles.Surveyor)]</c> or <c>[Authorize(Roles = AppRoles.AnyStaff)]</c>.
/// Employee roles equal the <see cref="Enums.EmployeeRole"/> names.
/// </summary>
public static class AppRoles
{
    public const string Citizen = "Citizen";
    public const string Surveyor = "Surveyor";
    public const string DepartmentHead = "DepartmentHead";
    public const string Manager = "Manager";
    public const string Accountant = "Accountant";

    public const string AnyStaff = "Surveyor,DepartmentHead,Manager,Accountant";
}
