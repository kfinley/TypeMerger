using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TypeMerger {
    /// <summary>
    /// Policy class used with the TypeMerger class to define properties that are ignored or priority when there objects contain the same properties and there is a collision.
    /// </summary>
    public class TypeMergerPolicy {

        private IList<Tuple<string, string>> ignoredProperties;
        private IList<Tuple<string, string>> useProperties;

        public TypeMergerPolicy() {
            
            ignoredProperties = new List<Tuple<string, string>>();
            useProperties = new List<Tuple<string, string>>();
        }

        internal IList<Tuple<string, string>> IgnoredProperties {
            
            get { return this.ignoredProperties; }
        }

        internal IList<Tuple<string, string>> UseProperties {
            
            get { return this.useProperties; }
        }

        /// <summary>
        /// Specify a property to be ignored from a object being merged.
        /// </summary>
        /// <param name="ignoreProperty">The property of the object to be ignored as a Func.</param>
        /// <returns>TypeMerger policy used in method chaining.</returns>
        public TypeMergerPolicy Ignore(Expression<Func<object>> ignoreProperty) {
            
            ignoredProperties.Add(GetObjectTypeAndProperty(ignoreProperty));
            return this;
        }

        /// <summary>
        /// Specify a property to use when there is a property name collision between objects being merged.
        /// </summary>
        /// <param name="useProperty"></param>
        /// <returns>TypeMerger policy used in method chaining.</returns>
        public TypeMergerPolicy Use(Expression<Func<object>> useProperty) {
            
            useProperties.Add(GetObjectTypeAndProperty(useProperty));
            return this;
        }

        /// <summary>
        ///   /// Merge two different object instances into a single object which is a super-set of the properties of both objects. 
        /// If property name collision occurs and no policy has been created to specify which to use using the .Use() method the property value from 'values1' will be used.
        /// </summary>
        /// <param name="values1">An object to be merged.</param>
        /// <param name="values2">An object to be merged.</param>
        /// <returns>New object containing properties from both objects</returns>
        public Object Merge(object values1, object values2) {
            
            return TypeMerger.Merge(values1, values2, this);
        }

        /// <summary>
        /// Inspects the property specified to get the underlying Type and property name to be used during merging.
        /// </summary>
        /// <param name="property">The property to inspect as a Func Expression.</param>
        /// <returns></returns>
        private Tuple<string, string> GetObjectTypeAndProperty(Expression<Func<object>> property) {

            var objType = string.Empty;
            var propName = string.Empty;

            try {
                if (property.Body is MemberExpression) {
                    objType = ((MemberExpression)property.Body).Expression.Type.Name;
                    propName = ((MemberExpression)property.Body).Member.Name;
                } else if (property.Body is UnaryExpression) {
                    objType = ((MemberExpression)((UnaryExpression)property.Body).Operand).Expression.Type.Name;
                    propName = ((MemberExpression)((UnaryExpression)property.Body).Operand).Member.Name;
                } else {
                    throw new Exception("Expression type unknown.");
                }
            } catch (Exception ex) {
                throw new Exception("Error in TypeMergePolicy.GetObjectTypeAndProperty.", ex);
            }

            return new Tuple<string, string>(objType, propName);
        }
    }
}
