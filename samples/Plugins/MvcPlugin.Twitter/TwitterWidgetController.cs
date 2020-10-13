using Microsoft.AspNetCore.Mvc;

namespace MvcPlugin.Twitter
{
    [Route("twitter")]
    // The name of the Views folder must be TwitterWidget
    public class TwitterWidgetController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}