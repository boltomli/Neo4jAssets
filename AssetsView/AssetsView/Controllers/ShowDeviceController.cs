using AssetsView.Service;
using System;
using System.Web;
using System.Web.Mvc;

namespace AssetsView.Controllers
{
    public class ShowDeviceController : AssetsController
    {
        public ShowDeviceController(AssetsInterface assetsService) : base(assetsService) { }

        // GET: ShowDevice
        public ActionResult Index(string msAlias)
        {
            var devices = assetsService.deviceInterface.getDevicesOwnedByEmployee(msAlias);
            return View();
        }
    }
}