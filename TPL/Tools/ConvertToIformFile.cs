using Microsoft.Extensions.FileProviders;

namespace TPLWeb.Tools
{
    public class ConvertToIformFile
    {
        public IFormFile ConvertToIFormFile(string filePath)
        {
            var fileProvider = new PhysicalFileProvider(Path.GetDirectoryName(filePath)!);
            var fileInfo = fileProvider.GetFileInfo(Path.GetFileName(filePath));

            var stream = fileInfo.CreateReadStream();
            var formFile = new FormFile(stream, 0, stream.Length, null!, fileInfo.Name);

            return formFile;
        }
    }
}