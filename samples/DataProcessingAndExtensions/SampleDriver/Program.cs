using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.Toolkit.Engine;
using SampleExtensions.CompositeCookers;
using SampleExtensions.OutputTypes;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace SampleDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Sample *.des file required");
                return;
            }

            var sampleFile = args[0];

            //
            // Create our runtime environment, enabling cookers and
            // adding inputs.
            //

            var runtime = Engine.Create(
                new EngineCreateInfo
                {
                    //
                    // Set this to from where you want to load your
                    // addins. The SDK by default will deploy your project,
                    // which is why we used that here. Production applications
                    // will more than likely use a different location (or
                    // a location specified by the user.)
                    //

                    ExtensionDirectory = Path.Combine(
                        Environment.CurrentDirectory,
                        Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase))
                });

            runtime.AddFile(sampleFile);

            //
            // Enable the cooker to data processing
            //

            runtime.EnableCooker(ImagesWithProcessInfo.CookerPath);

            //
            // Process our data.
            //

            var results = runtime.Process();

            //
            // Access our cooked data.
            //

            var data = results.QueryOutput<FullImageInfo>(new DataOutputPath(ImagesWithProcessInfo.CookerPath, nameof(ImagesWithProcessInfo.ImageInfoData)));

            Console.WriteLine("Image Path, Address, Size, Process ID, Load Time, Unload Time, Process Name");
            
            foreach(var image in data.ImageLoadInfo.Images)
            {
                Console.WriteLine($"{image.Image.Path}, {image.LoadAddress}, {image.Image.ImageSize}, {image.ProcessId}, {image.LoadTime}, {image.UnloadTime}, {data.GetProcessName(image)}");
            }
        }
    }
}
