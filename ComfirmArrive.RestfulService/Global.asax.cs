using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using Nxt.RestfulService.Main;

namespace Nxt.RestfulService
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Edit the base address of Service1 by replacing the "Service1" string below
            RouteTable.Routes.Add(new ServiceRoute("SerchInd", new WebServiceHostFactory(), typeof(SerchInd)));
            RouteTable.Routes.Add(new ServiceRoute("SerchDetail", new WebServiceHostFactory(), typeof(SerchDetail)));
            RouteTable.Routes.Add(new ServiceRoute("ConfirmArrive", new WebServiceHostFactory(), typeof(ConfirmArrive)));
            RouteTable.Routes.Add(new ServiceRoute("Save", new WebServiceHostFactory(), typeof(Save)));
        }
    }
}
