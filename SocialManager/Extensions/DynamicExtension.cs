using SocialManager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SocialManager.Extensions
{
    public class propertycontainer
    {
        public string facebookfield { get; set; }
        public string facebookparent { get; set; }
        public PropertyInfo facebookMappedProperty { get; set; }
    }


    public static class DynamicExtension
    {
        public static T ToStatic<T>(object dynamicObject)
        {
            //Create new Instance of the Static 
            var entity = Activator.CreateInstance<T>();

            //Convert the dynamicObject as an implemented dictionary
            var properties = dynamicObject as IDictionary<string, object>;

            if (properties == null)
                return entity;

            //Create Lookup Dictionary to hold static destination class set of FacebookMapping decorated properties
            List<propertycontainer> PropertyLookup = new List<propertycontainer>();
            //Get list of FacebookMapping custom attribute properties decorated within the destination static class 
            var destFBMappingProperties = (from PropertyInfo property in entity.GetType().GetProperties()
                                           where property.GetCustomAttributes(typeof(FacebookMapping), true).Length > 0
                                           select property).ToList();
            //Loop through the list of Facebook properties to build a dictionary lookup list
            foreach (PropertyInfo pi in destFBMappingProperties)
            {
                foreach (object attribute in pi.GetCustomAttributes(typeof(FacebookMapping)))
                {
                    FacebookMapping facebookMapAttribute = attribute as FacebookMapping;
                    if (facebookMapAttribute != null)
                    {
                        //Add new property container with Facebook mapping and property info details from destination class
                        PropertyLookup.Add(new propertycontainer
                            {
                                facebookfield = facebookMapAttribute.GetName(),
                                facebookparent = facebookMapAttribute.parent,
                                facebookMappedProperty = pi
                            });
                    }
                }
            }

            //Iterate through the dynamic Object's list of properties, looking for match from the facebook mapping lookup
            foreach (var entry in properties)
            {
                var MatchedResults = PropertyLookup.Where(x => x.facebookparent == entry.Key || x.facebookfield == entry.Key);

                if (MatchedResults != null)
                    foreach (propertycontainer DestinationPropertyInfo in MatchedResults)
                    {
                            object mappedValue =null;
                            if (entry.Value.GetType().Name == "JsonObject")
                            {
                                //drill down on level to obtain a list of properties from the child set of properties 
                                //of the dynamic Object
                                mappedValue = FindMatchingChildPropertiesRecursively(entry, DestinationPropertyInfo);                           

                                //child properity was not matched so apply the parent FacebookJson object as the entry value
                                if (mappedValue == null &&
                                    DestinationPropertyInfo.facebookfield == entry.Key)
                                    mappedValue = entry.Value;

                            }
                            else
                            {
                                if (String.IsNullOrEmpty(DestinationPropertyInfo.facebookparent) &&
                                    DestinationPropertyInfo.facebookfield == entry.Key)
                                    mappedValue = entry.Value;
                            }

                            //copy mapped value into destination class property
                            if (mappedValue != null)
                                if (DestinationPropertyInfo.facebookMappedProperty.PropertyType.Name == "DateTime")
                                {
                                    DestinationPropertyInfo.facebookMappedProperty.SetValue(entity, System.DateTime.Parse(mappedValue.ToString()), null);
                                }
                                else
                                    DestinationPropertyInfo.facebookMappedProperty.SetValue(entity, mappedValue, null);
                    }
            }
            return entity;
        }

        private static object FindMatchingChildPropertiesRecursively(
            KeyValuePair<string, object> entry, 
            propertycontainer DestinationPropertyInfo)
        {
            object mappedValue = null;
            //drill down on level to obtain a list of properties from the child set of properties 
            //of the dynamic Object
            var childproperties = entry.Value as IDictionary<string, object>;

            //This is a parent JsonObject so now lets see if we have a matching child property in this Json Object entry
            mappedValue = (from KeyValuePair<string, object> item in childproperties
                           where item.Key == DestinationPropertyInfo.facebookfield
                           select item.Value).FirstOrDefault();

            //child properity was not matched so apply the parent FacebookJson object as the entry value
            if (mappedValue == null)
            {
                foreach (KeyValuePair<string, object> item in childproperties)
                {
                    if (item.Value.GetType().Name == "JsonObject")
                    {
                        //recurse to find if child properties match our DestinationPropertyInfo.facebookfield
                        mappedValue = FindMatchingChildPropertiesRecursively(item, DestinationPropertyInfo);
                    }
                }
            }

            return mappedValue;
        }


    }


}