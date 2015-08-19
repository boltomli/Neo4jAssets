using AssetsViewSP.Models;
using System.Linq;
using System.Web.Http;

namespace AssetsViewSP.Controllers
{
    [RoutePrefix("device")]
    public class DeviceController : ApiController
    {
        [HttpGet]
        [Route("{sn}")]
        public IHttpActionResult GetDeviceBySN(string sn)
        {
            var data = WebApiConfig.GraphClient.Cypher
               .Match("(d:Device)")
               .Where("d.SN = {sn}")
               .WithParam("sn", sn)
               .Return(d => d.As<Device>())
               .Limit(1)
               .Results.FirstOrDefault();

            return Ok(data);
        }
    }
}