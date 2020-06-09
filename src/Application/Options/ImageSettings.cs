using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;

namespace IdeaShare.Application.Options
{
    public sealed class ImageSettings
    {
        public ImageOptions ProfilePicture { get; set; }
        public ImageOptions FeaturedImage { get; set; }
    }

    public sealed class ImageOptions
    {
        // Set to -1 to disable resizing.
        public int Height { get; set; }

        // Set to -1 to disable resizing.
        public int Width { get; set; }

        public ImageFormat Format { get; set; }

        public string Location { get; set; }
    }
}
