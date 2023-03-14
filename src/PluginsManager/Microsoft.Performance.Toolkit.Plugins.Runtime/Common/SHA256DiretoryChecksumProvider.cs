using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public class SHA256DiretoryChecksumProvider
        : IChecksumProvider<DirectoryInfo>
    {
        public async Task<string> GetChecksumAsync(DirectoryInfo dir)
        {
            return await HashUtils.GetDirectoryHashAsync(dir.FullName);
        }
    }
}
