using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Hosting; 
using System.IO; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Buddy.API.SPA.Controllers 
{
    /// <summary>
    /// A Basic SPA controller that serves a HTML file from
    /// within the wwwroot folder, at: wwwroot/dist/index.html
    /// </summary>
    [Route("/")]
    public abstract class SpaController : ControllerBase 
    {
        private readonly IHostingEnvironment _env;
        private readonly string _pathToIndexHTML;
        private readonly ILogger _logger;

        public SpaController(IHostingEnvironment env, ILogger logger) 
        {
            _env = env;
            _logger = logger;
        }

        public SpaController(string pathToIndexHTML, ILogger logger)
        {
            _pathToIndexHTML = pathToIndexHTML;
            _logger = logger;
        }

        /// <summary>
        /// By default returns the "index.html" file in "wwwroot/dist"
        /// 
        /// If you want to use a View for the spa entrypoint you can
        /// override this method and "return View()". Don't forget to
        /// create the view youself!
        /// </summary>
        /// <returns>The SPA HTML file</returns>
        [HttpGet]
        [Produces("text/html", Type = typeof(OkResult))]
        [AllowAnonymous]
        public virtual ActionResult Index()
        {
            var filePath = GetIndexPath();

            if(System.IO.File.Exists(filePath))
            {
                return PhysicalFile(
                    filePath,
                    "text/html"
                );
            }

            _logger.LogError("Failed to find index.html in path: " + filePath);
            throw new System.Exception("No index.html file found. Did you remember to build the front-end?");
        }

        private string GetIndexPath()
        {
            if(_pathToIndexHTML == null)
            {
                return Path.Combine(_env.WebRootPath, "dist", "index.html");
            }

            return Path.Combine(_pathToIndexHTML, "index.html");
        }
    }

}
