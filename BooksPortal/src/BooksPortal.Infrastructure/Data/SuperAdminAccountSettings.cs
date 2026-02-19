namespace BooksPortal.Infrastructure.Data;

public class SuperAdminAccountSettings : SeedAccountSettings
{
    public SuperAdminAccountSettings()
    {
        UserName = "admin";
        Email = "admin@booksportal.local";
        Password = "Admin@123456";
        FullName = "System Administrator";
        NationalId = "A0000001";
        Designation = "System Administrator";
    }
}
