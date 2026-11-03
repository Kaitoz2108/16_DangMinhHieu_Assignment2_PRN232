using System;

namespace FUNewsManagement_Client.Services;

public class UnauthorizedApiException : Exception
{
    public UnauthorizedApiException(string message = "Your session has expired or you are unauthorized.")
        : base(message)
    {
    }
}

public class ForbiddenApiException : Exception
{
    public ForbiddenApiException(string message = "You do not have permission to access this resource.")
        : base(message)
    {
    }
}
