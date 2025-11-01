namespace IdentityAuthorization.Authorization;

public static class Permissions
{
    public const string UsersRead= "users:read";
    public const string UsersUpdate= "users:update";
    public const string UsersDelete= "users:delete";
    
        
}

public static class CustomClaims
{
    public const string Permissions = "permissions";
}