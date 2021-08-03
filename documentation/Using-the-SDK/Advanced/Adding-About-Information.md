# Abstract

This document outlines how to add about/author information to your plugins.

# Motivation

When a plugin is used by a large and/or public audience, it is useful to 
give users an easy way to contact its owners/maintainers. SDK Drivers such as 
WPA look for __about information__ on a `ProcessingSource` for it to  
present to users.

# Implementation

To add about information to your `ProcessingSource`, simply override or 
implement `GetAboutInfo`:

```cs
[ProcessingSource(...)]
public class MyProcessingSource
    : ProcessingSource
{
    // ...

    public override ProcessingSourceInfo GetAboutInfo()
    {
        return new ProcessingSourceInfo
        {
            Owners = new[]
            {
                new ContactInfo
                {
                    Name = "Author Name",
                    Address = "Author Email",
                    EmailAddresses = new[]
                    {
                        "owners@mycompany.com",
                    },
                },
            },
            LicenseInfo = null,
            ProjectInfo = null,
            CopyrightNotice = $"Copyright (C) {DateTime.Now.Year}",
            AdditionalInformation = null,
        };
    }
}
```

or

```cs
[ProcessingSource(...)]
public class MyOtherProcessingSource
    : IProcessingSource
{
    // ...

    public ProcessingSourceInfo GetAboutInfo()
    {
        return new ProcessingSourceInfo
        {
            Owners = new[]
            {
                new ContactInfo
                {
                    Name = "Author Name",
                    Address = "Author Email",
                    EmailAddresses = new[]
                    {
                        "owners@mycompany.com",
                    },
                },
            },
            LicenseInfo = null,
            ProjectInfo = null,
            CopyrightNotice = $"Copyright (C) {DateTime.Now.Year}",
            AdditionalInformation = null,
        };
    }
}
```

# Viewing Your About Information
Where your about information appears depends on the SDK driver in which your plugin 
is loaded. In Windows Performance Analyzer, your about information will appear as 
a tab in the `Help -> About Windows Performance Analyzer` dialog.

# Conclusion

We have seen how to add about information to our SDK plugins, as well as how 
to view about information in Windows Performance Analyzer.

[Back to Advanced Topics](Overview.md)
