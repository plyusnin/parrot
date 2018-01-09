using System;
using ExifLib;

namespace Parrot.Viewer.GallerySources.Exif
{
    public static class ExifReaderHelper
    {
        public static object GetTagValueOrDefault(this ExifReader Reader, ExifTags Tag, object DefaultValue = null)
        {
            return Reader.GetTagValue(Tag, out object val) ? val : DefaultValue;
        }

        public static TValue GetTagValueOrDefault<TValue>(this ExifReader Reader, ExifTags Tag, TValue DefaultValue = default(TValue))
        {
            return Reader.GetTagValue(Tag, out TValue val) ? val : DefaultValue;
        }

        public static TOut GetTagValueOrDefault<TIn, TOut>(this ExifReader Reader, ExifTags Tag, Func<TIn, TOut> Converter, TOut DefaultValue = default(TOut))
        {
            return Reader.GetTagValue(Tag, out TIn val) ? Converter(val) : DefaultValue;
        }
    }
}
