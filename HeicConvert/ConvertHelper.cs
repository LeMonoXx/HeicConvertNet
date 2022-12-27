using ImageMagick;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace HeicConvert;

public static class ConvertHelper
{
    public static IObservable<int> ConvertAll(string[] allfiles, MagickFormat targetFormat = MagickFormat.Jpeg)
    {
        if(allfiles == null || allfiles.Length == 0)
            return Observable.Empty<int>();

        var directory = new FileInfo(allfiles[0]).Directory?.FullName;

        if(directory == null)
            return Observable.Empty<int>();

        if (!Directory.Exists(Path.Combine(directory, "converted")))
        {
            Directory.CreateDirectory(Path.Combine(directory, "converted"));
        }

        var i = 0;

        return Observable.Create(
        (IObserver<int> observer) =>
        {
            foreach (var file in allfiles)
            {
                i++;
                var progress = (int)(((double)i / (double)allfiles.Length) * 100);
                var info = new FileInfo(file);
                using (var image = new MagickImage(info.FullName))
                {
                    image.Format = targetFormat;
                    image.Write(Path.Combine(directory, "converted", @$"{info.Name}.jpg"));
                    observer.OnNext(progress);
                }
            }
            observer.OnCompleted();
            return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
        });
    }
}

