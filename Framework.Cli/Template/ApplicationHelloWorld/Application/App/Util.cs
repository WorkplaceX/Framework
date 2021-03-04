namespace Application.Doc
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    public static class Util
    {
        /// <summary>
        /// Gets Version. This is the application version.
        /// </summary>
        public static string Version
        {
            get
            {
                return $"v1.10 (Framework { Framework.UtilFramework.Version })";
            }
        }

        public static byte[] ImageResize(int width, byte[] data)
        {
            Image image;
            using (var memoryStream = new MemoryStream(data))
            {
                image = Image.FromStream(memoryStream);
            }
            var scale = (double)width / image.Width;
            var widthDest = (int)Math.Round(image.Width * scale);
            var heightDest = (int)Math.Round(image.Height * scale);
            var imageDest = image.GetThumbnailImage(widthDest, heightDest, () => false, IntPtr.Zero);

            using (var memoryStream = new MemoryStream())
            {
                var imageFormat = image.RawFormat; // Original format
                imageFormat = ImageFormat.Jpeg; // Keep size low
                imageDest.Save(memoryStream, image.RawFormat);
                return memoryStream.ToArray();
            }
        }

        public static void Assert(bool isAssert, string exceptionText)
        {
            if (!isAssert)
            {
                throw new Exception(exceptionText);
            }
        }

        public static void Assert(bool isAssert)
        {
            if (!isAssert)
            {
                throw new Exception("Assert!");
            }
        }
    }
}
