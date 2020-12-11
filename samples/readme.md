# Plug-in Creation for Performance SDK

## Implement a Simple Data Source

Sample source code: ./SimpleDataSource

To create a simple data source, perform the following:

1) Create a public class that implements the abstract class CustomDataSourceBase.
2) Create a public class that implements the abstract class CustomDataProcessorBase.
3) Create one or more data tables classes. These classes must:
   - Be public and static.
   - Be decorated with TableAttribute.
   - Expose a static public field or property named TableDescriptor of type TableDescriptor which provides information about the table.

The custom data source class will create a new instance of the custom data processor class when ProcessAsyncCore is called.
The custom data processor will a data source when ProcessAsyncCore is called.
Finally, the custom data processor class will create instances of the tables when BuildTableCore is called.