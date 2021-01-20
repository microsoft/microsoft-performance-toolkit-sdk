# The Custom Data Source Model


** INSERT IMAGES AND CLEAN UP LANGUAGE ** 

link to sample folder / real world maybe (LTTng)  ? 


# CUstom Data Source Model

So each custom data source from an implementation, or sorry from an interface perspective, has to do two things. The first thing it does is advertise the data sources or file types that the data source to custom data source contains logic for, and then it needs to implement the logic for creating tables from those data sources. So it's like like we saw before the extra custom data source says it processes ETL's and it can create the tables.
Or you feel file.
The bare basics you only need to utilize the SDK.
And Leslie, each custom data sources dynamically loaded at runtime by the SDK driver. So the piece example WPA.
And a little bit terminology. One or more of these custom data sources make up a plugin and plugins are compiled into binaries which later get loaded again dynamically into the SDK.
And this is really mission accomplished, because this means that plugins allow analysts to utilize SDK. Sorry to utilized MPA for arbitrary data sources by making custom data source is specific to the files or data sources they want to process. So yeah, mission accomplished.
OK, so now I want to go into a little bit of how do you make one of these custom data sources and we're going to look at this. I'm at a high level and will deep dive into a real live example.



# Custom Data Source Important Functions

So the entry point for every custom data source is going to be a class that implements the custom data source interface. One week implement interface is to extend the custom data source based abstract class at the SDK provides. Additionally, if you do one more thing which is to decorate your class with the custom data source attribute.
And then if you're custom data source is meant to open files, you decorate it with the file data source attribute, specifying the file type that it supports.
Then from the interface perspective, your custom data source needs to implement two methods. The first is. This is file supported method, which will take in a path for a file that has the extension you support and you need to return whether or not you actually can support that file. For proprietary format you can probably just return true here, but for example, if you're if you're saying you can process adapt file well, you might want to look at the first few lines and make sure the file is actually something you know how to deal with.
The second method is the create processor core method and this needs to return to class that implements the I custom Data processor interface. And as you might expect from the name, what the custom data processor does is. It contains all the logic for actually dealing with your data source and making tables.
So in this example here returning a new instance of what I named my data processor. So now we'll take a look at how do we make these data processors?



Each CustomDataSource
	Advertises the file types (data sources) for which it contains logic
	Implements logic for creating tables from opened files (data sources)
	Is created entirely using the Performance Toolkit SDK
	Is dynamically loaded at runtime by the SDK driver (WPA)
One or more CDS constitute a plugin
Plugins are compiled into binaries which later get loaded into the SDK
Plugins allow analysts to utilize WPA for arbitrary data sources. Mission accomplished!
