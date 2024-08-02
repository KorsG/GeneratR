using System;

namespace GeneratR.DotNet
{
    public class PropertyCodeModel
    {
        public PropertyCodeModel()
        {
        }

        public string PropertyName { get; set; }

        public string PropertyType { get; set; }

        public DotNetModifierKeyword Modifier { get; set; }

        public bool IsReadOnly { get; set; }

        public DotNetAccessModifierKeyword GetterModifier { get; set; }

        public string GetterBody { get; set; }

        public DotNetAccessModifierKeyword SetterModifier { get; set; }

        public string SetterBody { get; set; }

        public XmlDocumentationCodeModel XmlDocumentation { get; } = new XmlDocumentationCodeModel();

        ///<summary>
        /// List of attributes added to the property.
        ///</summary>
        public DotNetAttributeCollection Attributes { get; set; } = new DotNetAttributeCollection();

        public PropertyCodeModel AddAttribute(DotNetAttribute attribute)
        {
            if (Attributes == null) { Attributes = new DotNetAttributeCollection(); }
            Attributes.Add(attribute);
            return this;
        }

        public PropertyCodeModel SetGetter(DotNetAccessModifierKeyword modifier, string body)
        {
            GetterModifier = modifier;
            GetterBody = body;
            return this;
        }

        public PropertyCodeModel SetSetter(DotNetAccessModifierKeyword modifier, string body)
        {
            SetterModifier = modifier;
            SetterBody = body;
            return this;
        }

        public PropertyCodeModel WithXmlDocumentation(Action<XmlDocumentationCodeModel> action)
        {
            action?.Invoke(XmlDocumentation);
            return this;
        }

        public PropertyCodeModel Clone()
        {
            var clone = (PropertyCodeModel)MemberwiseClone();
            clone.Attributes = Attributes.Clone();
            return clone;
        }
    }
}
