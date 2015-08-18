using AssetsViewSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AssetsViewSP.Controllers
{
    [RoutePrefix("search")]
    public class SearchController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult SearchDevicesByOwner(string q)
        {
            var data = WebApiConfig.GraphClient.Cypher
               .Match("(d:Device)<-[:OWNS]-(o:Owner)")
               .Where("o.MSAlias = {msalias}")
               .WithParam("msalias", q)
               .Return<Device>("d")
               .Results.ToList();

            return Ok(data.Select(c => new { device = c }));
        }
    }
}