using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageRecognition.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageRecognition.Frontend.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ImageRecognitionManager _imageRecognitionManager;

        public IList<Album> Albums { get; set; }

        public IndexModel(ImageRecognitionManager imageRecognitionManager)
        {
            this._imageRecognitionManager = imageRecognitionManager;
        }

        public async Task OnGet()
        {
            Albums = await _imageRecognitionManager.GetAlbums(this.HttpContext.User.Identity.Name);
        }
    }
}
