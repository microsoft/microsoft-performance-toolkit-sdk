using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IChecksumProvider<TTarget>
    {
        Task<string> GetChecksumAsync(TTarget path);
    }
}
