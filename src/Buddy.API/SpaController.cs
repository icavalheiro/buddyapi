using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Hosting; 
using System.IO; 
using Microsoft.AspNetCore.Authorization; 

namespace Buddy.API {
    /// <summary>
    /// A Basic SPA controller that serves a HTML file from
    /// within the wwwroot folder, at: wwwroot/dist/index.html
    /// </summary>
    [Route("/")]
    public abstract class SpaController:ControllerBase {
        private readonly IHostingEnvironment _env; 

        public SpaController(IHostingEnvironment env) {
            _env = env; 
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
            var filePath = Path.Combine(_env.WebRootPath, "dist", "index.html");

            if(System.IO.File.Exists(filePath))
            {
                return PhysicalFile(
                    filePath,
                    "text/html"
                );
            }

            throw new System.Exception("No \"wwwroot/dist/index.html\" file found. Did you remember to build the front-end?");
        }
    }

}
