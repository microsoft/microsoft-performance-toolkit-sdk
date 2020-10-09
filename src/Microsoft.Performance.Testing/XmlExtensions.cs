// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.Performance.Testing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting with XML.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        ///     Gets the value of the attribute with the given name.
        /// </summary>
        /// <param name="element">
        ///     The element to interrogate.
        /// </param>
        /// <param name="attributeName">
        ///     The name of the element's attribute whose value is to be retrieved.
        /// </param>
        /// <returns>
        ///     The value of the attribute.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        ///     There is no matching attribute.
        ///     - or -
        ///     There is more than one matching attribute.
        /// </exception>
        public static string GetAttributeValue(this XElement element, string attributeName)
        {
            return element.Attributes().Single(x => x.Name.LocalName == attributeName).Value;
        }

        /// <summary>
        ///     Gets the value of the attribute with the given name.
        /// </summary>
        /// <param name="element">
        ///     The element to interrogate.
        /// </param>
        /// <param name="attributeName">
        ///     The name of the element's attribute whose value is to be retrieved.
        /// </param>
        /// <param name="value">
        ///     Receives the value of the attribute, if the attribute exists; null otherwise.
        /// </param>
        /// <returns>
        ///     true if the attribute existed and was retrieved; false otherwise.
        /// </returns>
        public static bool TryGetAttributeValue(this XElement element, string attributeName, out string value)
        {
            value = null;
            var attribute = element.Attributes().SingleOrDefault(x => x.Name.LocalName == attributeName);
            if (attribute == null)
            {
                return false;
            }

            value = attribute.Value;
            return true;
        }

        /// <summary>
        ///     Gets the children of the given element with the given name.
        /// </summary>
        /// <param name="element">
        ///     The element to interrogate.
        /// </param>
        /// <param name="children">
        ///     The name of the child elements to retrieve.
        /// </param>
        /// <returns>
        ///     All child elements with the specified name of the given element.
        /// </returns>
        public static IEnumerable<XElement> GetChildren(this XElement element, string children)
        {
            return element.Elements().Where(x => x.Name.LocalName == children);
        }

        /// <summary>
        ///     Gets the single child element with the given name of the given element.
        /// </summary>
        /// <param name="element">
        ///     The element to interrogate.
        /// </param>
        /// <param name="childName">
        ///     The name of the single child element to retrieve.
        /// </param>
        /// <returns>
        ///     The single child element with the given name.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        ///     There is no matching child element.
        ///     - or -
        ///     There is more than one matching child element.
        /// </exception>
        public static XElement GetChild(this XElement element, string childName)
        {
            return element.GetChildren(childName).Single();
        }

        /// <summary>
        ///     Gets the single child element with the given name of the given element,
        ///     or <c>null</c> if the child does not exist.
        /// </summary>
        /// <param name="element">
        ///     The element to interrogate.
        /// </param>
        /// <param name="childName">
        ///     The name of the single child element to retrieve.
        /// </param>
        /// <returns>
        ///     The single child element with the given name.
        ///     - or -
        ///     <c>null</c> if the child does not exist.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        ///     There is more than one matching child element.
        /// </exception>
        public static XElement GetChildOrDefault(this XElement element, string childName)
        {
            return element.GetChildren(childName).SingleOrDefault();
        }

        /// <summary>
        ///     Gets the single child element with the given name of the given element, if
        ///     the child exists; returns null if the child does not exist.
        /// </summary>
        /// <param name="element">
        ///     The element to interrogate.
        /// </param>
        /// <param name="childName">
        ///     The name of the single child element to retrieve.
        /// </param>
        /// <returns>
        ///     The single child element with the given name, if the child exists; null otherwise.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        ///     There is more than one matching child element.
        /// </exception>
        public static bool TryGetChild(this XElement element, string childName, out XElement child)
        {
            child = element.GetChildren(childName).SingleOrDefault();
            return child != null;
        }

        /// <summary>
        ///     Get a reasonable absolute XPath to this element.
        /// </summary>
        /// <param name="self">
        ///     The element to get the path to.
        /// </param>
        /// <returns>
        ///     An absolute XPath to the element. This path may become incorrect or invalid if elements are added or
        ///     removed from the document.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the collection is modified during execution.
        /// </exception>
        public static string GetAbsoluteXPath(this XElement self)
        {
            return BuildAbsoluteXPath(self).ToString();
        }

        /// <summary>
        ///     Get a reasonable absolute XPath to this attribute.
        /// </summary>
        /// <param name="self">
        ///     The attribute to get the path to.
        /// </param>
        /// <returns>
        ///     An absolute XPath to the attribute. This path may become incorrect or invalid if elements are added or
        ///     removed from the document.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the collection is modified during execution.
        /// </exception>
        public static string GetAbsoluteXPath(this XAttribute self)
        {
            var path = BuildAbsoluteXPath(self.Parent);

            path.Append("/@");
            path.Append(self.Name.LocalName);

            return path.ToString();
        }

        /// <summary>
        ///     Get the index of this element among the other elements which share the same parent.
        /// </summary>
        /// <param name="self">
        ///     The element to look for.
        /// </param>
        /// <returns>
        ///     The element index, or -1 if this element is the root element or if there are no siblings with the same
        ///     name.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the collection is modified during execution.
        /// </exception>
        public static int GetXPathIndex(this XElement self)
        {
            var parent = self.Parent;

            if (parent == null)
            {
                return -1;
            }

            int i = 1;
            bool found = false;

            foreach (var element in parent.Elements(self.Name))
            {
                if (found)
                {
                    // This is not the only element
                    return i;
                }
                else
                {
                    if (element == self)
                    {
                        if (i != 1)
                        {
                            // There have already been other elements
                            return i;
                        }

                        // We're not sure if we're the only element yet
                        found = true;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            if (found)
            {
                // This is the only element.
                return -1;
            }

            throw new InvalidOperationException("Collection was modified.");
        }

        /// <summary>
        ///     Get a reasonable absolute XPath to this element.
        /// </summary>
        /// <param name="self">
        ///     The element to get the path to.
        /// </param>
        /// <returns>
        ///     An absolute XPath to the element. This path may become incorrect or invalid if elements are added or
        ///     removed from the document.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the collection is modified during execution.
        /// </exception>
        private static StringBuilder BuildAbsoluteXPath(XElement self)
        {
            StringBuilder ret;

            if (self.Parent == null)
            {
                ret = new StringBuilder("/");
                ret.Append(self.Name.LocalName);
            }
            else
            {
                ret = BuildAbsoluteXPath(self.Parent);
                ret.Append('/');
                ret.Append(self.Name.LocalName);

                var index = GetXPathIndex(self);
                if (index > 0)
                {
                    ret.AppendFormat("[{0}]", index);
                }
            }

            return ret;
        }
    }
}
