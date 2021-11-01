The following hierarchy is created to test the cookers in the engine:

                 Composite4
                    /   \
                   /  Composite3
                  /    /     \
                Composite1    \   Composite2
                 /     \       \    /
            Source3   Source4  Source5
             /  \  
        Source1 Source2

This allows us for a multitude of scenarios, including:

SourceCooker
SourceCooker that uses other source cookers
Composite cooker for one source
Composite cooker that uses two sources
Composite cooker that uses a composite and a source
Composite cooker that uses composite cookers

Note that Source5InternalTable is marked as internal, and will always be enabled whenever Source5Processor is created.
    This table requires Composite2 cooker, and so Source5DataCooker will always be enabled in this scenario.
