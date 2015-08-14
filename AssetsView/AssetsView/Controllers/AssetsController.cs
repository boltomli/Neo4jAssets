using AssetsView.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssetsView.Controllers
{
    public class AssetsController : Controller
    {
        public AssetsInterface assetsService;

        public AssetsController(AssetsInterface assetsService)
        {
            this.assetsService = assetsService;
        }
    }
}