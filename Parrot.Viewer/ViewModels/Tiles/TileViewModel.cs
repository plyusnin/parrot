﻿using System.IO;
using System.Windows.Media.Imaging;
using Parrot.Viewer.GallerySources.Exif;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class TileViewModel
    {
        public TileViewModel(ExifInformation Exif, Stream ThumbnailStream)
        {
            this.Exif = Exif;
            Thumbnail = new BitmapImage { CreateOptions = BitmapCreateOptions.DelayCreation };
            Thumbnail.BeginInit();
            Thumbnail.StreamSource = ThumbnailStream;
            Thumbnail.CacheOption = BitmapCacheOption.OnDemand;
            Thumbnail.EndInit();
        }

        public BitmapImage Thumbnail { get; }
        public ExifInformation Exif { get; }
    }
}
