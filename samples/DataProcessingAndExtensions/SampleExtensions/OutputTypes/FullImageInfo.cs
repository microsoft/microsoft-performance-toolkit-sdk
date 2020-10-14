using DataExtensionsSample.OutputTypes;

namespace SampleExtensions.OutputTypes
{
    public sealed class FullImageInfo
    {
        public FullImageInfo(ProcessInfo processInfo, ImageLoadInfo imageLoadInfo)
        {
            ProcessInfo = processInfo;
            ImageLoadInfo = imageLoadInfo;
        }

        public ProcessInfo ProcessInfo { get; }

        public ImageLoadInfo ImageLoadInfo { get; }

        public string GetProcessName(ProcessImage image)
        {
            var processActivity = this.ProcessInfo.FindProcess(image.ProcessId, image.LoadTime);
            if(processActivity.HasValue)
            {
                return processActivity.Value.ProcessName;
            }

            return "N/A";
        }
    }
}
