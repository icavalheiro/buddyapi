using Buddy.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Buddy.API.Helpers
{
    /// <summary>
    /// Contains methods that help with model convertion to JSON.
    /// Mainly used in the "/controller/model" endpoints
    /// </summary>
    public static class ModelHelper
    {
        /// <summary>
        /// Maps to a property of class
        /// </summary>
        public struct PropertyValidations
        {
            public string name;
            public string type;
            public Validation[] validations;
            public object[] aditionalAttributes;
            public object payload;
        }

        /// <summary>
        /// Maps to a renderAsAttribute
        /// </summary>
        public struct RenderAs
        {
            public string type;
            public object payload;
        }

        /// <summary>
        /// Validation maps to a validation attribute
        /// a class property could have multiple validations
        /// </summary>
        public struct Validation
        {
            public string type;
            public string errorMessage;
            public object payload;
        }

        /// <summary>
        /// If this property has Validation attributes we shall extract them here
        /// </summary>
        /// <param name="property">Property to extract the validations from</param>
        /// <returns>The validations of the given property</returns>
        private static List<ValidationAttribute> GetValidationsForProperty(PropertyInfo property, out List<object> otherAttributes)
        {
            var attributes = property.GetCustomAttributes(true);
            var validations = new List<ValidationAttribute>();
            otherAttributes = new List<object>();
            foreach (var attribute in attributes)
            {
                var validationType = typeof(ValidationAttribute);
                var isValidationAttribute = validationType.IsInstanceOfType(attribute);

                //we only care for attributes that are or inherit from
                //ValidationAttribute, other attributes are not considered
                //as validation ones
                if (isValidationAttribute)
                {
                    validations.Add((ValidationAttribute)attribute);
                }
                else
                {
                    otherAttributes.Add(new
                    {
                        name=attribute.GetType().Name.Replace("Attribute", ""),
                        payload=attribute
                    });
                }
            }

            return validations;
        }

        /// <summary>
        /// Maps all the validations of a class properties to a struct
        /// </summary>
        /// <param name="validations">Valitions to be mappped for json convertion</param>
        /// <returns>The validation structs ready-for-json convertion</returns>
        private static List<Validation> ConvertToStructs(List<ValidationAttribute> validations)
        {
            var validationStructs = new List<Validation>();
            foreach (var validation in validations)
            {
                var validationStruct = new Validation
                {
                    type = validation.GetType().Name.Replace("Attribute", ""),
                    errorMessage = validation.FormatErrorMessage("{0}")
                };

                //UGLY CODE EXPLANATION
                //There's no easy way here, we will have to test for each known property
                //and add custom behaviour depending on them
                //later on we could try to find a generic way of doing this, in order to
                //make this future proof.

                //for now I just search the validation attributes documentation in microsoft docs
                //and manually mapped them here depending on their type

                //only validations that require some sort of "payload" are handled here

                //START UGLY CODE
                if (typeof(RegularExpressionAttribute).IsInstanceOfType(validation))
                {
                    var regexVal = (RegularExpressionAttribute)validation;
                    validationStruct.payload = regexVal.Pattern;
                }
                else
                if (typeof(MinLengthAttribute).IsInstanceOfType(validation))
                {
                    var minVal = (MinLengthAttribute)validation;
                    validationStruct.payload = minVal.Length;
                }
                else
                if (typeof(MaxLengthAttribute).IsInstanceOfType(validation))
                {
                    var maxVal = (MaxLengthAttribute)validation;
                    validationStruct.payload = maxVal.Length;
                }
                else
                if (typeof(RangeAttribute).IsInstanceOfType(validation))
                {
                    var rangeVal = (RangeAttribute)validation;
                    validationStruct.payload = $"{rangeVal.Minimum} ~ {rangeVal.Maximum}";
                }
                else
                if (typeof(StringLengthAttribute).IsInstanceOfType(validation))
                {
                    var stringLengVal = (StringLengthAttribute)validation;
                    validationStruct.payload = $"{stringLengVal.MinimumLength} ~ {stringLengVal.MaximumLength}";
                }
                //END UGLY CODE

                //add this validation to the list of validation for this prop
                validationStructs.Add(validationStruct);
            }

            return validationStructs;
        }

        /// <summary>
        /// Get the payload for this given property (if there any)
        /// The payload is a wayt for us to send extra information
        /// that might be useful for the front-end depending on the
        /// type of the property, like the possible options for an
        /// enumerator.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static object GetPayload(PropertyInfo prop)
        {
            var typeToCheck = prop.PropertyType;
            if (prop.PropertyType.IsArray)
            {
                //If it's an array we just want the element type
                typeToCheck = prop.PropertyType.GetElementType();
            }

            if (typeToCheck.IsEnum)
            {
                return Enum.GetNames(typeToCheck);
            }

            return null;
        }

        /// <summary>
        /// Converts the properties to ready-for-json structs
        /// </summary>
        /// <param name="properties">Properties to be converted to ready-for-json structs</param>
        /// <param name="optionsEndpointResolver">Endpoint resolver for the given property Type</param>
        /// <returns>The ready-for-json structs</returns>
        private static List<PropertyValidations> ConvertToStructs(IEnumerable<PropertyInfo> properties)
        {
            var propertyValidations = new List<PropertyValidations>();
            foreach (var prop in properties)
            {
                //lets get the data to use in the building
                var validations = GetValidationsForProperty(prop, out var otherAttributes);
                var asStruct = ConvertToStructs(validations);
                var type = prop.PropertyType.Name;
                var payload = GetPayload(prop);

                //build the property as a struct of data
                var propValidationStruct = new PropertyValidations
                {
                    name = prop.Name.ToLower(),//it will contain '[]' if it's an array
                    type = type,
                    validations = asStruct.ToArray(),
                    payload = payload,
                    aditionalAttributes = otherAttributes.ToArray()
                };

                //add it to the list that represents the model
                propertyValidations.Add(propValidationStruct);
            }

            return propertyValidations;
        }

        /// <summary>
        /// Converts the model/class to a model-json.
        /// It extracts the validation and "renderAs" attributes and exposes them
        /// in a clean format. It's really useful if you want to improve the user
        /// experience in the front-end by validating the model before submission.
        /// You may need to implement a custom plugin in the front-end that will
        /// handle the just in timemodel validation.
        /// 
        /// It returns a ready-for-json object, that can easily be converted into a
        /// json string by Newtonsoft.Json or the new .net core json.
        /// </summary>
        /// <typeparam name="T">Model to be converted</typeparam>
        /// <param name="optionsEndpointResolver">A function that returns the URL from where the options for a given type could be retrieve (if the type is found within the model)</param>
        /// <returns>A ready-for-json object</returns>
        public static object GetModelAsJson<T>()
        {
            var properties = typeof(T).GetProperties();
            var validationStructs = ConvertToStructs(properties);
            return new { properties = validationStructs };
        }
    }
}
