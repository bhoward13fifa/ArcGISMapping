using MappingDataManager.Library.Models;
using MappingDataManager.Library.DataAccess;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web.Http;

namespace ArcGISMapping.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        [HttpGet]
        public UserModel GetById()
        {
            string userId = RequestContext.Principal.Identity.GetUserId();
            UserData data = new UserData();

            return data.GetUserById(userId).First();
        }

    }
}
