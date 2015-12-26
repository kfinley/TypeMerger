using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using System.Linq.Expressions;

namespace TypeMerger {
    /// <summary>
    /// A Utility class used to merge the properties of heterogenious objects
    /// 
    /// Original Source: Mark J. Miller
    /// URL: http://www.developmentalmadness.com/archive/2008/02/12/extend-anonymous-types-using.aspx
    /// 
    /// Extended by: Kyle Finley
    /// Items Added: TypeMergerPolicy for ignoring properties.
    /// 
    /// </summary>
    public class Merger {

        //assembly/module builders
        private static AssemblyBuilder asmBuilder = null;
        private static ModuleBuilder modBuilder = null;
        private static TypeMergerPolicy typeMergerPolicy = null;

        //object type cache
        private static IDictionary<String, Type> anonymousTypes = new Dictionary<String, Type>();

        //used for thread-safe access to Type Dictionary
        private static Object _syncLock = new Object();

        /// <summary>
        /// Merge two different object instances into a single
        /// object which is a super-set
        /// of the properties of both objects
        /// </summary>
        public static Object Merge(Object values1, Object values2) {

            //lock for thread safe writing
            lock (_syncLock) {

                //create a name from the names of both Types
                var name = String.Format("{0}_{1}", values1.GetType(),
                    values2.GetType());

                if (typeMergerPolicy != null) {
                    name += "_" + string.Join(",", typeMergerPolicy.IgnoredProperties.Select(x => String.Format("{0}_{1}", x.Item1, x.Item2)));
                }

                //now that we're inside the lock - check one more time
                object result = CreateInstance(name, values1, values2);
                if (result != null) {
                    typeMergerPolicy = null;
                    return result;
                }

                //merge list of PropertyDescriptors for both objects
                PropertyDescriptor[] pdc = GetProperties(values1, values2);

                //make sure static properties are properly initialized
                InitializeAssembly();

                //create the type definition
                Type newType = CreateType(name, pdc);

                //add it to the cache
                anonymousTypes.Add(name, newType);

                //return an instance of the new Type
                result = CreateInstance(name, values1, values2);
                typeMergerPolicy = null;
                return result;
            }
        }

        internal static Object Merge(object values1, object values2, TypeMergerPolicy policy) {

            typeMergerPolicy = policy;
            return Merge(values1, values2);
        }

        public static TypeMergerPolicy Ignore(Expression<Func<object>> ignoreProperty) {
            return new TypeMergerPolicy().Ignore(ignoreProperty);
        }

        public static TypeMergerPolicy Use(Expression<Func<object>> useProperty) {
            return new TypeMergerPolicy().Use(useProperty);
        }

        /// <summary>
        /// Instantiates an instance of an existing Type from cache
        /// </summary>
        private static Object CreateInstance(String name, object values1, object values2) {
            Object newValues = null;

            //check to see if type exists
            if (anonymousTypes.ContainsKey(name)) {

                //merge all values together into an array
                Object[] allValues = MergeValues(values1, values2);

                //get type
                Type type = anonymousTypes[name];

                //make sure it isn't null for some reason
                if (type != null) {
                    //create a new instance
                    newValues = Activator.CreateInstance(type, allValues);
                } else {
                    //remove null type entry
                    lock (_syncLock)
                        anonymousTypes.Remove(name);
                }
            }

            //return values (if any)
            return newValues;
        }

        /// <summary>
        /// Merge PropertyDescriptors for both objects
        /// </summary>
        private static PropertyDescriptor[] GetProperties(object values1, object values2) {

            //dynamic list to hold merged list of properties
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

            //get the properties from both objects
            PropertyDescriptorCollection pdc1 = TypeDescriptor.GetProperties(values1);
            PropertyDescriptorCollection pdc2 = TypeDescriptor.GetProperties(values2);

            //add properties from values1
            for (int i = 0; i < pdc1.Count; i++) {
                if (typeMergerPolicy == null
                    || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(values1.GetType().Name, pdc1[i].Name))
                    & !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(values2.GetType().Name, pdc1[i].Name))))
                    properties.Add(pdc1[i]);
            }
            //add properties from values2
            for (int i = 0; i < pdc2.Count; i++) {
                if (typeMergerPolicy == null
                    || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(values2.GetType().Name, pdc2[i].Name))
                    & !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(values1.GetType().Name, pdc2[i].Name))))
                    properties.Add(pdc2[i]);
            }
            //return array
            return properties.ToArray();
        }

        /// <summary>
        /// Get the type of each property
        /// </summary>
        private static Type[] GetTypes(PropertyDescriptor[] pdc) {
            List<Type> types = new List<Type>();

            for (int i = 0; i < pdc.Length; i++)
                types.Add(pdc[i].PropertyType);

            return types.ToArray();
        }

        /// <summary>
        /// Merge the values of the two types into an object array
        /// </summary>
        private static Object[] MergeValues(object values1, object values2) {
            PropertyDescriptorCollection pdc1 = TypeDescriptor.GetProperties(values1);
            PropertyDescriptorCollection pdc2 = TypeDescriptor.GetProperties(values2);

            List<Object> values = new List<Object>();
            for (int i = 0; i < pdc1.Count; i++) {
                if (typeMergerPolicy == null
                    || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(values1.GetType().Name, pdc1[i].Name))
                    & !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(values2.GetType().Name, pdc1[i].Name))))
                    values.Add(pdc1[i].GetValue(values1));
            }

            for (int i = 0; i < pdc2.Count; i++) {
                if (typeMergerPolicy == null
                    || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(values2.GetType().Name, pdc2[i].Name))
                    & !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(values1.GetType().Name, pdc2[i].Name))))
                    values.Add(pdc2[i].GetValue(values2));
            }
            return values.ToArray();
        }

        /// <summary>
        /// Initialize static objects
        /// </summary>
        private static void InitializeAssembly() {
            //check to see if we've already instantiated
            //the static objects
            if (asmBuilder == null) {
                //create a new dynamic assembly
                AssemblyName assembly = new AssemblyName();
                assembly.Name = "AnonymousTypeExentions";

                //get the current application domain
                AppDomain domain = Thread.GetDomain();

                //get a module builder object
                asmBuilder = domain.DefineDynamicAssembly(assembly,
                    AssemblyBuilderAccess.Run);
                modBuilder = asmBuilder.DefineDynamicModule(
                    asmBuilder.GetName().Name, false
                    );
            }
        }

        /// <summary>
        /// Create a new Type definition from the list
        /// of PropertyDescriptors
        /// </summary>
        private static Type CreateType(String name, PropertyDescriptor[] pdc) {
            //create TypeBuilder
            TypeBuilder typeBuilder = CreateTypeBuilder(name);

            //get list of types for ctor definition
            Type[] types = GetTypes(pdc);

            //create priate fields for use w/in the ctor body and properties
            FieldBuilder[] fields = BuildFields(typeBuilder, pdc);

            //define/emit the Ctor
            BuildCtor(typeBuilder, fields, types);

            //define/emit the properties
            BuildProperties(typeBuilder, fields);

            //return Type definition
            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Create a type builder with the specified name
        /// </summary>
        private static TypeBuilder CreateTypeBuilder(string typeName) {
            TypeBuilder typeBuilder = modBuilder.DefineType(typeName,
                        TypeAttributes.Public,
                        typeof(object));
            //return new type builder
            return typeBuilder;
        }

        /// <summary>
        /// Define/emit the ctor and ctor body
        /// </summary>
        private static void BuildCtor(TypeBuilder typeBuilder, FieldBuilder[] fields, Type[] types) {
            //define ctor()
            ConstructorBuilder ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                types
                );

            //build ctor()
            ILGenerator ctorGen = ctor.GetILGenerator();

            //create ctor that will assign to private fields
            for (int i = 0; i < fields.Length; i++) {
                //load argument (parameter)
                ctorGen.Emit(OpCodes.Ldarg_0);
                ctorGen.Emit(OpCodes.Ldarg, (i + 1));

                //store argument in field
                ctorGen.Emit(OpCodes.Stfld, fields[i]);
            }

            //return from ctor()
            ctorGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Define fields based on the list of PropertyDescriptors
        /// </summary>
        private static FieldBuilder[] BuildFields(TypeBuilder typeBuilder, PropertyDescriptor[] pdc) {
            List<FieldBuilder> fields = new List<FieldBuilder>();

            //build/define fields
            for (int i = 0; i < pdc.Length; i++) {
                PropertyDescriptor pd = pdc[i];

                //define field as '_[Name]' with the object's Type
                FieldBuilder field = typeBuilder.DefineField(
                    String.Format("_{0}", pd.Name),
                    pd.PropertyType,
                    FieldAttributes.Private
                    );

                //add to list of FieldBuilder objects
                if (fields.Contains(field) == false)
                    fields.Add(field);
            }

            return fields.ToArray();
        }

        /// <summary>
        /// Build a list of Properties to match the list of private fields
        /// </summary>
        private static void BuildProperties(TypeBuilder typeBuilder, FieldBuilder[] fields) {
            //build properties
            for (int i = 0; i < fields.Length; i++) {
                //remove '_' from name for public property name
                String propertyName = fields[i].Name.Substring(1);

                //define the property
                PropertyBuilder property = typeBuilder.DefineProperty(propertyName,
                    PropertyAttributes.None, fields[i].FieldType, null);

                ////define 'Get' method only (anonymous types are read-only)
                //MethodBuilder getMethod = typeBuilder.DefineMethod(
                //    String.Format("Get_{0}", propertyName)
                //    MethodAttributes.Public  MethodAttributes.SpecialName
                //         MethodAttributes.HideBySig,
                //    fields[i].FieldType,
                //    Type.EmptyTypes
                //    );

                //define 'Get' method only (anonymous types are read-only)
                MethodBuilder getMethod = typeBuilder.DefineMethod(
                    String.Format("Get_{0}", propertyName),
                    MethodAttributes.Public,
                    fields[i].FieldType,
                    Type.EmptyTypes
                    );

                //build 'Get' method
                ILGenerator methGen = getMethod.GetILGenerator();

                //method body
                methGen.Emit(OpCodes.Ldarg_0);
                //load value of corresponding field
                methGen.Emit(OpCodes.Ldfld, fields[i]);
                //return from 'Get' method
                methGen.Emit(OpCodes.Ret);

                //assign method to property 'Get'
                property.SetGetMethod(getMethod);

                ////define 'Set' method
                //MethodBuilder setMethod = typeBuilder.DefineMethod(
                //    String.Format("Set_{0}", propertyName),
                //    MethodAttributes.Public | MethodAttributes.SpecialName,
                //    null,
                //    new Type[] { fields[i].FieldType }
                //);

                //ILGenerator setMethodGenerator = setMethod.GetILGenerator();
                //setMethodGenerator.Emit(OpCodes.Ldarg_0);     // load this
                //setMethodGenerator.Emit(OpCodes.Ldarg_1);     // load value
                //setMethodGenerator.Emit(OpCodes.Stfld, fields[i]); // store into field
                //setMethodGenerator.Emit(OpCodes.Ret); // return

                //property.SetSetMethod(setMethod);

            }
        }

    }
}

