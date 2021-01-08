# Abstract
This document will cover:
* Adding an AddIn that a SDK will recognize to your project
  * This includes implementing the required interfaces
* Adding a simple table to make sure your AddIn works
* Stepping through your AddIn

A more detailed explanation of concepts about will come in a later section; this section is simply to show what is required to get SDK to understand your AddIn

[[_TOC_]]

# Requirements
You __MUST__ have created an appropriate project before following this document. See [here](/UsingtheSDK/Creatingyourproject) for how to create a project if you have not done so.

# Creating Your First AddIn

There are two methods to creating a Custom Data Source that is recognized by SDK:
1) Using the helper abstract base classes
2) Implementing the raw interfaces

In this document we will use method one (1.) Method two (2) is more advanced and will be outlined in the reference section.

## Creating the Custom Data Source

### Getting Tools to recognize your Custom Data Source

1) Create a new class in your project, and name it something indicative of what it does. For this document, we will create "HelloWorldCustomDataSource"
2) Add `using Microsoft.Performance.SDK` to your using statements.
3) Add `using Microsoft.Performance.SDK.Processing` to your using statements.
4) In order for a Custom Data Source (CDS) to be recognized, it MUST satisfy the following:
  a) Be a __public__ type
  b) Have a __public__ __parameterless__ constructor
  c) Implement the __ICustomDataSource__ interface
  d) Be decorated with the __CustomDataSourceAttribute__ attribute
  e) Be decorated with at least one of the derivatives of the __DataSourceAttribute__ attribute
5) For our purposes, we will use the provided __CustomDataSourceBase__ abstract class, which gives us b) and c). Go ahead and derive from __CustomDataSourceBase__
6) Make sure your class is __public__
7) Right click on your class name (that should have a red squiggly line under it) and select Quick Actions and Refactorings > Implement Abstract class
8) Add the following attribute to your class:
```` cs
[CustomDataSource(
    "{526EF20C-BA9E-4CC3-AEA9-949A6C9F17B9}",
    "HelloWorld",
    "A data source to say hello!")]
````
  a) The GUID must be unique for your Custom Data Source. You can use Visual Studio's Tools > Create Guidâ€¦ tool to create a new GUID
  b) The Custom Data Source __MUST__ have a name
  c) The Custom Data Source __MUST__ have a description
9) Add the following attribute to your class:
```` cs
[FileDataSource(
    ".txt",
    "Text files")]
````
  a) A file extension is __REQUIRED__
  b) A description is __OPTIONAL__. The description is what appears in the file open menu to help users understand what the file type actually is.
10) Delete the `throw new NotImplementedException()` from `SetApplicationEnvironment`.
11) We have configured our class as a Custom Data Source that understands files with the `.txt` extension.
12) Let's make sure that we have configured it correctly. Your class should look something like this:
 ![VS2017_CDS_Definition.PNG](/.attachments/VS2017_CDS_Definition10256d94305848b48ac00a2bf16d527d.PNG)

#### Troubleshooting

If SDK does not recognize your Custom Data Source, check the following:
* Is you Custom Data Source __public__?
* Does your Custom Data Source have the __CustomDataSourceAttribute__ attribute and __FileDataSourceAttribute__ attributes?
* Does you Custom Data Source have a __public__ __parameterless__ constructor?
* If you are not using the targets provided by the SDK, navigate to the instance of SDK that you are using and check the following (if you installed the SDK NuGet package, the following is taken care of for you. For reference, the SDK uses $(OutDir)\WPA_Testing):
  * Is there a __CustomDataSources__ folder next to wpa.exe?
  * Is there a folder under __CustomDataSources__ for your addin?
  * Is your addin in that folder?
![CDS_FS_Tree_Folders.PNG](/.attachments/CDS_FS_Tree_Folders76922c4ab41f4668a5a45e5f461ba7f7.PNG)
![CDS_FS_Tree_Filess.PNG](/.attachments/CDS_FS_Tree_Filess9fcdd4c6a2ea46c0a2537a8b2c15860d.PNG)

Feel free to reach out to the team if you have any issues.

### Making sure you can Debug

In the ideal world, the flow is as follows:

1) Set a breakpoint on the `SetApplicationEnvironment` function.
2) Press F5 to execute to start the testing tool (WPA).
3) The breakpoint should be hit.

## Adding the Processor

Without a processor, your Custom Data Source won't do anything. We will go ahead and add a new class to actually process our files.

__SIDEBAR__ What is the difference between a _Custom Data Source_ and a _Custom Data Processor_? The _Custom Data Source_ is effectively the factory for _Custom Data Processors_. Only one instance is ever created. The _Custom Data Source_ controls what data is able to be processed, and facilitates creating processors to process data. The _Custom Data Processors_ are created for every file. If you open a file, a processor is created to process it. If you open another file later, another processor will be created.

1) Create new class in your project and name it "HelloWorldCustomDataProcessor"
2) Add `using Microsoft.Performance.SDK` to your using statements.
3) Add `using Microsoft.Performance.SDK.Processing` to your using statements.
4) Inherit from the __CustomDataProcessorBase__ class.
  a) Follow a similar process for implementing your CustomDataSource to implement the abstract class. Make sure to implement the correct constructor and methods.
5) Because we are just processing simple text files, go ahead and modify the constructor of your class to take a collection of file names. The IDataSources that you will have expose Uri objects, but we are processing text files on the file system. An array of string will work just fine.
![VS2017_CDP_Definition.PNG](/.attachments/VS2017_CDP_Definition4b14255b3c094a7384f7898ed5af3b3b.PNG)
6) Navigate back to your Custom Data Source class. Modify SetApplicationEnvironment to store the application environment, and modify CreateProcessorCore to return a new instance of your processor. When you have finished, it should look something like this:
![VS2017_CDS_Implementation.PNG](/.attachments/VS2017_CDS_Implementationbeee912566344e468b5936415a13c088.PNG)
7) Go ahead and press F5 to run your project to make sure that your AddIn still loads correctly.

Note that you can have more advanced logic in your CreateProcessorCore method to create different processors if you would like based on the file, or any other criteria. You are not restricted to always returning the same type from this method.

## Tables

At this point, we have implemented our Custom Data Source, and have started to implement our Custom Data Processor. We are now going to create two tables: a metadata table and a regular table.

A table is just a logical collection of similar data points. Things like CPU Usage, Thread Lifetimes, File I/O, etc. are all tables.

Your data processor is responsible for instantiating the proper tables based on what the user has decided to enable. There is no explicit table interface so as to give you flexibility in how you implement your tables. All that matters is that you have some way of getting the data out of the data files and into the __ITableBuilder__ in CreateTable in order for SDK to understand your data.

As we are using CustomDataSourceBase, we have some helper attributes to allow automatic discovery of our tables. Go ahead and create a "Tables" folder in your project, and a "Metadata" folder under that. You should end up with something like this:
![VS2017_ProjectLayout_Adding_Tables_01.PNG](/.attachments/VS2017_ProjectLayout_Adding_Tables_01ea0a630e44f643e0ba8fd429883565f2.PNG)

Let us also go ahead and create an interface to use for all of our tables to simplify management of them. Add a `TableBase` abstract class in the tables folder. All of our tables will need some way to build themselves via the __ITableBuilder__ interface, and access to the underlying data in the files. Remember that we potentially have multiple files.

For the purposes of this tutorial, we are going to assume the files will fit in memory, and so we will make sure all tables have access to the collection of lines in the file.

This suggests the following class:
![VS2017_TableBase_01.PNG](/.attachments/VS2017_TableBase_014b53bf41c5744c42bd3807f18824d96c.PNG)

All of our regular and metadata tables will derive from this class.

### Processing the data into a format for our tables.

Because our tables will operate on a collection of lines in the file, we will need to process our file in order to create the necessary shape. Open your Custom Data Processor file and go to the constructor.

1) Assign the files array to a readonly backing field.
2) Create another backing field, called fileContents, of type `IReadOnlyDictionary<string, IReadOnlyList<string>>`
3) We are going to read all of the lines of the file into this dictionary in ProcessAsync.
4) Go to the ProcessAsync method.
5) Read all of the file content into the dictionary:
![VS2017_CDP_ImplementProcessAsync.PNG](/.attachments/VS2017_CDP_ImplementProcessAsync19c640523da84e60838e8393a041c39a.PNG)
6) We do not have anything asynchronous, so returning Task.CompletedTask is fine.

__NOTE__: if you must do processing based on which tables are enabled, you would check the _EnabledTables_ property on your class to see what you should do. For example, the XPerf/ETW AddIn looks at what tables are enabled in order to turn on additional event processors to avoid processing everything if it doesn't have to.

__NOTE__: ProcessAsync is also where you would determine the information necessary for GetDataSourceInfo(). In this sample, there is nothing.

Now, we will go ahead and implement GetDataSourceInfo as it is required. The __DataSourceInfo__ is used to tell SDK the time range of the data (if applicable) and any other relevant data for rendering / synchronizing.  In our case, we have nothing, so just return `new DataSourceInfo(0, long.MaxValue)` to tell SDK that it encompasses all time.

Let us now create a table, and then outline how to instantiate it.

### Metadata Tables

Metadata tables are used to expose information _about_ the data being processed, not the actual data being processed. Metadata would be something like "number of events in the file" or "file size" or any other number of things that describes the data being processed.

For our purposes, we will expose the number of lines in the file and the word count of the file as metadata.

1) In the metadata folder, create a new table called FileStatsMetadataTable.
2) Inherit from our TableBase class.
3) In order for the CustomDataSourceBase to understand our metadata table, add the following attribute:
```` cs
[MetadataTable(  
    "{791A070D5E4A4E19AF85D07B94069CCF}",  
    "File Stats",  
    "Statistics for text files")]
````
  a) The GUID must be unique across all tables.
  b) This attribute denotes the table as metadata.
4) We will now declare our columns on our table. We can do this using the ColumnConfiguration class. We need this class when we add columns to our table, so we will expose them as fields for easy reference.
  a) It is possible to declaratively describe the table configuration as well. This is an advanced topic handled in a later section.
5) Your class should look like this so far:
 ![VS2017_ImplemtingMetadata_01.PNG](/.attachments/VS2017_ImplemtingMetadata_0103379f203d524f40a37b0dd6187c45a9.PNG)
  a) The Column metadata describes each column in the table. Each column must have a unique GUID and a unique name. The GUID must be unique globally; the name only unique within the table.
  b) The UIHints provides some hints to SDK on how to render the column. For now, we are simply saying to allocate at least 80 units of width.
6) Now we are going to implement our columns. Columns are implemented via __Projections__, which are simply functions that map a _row index_ to a data point.
7) Let's start with the file name column. We have access to a dictionary mapping the file name to the lines in the file, so our projection is simply a map of an index into the dictionary key collection. We will go ahead and enumerate the names into an array so that we can have consistent name lookup by index:
`var fileNames = this.Lines.Keys.ToArray();`
`var fileNameProjection = Projection.Index(fileNames.AsReadOnly());`

8) Now lets add a line count column. This is simply a projection of the size of the list containing the files lines. We will __compose__ our file name projection with another projection that grabs the count of the lines in the file:
`var lineCountProjection = fileNameProjection.Compose(x => this.Lines[x].Count);`

9) Finally, lets create a word count projection. This is the sum of all words in all of the lines. We can do this by composing with a split on whitespace, and summing the results. This can be expensive, so let's use a cached projection that caches the results:
```` cs
var wordCountProjection = Projection.CacheOnFirstUse(
    fileNames.Length,
    fileNameProjection.Compose(x => this.Lines[x].SelectMany(y => y.Split(null)).Count()));
````

10) We can now use the table builder to build our table. Simply set the row count (we have one row per file) and then add the columns using AddColumn.
11) You final Build method should look something like this:
![VS2017_Metadata_Build_Complete.PNG](/.attachments/VS2017_Metadata_Build_Complete6c8f38bac32e4fa3a839534b8395a590.PNG)

12) Now we need to go back to our Custom Data Processor and wire up table instantiation. 

### Table Instantiation

1) Go back to your Custom Data Processor and find the BuildTableCore method.
2) We know all of our tables have the following constructor (by our project convention):
`<constructor>(IReadOnlyDictionary<string, IReadOnlyList<string>>)`
3) Let's add a private method called _InstantiateTable_ that activates our table type and passes in the file content:
4) Now in BuildTableCore, we can simply instantiate the table, and pass our tableBuilder to it.
![VS2017_TableInstantiation.PNG](/.attachments/VS2017_TableInstantiationefbd740313fb4b18ab88b018f2fe0160.PNG)

### Testing our table

1) Build and press F5 to launch your AddIn in SDK.
2) Using File > Open, open a .txt file.
3) SDK will display a progress dialog, and then show the analysis view. We have no data tables, so the graph explorer will be empty. However, notice that there is now a _Metadata_ file item at the top. Expand the drop down, and you will see the name of your data source, in this case, _Hello World_. Click it.
4) Your metadata table will pop up. Expand it and you will see that our file name, line count, and word count are displayed in the table.
![WPA_MetadataMenu.PNG](/.attachments/WPA_MetadataMenu6ac194c95da84d279ba27f9e7b6e56e1.PNG)
![WPA_MetadataTable.PNG](/.attachments/WPA_MetadataTablea812a8c08fc14215be69d68c1b9adc78.PNG)
5) Close SDK

### Data Tables

Now, lets add a regular data table to SDK. Just like for the metadata table, we will add a new class, derive from TableBase, and implement. This time, put your new table in the _Tables_ folder. We will have this table count the characters in each word. This will also give us a column (word) that we can group on.

1) Create a new class in the _Tables_ folder called "WordTable".
2) Inherit the TableBase class and implement.
3) This time, instead of MetadataTable, just use the "Table" attribute.
  a) Don't forget to generate a new GUID for this table!
4) We are going to begin grouping our tables by categories, so go ahead and use the Category property on the attribute to give it the `Cateogry = "Words"`
5) You should have something like this:
![VS2017_DataTable_01.PNG](/.attachments/VS2017_DataTable_01f5da790061244346ad813674b6bac0b1.PNG)
6) We are going to create three columns: one for the file,  one for the word itself, and one for the number of characters. As an exercise, go ahead and declare the column configurations and implement the projections, culminating with an implementation of Build.
7) You might have come up with something like this:
![VS2017_DataTable_02.PNG](/.attachments/VS2017_DataTable_026fa19912e2ea459db092805258eb8aae.PNG)

Now let's test our table. Open SDK again and open a text file. You should see the graph explorer populate with a "Word" category, and your new table. Drag the table over into the analysis view, and you will see the columns. Experiment with moving the gold bar around and grouping / graphing.
![WPA_DataTable_01.PNG](/.attachments/WPA_DataTable_01a3edf11e5af74d0cb2187ac4cd1b7e7f.PNG)

### Bonus: Progress bar

You may have notices the IProgress interface in the ProcessAsync method. This interface allows for you to report the progress of your processing back to SDK. For this sample, we will report progress as a percentage of files read. Progress takes an integer between 0 and 100, inclusive.

1) In ProcessAsync, get the count of files
2) For every file, call progress.
3) Run SDK, and open multiple text files at once in the File>Open menu. You should see the progress bar fill before opening showing the tables. If the files are really small, it may go too fast.
![VS2017_CDP_Progress_01.PNG](/.attachments/VS2017_CDP_Progress_01b1c6252f7bca4bdcb3e77f10c761d5fd.PNG)
![WPA_OpenMultipleFiles.PNG](/.attachments/WPA_OpenMultipleFiles5b6531ce847446099b1b12039a64cb44.PNG)

# Conclusion

At this point, you have what you need to start creating tables for your data in SDK.
Additional sections will cover
* Setting the default graphing behavior
* Defining configurations for your tables
* Using SDK to open multiple files of different types
* Using SDK from the command line