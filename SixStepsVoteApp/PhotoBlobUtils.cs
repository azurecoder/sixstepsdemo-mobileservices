using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SixStepsVoteApp
{
    public class PhotoBlobUtils
    {
        private readonly string _filename;
        public const string BaseUri = "https://elastastorage.blob.core.windows.net/eventphotos/";

        public PhotoBlobUtils(string filename)
        {
            _filename = filename;
        }

        public ImageSource GetPhotoImage()
        {
            string filePath = null;
            ImageSource sauce;
            filePath = !_filename.Contains(BaseUri) ? String.Concat(BaseUri, _filename) :_filename;

            try
            {
                sauce = new BitmapImage(new Uri(filePath));
            }
            catch(Exception ex)
            {
                throw new FileNotFoundException(ex.Message);
            }

            return sauce;
        }
    }
}
