// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;
using System.Linq;

namespace SimplePlugin
{
    //
    // This is a sample Processing Source that understands files with the .txt extension
    //

    //
    // In order for a processing source to be recognized, it MUST satisfy the following:
    //  a) Be a public type
    //  b) Have a public parameterless constructor
    //  c) Implement the IProcessingSource interface
    //  d) Be decorated with the ProcessingSourceAttribute attribute
    //  e) Be decorated with at least one of the derivatives of the DataSourceAttribute attribute
    //

    [ProcessingSource(
        "myid01",  // The GUID must be unique for your Processing Source. You can use Visual Studio's Tools -> Create Guid… tool to create a new GUID
        "Simple Data Source",                      // The Processing Source MUST have a name
        "A data source to count words!")]          // The Processing Source MUST have a description
    [FileDataSource(
        ".txt",                                    // A file extension is REQUIRED
        "Text files")]                             // A description is OPTIONAL. The description is what appears in the file open menu to help users understand what the file type actually is. 

    //
    // There are two methods to creating a Processing Source that is recognized by the SDK:
    //    1. Using the helper abstract base classes
    //    2. Implementing the raw interfaces
    // This sample demonstrates method 1 where the CustomDataSourceBase abstract class
    // helps provide a public parameterless constructor and implement the ICustomDataSource interface
    //

    public class WordsProcessingSource
        : ProcessingSource
    {
        private IApplicationEnvironment applicationEnvironment;

        //
        // Provides information about this Processing Source, such as the author,
        // a project link, licensing, etc. This information can be used by tools
        // to display "About" information for your Processing Source. For example,
        // Windows Performance Analyzer (WPA) uses this information for Help->About.
        //

        public override ProcessingSourceInfo GetAboutInfo()
        {
            return new ProcessingSourceInfo
            {
                //
                // The copyright notice for this Processing Source.
                //
                CopyrightNotice = "Copyright 2021 Microsoft Corporation. All Rights Reserved.",

                //
                // The license under which this Processing Source may be used.
                //
                LicenseInfo = new LicenseInfo
                {
                    Name = "MIT",
                    Text = "Please see the link for the full license text.",
                    Uri = "https://github.com/microsoft/microsoft-performance-toolkit-sdk/blob/main/LICENSE.txt",
                },

                //
                // A collection of the people or entities that own this Processing Source.
                //
                Owners = new[]
                {
                    new ContactInfo
                    {
                        Address = "1 Microsoft Way, Redmond, WA 98052",
                        EmailAddresses = new[]
                        {
                            "noreply@microsoft.com",
                        },
                    },
                },

                //
                // Information, if applicable, of where users can find this project.
                //
                ProjectInfo = new ProjectInfo
                {
                    Uri = "https://github.com/microsoft/microsoft-performance-toolkit-sdk",
                },

                //
                // Any additional information you wish your users to know about this Processing Source.
                //
                AdditionalInformation = new[]
                {
                    "This Processing Source is a sample showcasing the Performance Toolkit SDK.",
                }
            };
        }

        protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
        {
            //
            // Saves the given application environment into this instance. This samples does not directly use this value,
            // but it can be used to perform various application-specific actions such as:
            //   - presenting dialog boxes
            //   - refreshing tables
            //   - serializing table configurations
            // See advanced tutorials for more information
            //

            this.applicationEnvironment = applicationEnvironment;
        }

        protected override ICustomDataProcessor CreateProcessorCore(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            //
            // Create a new instance of a class implementing ICustomDataProcessor here to process the specified data sources.
            // Note that you can have more advanced logic here to create different processors if you would like based on the source, or any other criteria.
            // You are not restricted to always returning the same type from this method.
            //

            return new WordsDataProcessorWithSourceParser(
                new WordSourceParser(dataSources.Select(x => x.Uri.LocalPath).ToArray()),
                options,
                this.applicationEnvironment,
                processorEnvironment);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource source)
        {
            //
            // This method is called for every data source which matches the one declared in the DataSource attribute. It may be useful
            // to peek inside the source to truly determine if you can support it, especially if your data processor supports a common
            // filetype like .txt or .csv.
            // For this sample, we'll always return true for simplicity.
            //

            return true;
        }
    }
}
