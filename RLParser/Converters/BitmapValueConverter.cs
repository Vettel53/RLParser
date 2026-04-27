using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;

namespace RLParser.Converters;

public sealed class BitmapValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        if (value is Bitmap bitmap)
        {
            return bitmap;
        }

        if (value is not string source || string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        try
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var absoluteUri))
            {
                if (absoluteUri.Scheme.Equals("avares", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = AssetLoader.Open(absoluteUri);
                    return new Bitmap(stream);
                }

                if (absoluteUri.IsFile)
                {
                    return new Bitmap(absoluteUri.LocalPath);
                }
            }

            if (File.Exists(source))
            {
                return new Bitmap(source);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}