using MappingDesktopUI.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MappingDesktopUI.Library.Api
{
    public interface IUserEndpoint
    {
        Task<List<UserModel>> GetAll();
    }
}