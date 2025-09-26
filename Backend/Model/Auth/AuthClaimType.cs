namespace ITTitans.Hackathon2025.Model.Auth;

public enum AuthClaimType
{
    SeeUser = 1,
    ManageUser = 2,
    
    SeeRole = 10,
    ManageRole = 11,
    
    SeeSkill = 12,
    ManageSkill = 13,
    
    SupplyCertificateWorkflowStart = 20,
    SupplyCertificateWorkflowProcess = 21,
    
    DemandDataSourceWorkflowStart = 30,
    DemandDataSourceWorkflowProcess = 31,
}