using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Prise.Tests.Plugins
{
    public class TestableTypeBuilder
    {
        private string name;
        private string @namespace;
        private System.Reflection.CustomAttributeData[] attributes;
        public TestableTypeBuilder WithCustomAttributes(params System.Reflection.CustomAttributeData[] attributes)
        {
            this.attributes = attributes;
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
            _Name = name,
            _Namespace = @namespace,
        };

        public static TestableTypeBuilder NewTestableType() => new TestableTypeBuilder();
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

    public class TestableType : Type
    {
        internal System.Collections.Generic.IEnumerable<System.Reflection.CustomAttributeData> _CustomAttributes;
        internal string _Name;
        internal string _Namespace;

        public override System.Collections.Generic.IEnumerable<System.Reflection.CustomAttributeData> CustomAttributes => _CustomAttributes;

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

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

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

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

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