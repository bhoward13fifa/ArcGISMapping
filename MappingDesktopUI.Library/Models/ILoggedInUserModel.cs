using System;

namespace MappingDesktopUI.Library.Models
{
    public interface ILoggedInUserModel
    {
        DateTime CreateDate { get; set; }
        string UserEmail { get; set; }
        string UserFirstName { get; set; }
        string UserId { get; set; }
        string UserLastName { get; set; }
        string Token { get; set; }
    }
}