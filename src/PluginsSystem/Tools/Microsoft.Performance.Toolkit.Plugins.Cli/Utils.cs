
namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public static class Utils
    {
        public static string GetValidDestFileName(string file)
        {
            string? directory = Path.GetDirectoryName(file);
            string name = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);

            string destFileName = file;

            int fileCount = 1;
            while (File.Exists(destFileName))
            {
                destFileName = Path.Combine(directory!, $"{name}_({fileCount++}){extension}");
            }

            return destFileName;
        }
    }
}
