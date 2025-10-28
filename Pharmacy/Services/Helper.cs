using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace Pharmacy.Services
{
    public static class Helper
    {
        public static async Task<string> RenderRazorViewToString(Controller controller, string viewName, object model = null)
        {
            controller.ViewData.Model = model;
            
            using (var writer = new StringWriter())
            {
                // Use GetRequiredService to avoid nullable service warning and fail fast if view engine is not available
                var viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

        public static async Task<string> RenderPartialViewToString(Controller controller, string partialViewName, object model)
        {
            if (string.IsNullOrEmpty(partialViewName))
                partialViewName = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                // Use GetRequiredService to avoid nullable service warning and fail fast if view engine is not available
                var viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, partialViewName, false);

                if (!viewResult.Success)
                {
                    viewResult = viewEngine.FindView(controller.ControllerContext, partialViewName, true);
                }

                if (viewResult.Success == false)
                {
                    return $"A view with the name {partialViewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}