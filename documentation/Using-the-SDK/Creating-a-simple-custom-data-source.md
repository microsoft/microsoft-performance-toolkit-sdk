# Creating a Simple Custom Data Source

TODO: make this file more in-depth

An SDK plugin consists of one or more custom data sources (CDSs). Each CDS utilizes the SDK 
to create a data processing pipeline for any files that the CDS supports. It also acts as the 
entry point for the SDK to discover and utilizes these data pipelines. A standard CDS will utilize 
a custom data processor (CDP) to process the data sources (typically files) that the SDK runtime provides it.
A plugin may also define tables that get utilized by viewers such as Windows Performance Analyzer (WPA) 
to display the processed data in an organized, interactable collection of rows. Tables are discovered 
and passed into CDPs by the SDK runtime.

The SDK also supports advanced data processing pipelines through data cookers and extensions, 
but these topics will be covered in the advanced tutorials (still to come!).

## Corresponding Sample
Refer to [/samples/SimpleDataSource](../../samples/SimpleDataSource) for source code that implements 
the steps outlined in this file.

## Implement a Simple Data Source

To create a simple data source, perform the following:

1) Create a public class that extends the abstract class `CustomDataSourceBase`. This is your custom data source (CDS).
2) Create a public class that extends the abstract class `CustomDataProcessorBase`. This your custom data processor (CDP).
3) Create one or more data tables classes. These classes must:
   - Be public and static.
   - Be decorated with `TableAttribute`.
   - Expose a static public field or property named TableDescriptor of type `TableDescriptor` which provides information about the table.
   If these requirements are not met, the SDK runtime will not be able to find and pass your tables to your CDP.

When the SDK needs to process files your CDS supports, it will obtain an instance of your CDP through 
your CDS's constructor. This is also where your CDS learns about the files, passed as `IDataSource`s, that it will need to process. 
So, your CDS must do two things in its constructor:
1) Return an instance of your CDP.
2) Note the `IDataSource`s it receives in such a way that the CDP can later process them. This is most easily done by 
   passing the `IDataSource`s into your CDP's constructor, where it gets stored as a field.

When it is time to process the given `IDataSource`s, the SDK will call `ProcessAsyncCore` on the instance of your CDP 
returned by your CDS's constructor.

Sometime after `ProcessAsyncCore` finishes (at least for typical SDK runtime consumers who ask the SDK to build tables only *after* asking it to process sources), the SDK will call `BuildTableCore` on your CDP for each table "hooked up" to it. This
method receives the `TableDescriptor` of the table it must create, and is responsible for populating the `ITableBuilder` with the 
columns that make up the table being constructed. This is most easily done by delegating the work of populating the `ITableBuilder` to an 
instance of the class/table described by the `TableDescriptor`.

Note that tables are "hooked up" to CDPs automatically by the SDK. Any class decorated with the `TableAttribute` that has a public static
`TableDescriptor` field/property is automatically hooked up to any CDPs declared in that table's assembly. There are ways to 
explicitly enumerate the CDPs that a table gets hooked up to, but this will be covered in the advanced tutorials (still to come!).