using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectEstrada
{
    partial class MainWindow
    {
        static unsafe byte[] CompileShader(string shaderName)
        {
            var file = StorageFile.GetFileFromPathAsync(GetShaderFilePath(shaderName)).GetAwaiter().GetResult();
            var buffer = FileIO.ReadBufferAsync(file).GetAwaiter().GetResult();
            return buffer.ToArray();
        }

        static unsafe string GetShaderFilePath(string shaderName)
        {
            var appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var shaderFolder = appFolder.GetFolderAsync("Shaders").GetAwaiter().GetResult();
            var file = shaderFolder.GetFileAsync(shaderName + ".hlsl").GetAwaiter().GetResult();
            return file.Path;
        }
    }
}
