using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeMerger {

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

        public TypeMergerPolicy Ignore(Expression<Func<object>> ignoreProperty) {
            ignoredProperties.Add(GetObjectTypeAndProperty(ignoreProperty));
            return this;
        }

        public TypeMergerPolicy Use(Expression<Func<object>> useProperty) {
            useProperties.Add(GetObjectTypeAndProperty(useProperty));
            return this;
        }

        public Object Merge(object values1, object values2) {
            return Merger.Merge(values1, values2, this);
        }

        private Tuple<string, string> GetObjectTypeAndProperty(Expression<Func<object>> ignoreProperty) {

            var objType = string.Empty;
            var propName = string.Empty;

            try {
                if (ignoreProperty.Body is MemberExpression) {
                    objType = ((MemberExpression)ignoreProperty.Body).Member.ReflectedType.UnderlyingSystemType.Name;
                    propName = ((MemberExpression)ignoreProperty.Body).Member.Name;
                } else if (ignoreProperty.Body is UnaryExpression) {
                    objType = ((MemberExpression)((UnaryExpression)ignoreProperty.Body).Operand).Member.ReflectedType.UnderlyingSystemType.Name;
                    propName = ((MemberExpression)((UnaryExpression)ignoreProperty.Body).Operand).Member.Name;
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
