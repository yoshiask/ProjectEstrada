using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;

namespace ProjectEstrada
{
    partial class MainWindow
    {
        static byte[] CompileShader(string shaderName)
        {
            var file = StorageFile.GetFileFromPathAsync(GetShaderFilePath(shaderName)).GetAwaiter().GetResult();
            var buffer = FileIO.ReadBufferAsync(file).GetAwaiter().GetResult();
            return buffer.ToArray();
        }

        static string GetShaderFilePath(string shaderName)
        {
            var appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var shaderFolder = appFolder.GetFolderAsync("Shaders").GetAwaiter().GetResult();
            var file = shaderFolder.GetFileAsync(shaderName + ".hlsl").GetAwaiter().GetResult();
            return file.Path;
        }
    }
}
