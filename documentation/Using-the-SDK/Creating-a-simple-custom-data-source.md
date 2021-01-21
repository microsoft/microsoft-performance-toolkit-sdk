# Creating a Simple Custom Data Source

A Custom Data Source (CDS) uses the SDK to create a data processing pipeline for any data sources, typically files, 
that the CDS supports. Notably, a CDS acts as the entry point for the SDK runtime to discover and utilizes these data 
pipelines. An SDK Plugin may contain more than one CDS.

A standard CDS will utilize a custom data processor (CDP) to process the data sources that the SDK runtime provides it.  

A plugin may also define tables that get utilized by viewers such as Windows Performance Analyzer (WPA) to display the 
processed data in an organized, interactable collection of rows. Tables are discovered and passed into CDPs by the SDK 
runtime.

The SDK also supports advanced data processing pipelines through data cookers and extensions, but these topics are 
covered in the advanced documentaton. Refer to [Overview](../Overview.md) for more information.

## Corresponding Sample

Refer to /samples/SimpleDataSource for source code that implements the steps outlined in this file.

## Requirements of a Simple Data Source

The following are required for a Simple Data Source:

1. Create a public class that extends the abstract class `CustomDataSourceBase`. This is your custom data source (CDS).
2. Create a public class that extends the abstract class `CustomDataProcessorBase`. This your custom data processor 
   (CDP).
3. Create one or more data tables classes. These classes must:
   - Be public and static.
   - Be decorated with `TableAttribute`.
   - Expose a static public field or property named TableDescriptor of type `TableDescriptor` which provides information
     about the table. If these requirements are not met, the SDK runtime will not be able to find and pass your tables 
     to your CDP.

## Implementing a Custom Data Source Class

1. Create a public class that extends the abstract class `CustomDataSourceBase`. Note that the class is decorated with 
   two attributes: `CustomDataSourceAttribute` and `FileDataSourceAttribute`. The former is used by the SDK runtime to 
   locate Custom Data Sources. The latter identifies a type of data source that the CDS can consume.

   ```
   [CustomDataSource(
      "{F73EACD4-1AE9-4844-80B9-EB77396781D1}",  // The GUID must be unique for your Custom Data Source. You can use 
                                                // Visual Studio's Tools -> Create Guid… tool to create a new GUID
      "Simple Data Source",                      // The Custom Data Source MUST have a name
      "A data source to count words!")]          // The Custom Data Source MUST have a description
   [FileDataSource(
      ".txt",                                    // A file extension is REQUIRED
      "Text files")]                             // A description is OPTIONAL. The description is what appears in the 
                                                // file open menu to help users understand what the file type actually 
                                                // is. 
   public class SimpleCustomDataSource : CustomDataSourceBase
   {
   }
   ```

2. Overwrite the SetApplicationEnvironmentCore method. The IApplicationEnvironment parameter is stored in the base 
   class' ApplicationEnvironment property.  
   **Note**: we intend to make this method virtual in the base class in the future as nothing needs to be done here

   ```
   protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
   {
   }
   ```

3. Overwrite the CreateProcessorCore method. When the SDK needs to process files your CDS supports, it will obtain an 
   instance of your CDP by calling this method. This is also where your CDS learns about the files, passed as `IDataSource`s, that it will need to process.

   ```
   protected override ICustomDataProcessor CreateProcessorCore(
      IEnumerable<IDataSource> dataSources,
      IProcessorEnvironment processorEnvironment,
      ProcessorOptions options)
   {
      //
      // Create a new instance of a class implementing ICustomDataProcessor here to process the specified data 
      // sources.
      // Note that you can have more advanced logic here to create different processors if you would like based 
      // on the file, or any other criteria.
      // You are not restricted to always returning the same type from this method.
      //

      return new SimpleCustomDataProcessor(
            dataSources.Select(x => x.GetUri().LocalPath).ToArray(),
            options,
            this.applicationEnvironment,
            processorEnvironment,
            this.AllTables,
            this.MetadataTables);
   }
   ```
4. Overwrite the IsFileSupportedCore method. This is where your class will determine if a given file contains data 
   appropriate to your CDS. For example, if your CDS consumes .xml files, not all .xml files will be valid for your
   CDS. Use this method as an opportunity to filter out the files that aren't consumable by this CDS.  
   :warning:: we intend to change this method before 1.0 release. more details to follow.
   ```
   protected override bool IsFileSupportedCore(string path)
   {
      //
      // This method is called for every file whose filetype matches the one declared in the FileDataSource attribute. It may be useful
      // to peek inside the file to truly determine if you can support it, especially if your CDS supports a common
      // filetype like .txt or .csv.
      // For this sample, we'll always return true for simplicity.
      //

      return true;
   }
   ```

When it is time to process the given `IDataSource`s, the SDK will call `ProcessAsyncCore` on the instance of your CDP 
returned by your CDS's constructor.

Sometime after `ProcessAsyncCore` finishes (at least for typical SDK runtime consumers who ask the SDK to build tables 
only *after* asking it to process sources), the SDK will call `BuildTableCore` on your CDP for each table "hooked up" 
to it. This method receives the `TableDescriptor` of the table it must create, and is responsible for populating the 
`ITableBuilder` with the 
columns that make up the table being constructed. This is most easily done by delegating the work of populating the `ITableBuilder` to an 
instance of the class/table described by the `TableDescriptor`.

Note that tables are "hooked up" to CDPs automatically by the SDK. Any class decorated with the `TableAttribute` that has a public static
`TableDescriptor` field/property is automatically hooked up to any CDPs declared in that table's assembly. There are ways to 
explicitly enumerate the CDPs that a table gets hooked up to, but this will be covered in the advanced tutorials (still to come!).