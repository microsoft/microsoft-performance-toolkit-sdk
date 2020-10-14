using System;
using System.Collections.Generic;
using DataExtensionsSample.DataTypes;
using SampleExtensions.DataTypes;

namespace DataGenerator
{
    public static class KeyMappings
    {
        public static Dictionary<Type, string> MappingFromType = new Dictionary<Type, string>
        {
            {typeof(ProcessLoad), "ProcessLoad"},
            {typeof(ProcessUnload), "ProcessUnload"},
            {typeof(ImageLoad), "ImageLoad"},
            {typeof(ImageUnload), "ImageUnload"},
            {typeof(CSwitchData), "CSwitch"},
        };

        public static Dictionary<string, Type> MappingFromKey = new Dictionary<string, Type>
        {
            {"ProcessLoad", typeof(ProcessLoad)},
            {"ProcessUnload", typeof(ProcessUnload)},
            {"ImageLoad", typeof(ImageLoad)},
            {"ImageUnload", typeof(ImageUnload)},
            {"CSwitch", typeof(CSwitchData)},
        };
    }

}
