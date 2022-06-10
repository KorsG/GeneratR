using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class PropertyCodeModel
    {
        public PropertyCodeModel()
        {
            Attributes = new DotNetAttributeCollection();
        }

        public string PropertyName { get; set; }

        public string PropertyType { get; set; }

        public DotNetModifierKeyword DotNetModifier { get; set; }

        ///<summary>
        /// List of attributes added to the property.
        ///</summary>
        public DotNetAttributeCollection Attributes { get; set; }

        public PropertyCodeModel AddAttribute(DotNetAttribute attribute)
        {
            if (Attributes == null) { Attributes = new DotNetAttributeCollection(); }
            Attributes.Add(attribute);
            return this;
        }
    }
}
