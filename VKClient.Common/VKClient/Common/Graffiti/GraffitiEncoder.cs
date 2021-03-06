using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VKClient.Common.Framework;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace VKClient.Common.Graffiti
{
  public class GraffitiEncoder
  {
    private const int RENDER_IMAGE_DPI = 72;
    private readonly WriteableBitmap _bitmap;

    public GraffitiEncoder(WriteableBitmap bitmap)
    {
      this._bitmap = bitmap;
    }

    public async Task<Stream> Encode()
    {
      try
      {
        Stopwatch stopwatchEncode = new Stopwatch();
        stopwatchEncode.Start();
        IRandomAccessStream imageStream = (IRandomAccessStream) new InMemoryRandomAccessStream();
        BitmapEncoder bitmapEncoder = await GraffitiEncoder.BuildEncoder(imageStream);
        BitmapPixelFormat pixelFormat;
        byte[] imageBinaryData1 = GraffitiEncoder.GetImageBinaryData1(this._bitmap, out pixelFormat);
        int num1 = (int) pixelFormat;
        int num2 = 0;
        int pixelWidth = this._bitmap.PixelWidth;
        int pixelHeight = this._bitmap.PixelHeight;
        double dpiX = 72.0;
        double dpiY = 72.0;
        byte[] pixels = imageBinaryData1;
        bitmapEncoder.SetPixelData((BitmapPixelFormat) num1, (BitmapAlphaMode) num2, (uint) pixelWidth, (uint) pixelHeight, dpiX, dpiY, pixels);
        await bitmapEncoder.FlushAsync().AsTask().ConfigureAwait(false);
        long num3 = (long) imageStream.Size;
        stopwatchEncode.Stop();
        Execute.ExecuteOnUIThread((Action) (() => {}));
        return imageStream.AsStreamForRead();
      }
      catch
      {
        return (Stream) null;
      }
    }

    private static async Task<BitmapEncoder> BuildEncoder(IRandomAccessStream outputStream)
    {
      return await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream).AsTask<BitmapEncoder>().ConfigureAwait(false);
    }

    private static byte[] GetImageBinaryData1(WriteableBitmap bitmap, out BitmapPixelFormat pixelFormat)
    {
      pixelFormat = BitmapPixelFormat.Bgra8;
      return bitmap.ToByteArray();
    }

    private static byte[] GetImageBinaryData2(WriteableBitmap bitmap, out BitmapPixelFormat pixelFormat)
    {
      pixelFormat = BitmapPixelFormat.Rgba8;
      int pixelWidth = bitmap.PixelWidth;
      int pixelHeight = bitmap.PixelHeight;
      int[] pixels = bitmap.Pixels;
      byte[] numArray = new byte[4 * pixelWidth * pixelHeight];
      int index1 = 0;
      int index2 = 0;
      while (index1 < pixels.Length)
      {
        int num = pixels[index1];
        numArray[index2] = (byte) (num >> 16);
        numArray[index2 + 1] = (byte) (num >> 8);
        numArray[index2 + 2] = (byte) num;
        numArray[index2 + 3] = (byte) (num >> 24);
        ++index1;
        index2 += 4;
      }
      return numArray;
    }
  }
}
