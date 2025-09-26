namespace ITTitans.Hackathon2025.WebAPI.Auth;

public enum AuthClaimType
{
    SeeUser = 1,
    ManageUser = 2,
    
    SeeRole = 10,
    ManageRole = 11,
    
    SupplyCertificateWorkflowStart = 20,
    SupplyCertificateWorkflowProcess = 21,
    
    DemandDataSourceWorkflowStart = 30,
    DemandDataSourceWorkflowProcess = 31,
}