namespace MunicipalPermitSystem.Api.Models.Enums;

// Mirrors the sequence diagrams: Citizen -> Surveyor -> Dept Head -> Manager -> Accountant.
public enum PermitStatus
{
    Submitted,
    UnderSurveyorReview,
    ReturnedToCitizen,
    ForwardedToDeptHead,
    ReturnedToSurveyor,
    ForwardedToManager,
    Approved,
    Rejected,
    PaymentPending,
    Completed
}
