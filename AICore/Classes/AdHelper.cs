using System.DirectoryServices.AccountManagement;

//using SematicKernelWeb.ClaimsIdentities;

namespace AICore.Classes;

public static class AdHelper
{
    public static string GetUserName(this Guid activeDirectoryId)
    {
        try
        {
            using PrincipalContext principalContext = new(ContextType.Domain);
            UserPrincipal userPrincipal =
                UserPrincipal.FindByIdentity(principalContext, activeDirectoryId.ToString());

            if (userPrincipal != null)
                return userPrincipal.SamAccountName ?? userPrincipal.UserPrincipalName ?? userPrincipal.Name;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return "";
    }
}