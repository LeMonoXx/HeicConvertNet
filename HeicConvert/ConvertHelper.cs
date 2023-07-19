using ImageMagick;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeicConvert;

public static class ConvertHelper
{
    private static readonly ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 1.0))
        };

    public static IObservable<int> ConvertAll(string[] allfiles, MagickFormat targetFormat = MagickFormat.Jpeg, bool deleteOriginalImages = true)
    {
        if(allfiles == null || allfiles.Length == 0)
            return Observable.Empty<int>();

        var directory = new FileInfo(allfiles[0]).Directory?.FullName;

        if(directory == null)
            return Observable.Empty<int>();

        if (!deleteOriginalImages && !Directory.Exists(Path.Combine(directory, "converted")))
        {
            Directory.CreateDirectory(Path.Combine(directory, "converted"));
        }

        long i = 0;

        return Observable.Create(
        (IObserver<int> observer) =>
        {
            Parallel.ForEach(allfiles, options, (file) =>
            {
                Interlocked.Increment(ref i);

                var info = new FileInfo(file);

                if (!info.Exists)
                    return;

                using (var image = new MagickImage(info.FullName))
                {
                    image.Format = targetFormat;
                    var imagePath = string.Empty;
                    var newImageName = @$"{Path.GetFileNameWithoutExtension(info.Name)}.{targetFormat.ToString().ToLower()}";
                    
                    if(!deleteOriginalImages)
                        imagePath = Path.Combine(directory, "converted", newImageName);
                    else
                        imagePath = Path.Combine(directory, newImageName);

                    image.Write(imagePath);

                    var ilI = Interlocked.Read(ref i);
                    int progress = (int)(((double)ilI / (double)allfiles.Length) * 100);
                    observer.OnNext((int)progress);
                }

                info.Delete();
            });

            observer.OnCompleted();
            return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
        });
    }
}

