using AssetsViewSP.Models;
using System.Linq;
using System.Web.Http;

namespace AssetsViewSP.Controllers
{
    [RoutePrefix("user")]
    public class UserController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetUsersByManager(string manager)
        {
            var data = WebApiConfig.GraphClient.Cypher
               .Match("(u:Owner)-[:REPORTSTO]->(m:Owner)")
               .Where("m.MSAlias = {manager}")
               .WithParam("manager", manager)
               .Return(u => u.As<User>())
               .OrderBy("u.MSAlias")
               .Results.ToList();

            return Ok(data.Select(c => new { user = c }));
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetUserManager(string sub)
        {
            var data = WebApiConfig.GraphClient.Cypher
               .Match("(u:Owner)-[:REPORTSTO]->(m:Owner)")
               .Where("u.MSAlias = {sub}")
               .WithParam("sub", sub)
               .Return(m => m.As<User>())
               .Limit(1)
               .Results.FirstOrDefault();

            return Ok(data);
        }
    }
}