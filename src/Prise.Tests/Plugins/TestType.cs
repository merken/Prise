using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Prise.Tests.Plugins
{
    public interface IBuilder<T>
    {
        T Build();
    }

    public class TestableTypeBuilder : IBuilder<TestableType>
    {
        private string name;
        private string @namespace;
        private List<CustomAttributeData> attributes;
        private List<MemberInfo> members;
        private List<MethodInfo> methods;
        private List<FieldInfo> fields;
        public TestableTypeBuilder()
        {
            this.attributes = new List<CustomAttributeData>();
            this.members = new List<MemberInfo>();
            this.methods = new List<MethodInfo>();
            this.fields = new List<FieldInfo>();
        }
        public TestableTypeBuilder WithCustomAttributes(params CustomAttributeData[] attributes)
        {
            this.attributes.AddRange(attributes);
            return this;
        }

        public TestableTypeBuilder WithMethods(params MethodInfo[] methods)
        {
            this.methods.AddRange(methods);
            return this;
        }

        public TestableTypeBuilder WithFields(params FieldInfo[] fields)
        {
            this.fields.AddRange(fields);

            return this;
        }

        public TestableTypeBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public TestableTypeBuilder WithNamespace(string @namespace)
        {
            this.@namespace = @namespace;
            return this;
        }

        public TestableType Build() => new TestableType
        {
            _CustomAttributes = this.attributes,
            _Name = this.name,
            _Namespace = this.@namespace,
            _Methods = this.methods,
            _Fields = this.fields,
            _Members = this.members
        };

        public static TestableTypeBuilder New() => new TestableTypeBuilder();
    }

    public class TestableAttributeBuilder : IBuilder<TestableAttribute>
    {
        private Type attributeType;
        private List<CustomAttributeNamedArgument> arguments;
        public TestableAttributeBuilder()
        {
            this.arguments = new List<CustomAttributeNamedArgument>();
        }

        public TestableAttributeBuilder WithAttributeType(Type type)
        {
            this.attributeType = type;
            return this;
        }

        public TestableAttributeBuilder WithNamedAgrument(CustomAttributeNamedArgument argument)
        {
            this.arguments.Add(argument);
            return this;
        }

        public TestableAttributeBuilder WithNamedAgrument(string name, object value)
        {
            this.arguments.Add(new CustomAttributeNamedArgument(new TestableMemberInfo
            {
                _Name = name
            }, new CustomAttributeTypedArgument(value)));
            return this;
        }

        public TestableAttribute Build() => new TestableAttribute
        {
            _AttributeType = this.attributeType,
            _NamedArguments = this.arguments
        };

        public static TestableAttributeBuilder New() => new TestableAttributeBuilder();
    }

    public class TestableMethodInfoBuilder : IBuilder<TestableMethodInfo>
    {
        private string name;
        private List<CustomAttributeData> attributes;
        public TestableMethodInfoBuilder()
        {
            this.attributes = new List<CustomAttributeData>();
        }

        public TestableMethodInfoBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public TestableMethodInfoBuilder WithAttribute(CustomAttributeData attribute)
        {
            this.attributes.Add(attribute);
            return this;
        }

        public TestableMethodInfo Build() => new TestableMethodInfo
        {
            _Name = this.name,
            _CustomAttributes = this.attributes
        };

        public static TestableMethodInfoBuilder New() => new TestableMethodInfoBuilder();
    }

    public class TestableFieldBuilder : IBuilder<TestableFieldInfo>
    {
        private string name;
        private List<CustomAttributeData> attributes;
        public TestableFieldBuilder()
        {
            this.attributes = new List<CustomAttributeData>();
        }

        public TestableFieldBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public TestableFieldBuilder WithAttribute(CustomAttributeData attribute)
        {
            this.attributes.Add(attribute);
            return this;
        }

        public TestableFieldInfo Build() => new TestableFieldInfo
        {
            _Name = this.name,
            _CustomAttributes = this.attributes
        };

        public static TestableFieldBuilder New() => new TestableFieldBuilder();
    }
    public class TestableAttribute : CustomAttributeData
    {
        internal Type _AttributeType;
        internal IList<CustomAttributeNamedArgument> _NamedArguments;
        public override IList<CustomAttributeNamedArgument> NamedArguments => _NamedArguments;
        public override Type AttributeType => _AttributeType;
    }

    public class TestableMemberInfo : MemberInfo
    {
        internal string _Name;

        public override IEnumerable<CustomAttributeData> CustomAttributes { get; } // TODO

        public override Type DeclaringType => throw new NotImplementedException();

        public override MemberTypes MemberType => throw new NotImplementedException();

        public override string Name => _Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }

    public class TestableMethodInfo : MethodInfo
    {
        internal string _Name;
        internal IEnumerable<CustomAttributeData> _CustomAttributes;
        public override IEnumerable<CustomAttributeData> CustomAttributes => _CustomAttributes;
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

        public override MethodAttributes Attributes => throw new NotImplementedException();

        public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

        public override Type DeclaringType => throw new NotImplementedException();

        public override string Name => _Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit) => _CustomAttributes.ToArray();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _CustomAttributes.ToArray();

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }

    public class TestableFieldInfo : FieldInfo
    {
        internal IEnumerable<CustomAttributeData> _CustomAttributes;
        internal string _Name;
        public override IEnumerable<CustomAttributeData> CustomAttributes => _CustomAttributes;

        public override FieldAttributes Attributes => throw new NotImplementedException();

        public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

        public override Type FieldType => throw new NotImplementedException();

        public override Type DeclaringType => throw new NotImplementedException();

        public override string Name => _Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override object[] GetCustomAttributes(bool inherit) => _CustomAttributes.ToArray();
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _CustomAttributes.ToArray();

        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestableType : TypeInfo
    {
        internal IEnumerable<CustomAttributeData> _CustomAttributes;
        internal string _Name;
        internal string _Namespace;
        internal IEnumerable<MemberInfo> _Members;
        internal IEnumerable<MethodInfo> _Methods;
        internal IEnumerable<FieldInfo> _Fields;

        public override IEnumerable<FieldInfo> DeclaredFields => _Fields;

        public override IEnumerable<System.Reflection.CustomAttributeData> CustomAttributes => _CustomAttributes;

        public override Assembly Assembly => throw new NotImplementedException();

        public override string AssemblyQualifiedName => throw new NotImplementedException();

        public override Type BaseType => throw new NotImplementedException();

        public override string FullName => throw new NotImplementedException();

        public override Guid GUID => throw new NotImplementedException();

        public override Module Module => throw new NotImplementedException();

        public override string Namespace => _Namespace;
        public override Type UnderlyingSystemType => throw new NotImplementedException();

        public override string Name => _Name;

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit) => _CustomAttributes.ToArray();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _CustomAttributes.ToArray();

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => _Members.ToArray();

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => _Methods.ToArray();
        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override bool HasElementTypeImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsArrayImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsByRefImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }
    }
}