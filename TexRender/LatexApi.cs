using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace TexRender
{
    public sealed class LatexApi
    {
        private static readonly HttpClient Client;

        static LatexApi()
        {
            Client = new HttpClient();
        }

        //public static async Task<string> RenderLatexToSvg(string latexString)
        //{
        //    latexString = WebUtility.UrlEncode(latexString);
        //    var response = await Client.GetAsync("https://latex.codecogs.com/svg.latex?" + latexString);
        //    return await response.Content.ReadAsStringAsync();
        //}

        //public static async Task<SvgImageSource> RenderLatexToSvgImage(string latexString)
        //{
        //    latexString = WebUtility.UrlEncode(latexString);
        //    var response = await Client.GetAsync("https://latex.codecogs.com/svg.latex?" + latexString);
        //    var svg = new SvgImageSource();
        //    await svg.SetSourceAsync((await response.Content.ReadAsStreamAsync()).AsRandomAccessStream());
        //    return svg;
        //}

        //public static async Task<byte[]> RenderLatexToPng(string latexString)
        //{
        //    latexString = WebUtility.UrlEncode(latexString);
        //    var response = await Client.GetAsync("https://latex.codecogs.com/png.latex?" + latexString);
        //    var bitmapDecoder = await BitmapDecoder.CreateAsync((await response.Content.ReadAsStreamAsync()).AsRandomAccessStream());
        //    return await response.Content.ReadAsByteArrayAsync();
        //}

        public static string GetFormatString(Format format)
        {
            switch (format)
            {
                case Format.Svg:
                    return "svg";
                case Format.Png:
                    return "png";
                case Format.Gif:
                    return "gif";
                default:
                    return "svg";
            }
        }
    }

    public enum Format
    {
        Svg,
        Png,
        Gif
    }

}
