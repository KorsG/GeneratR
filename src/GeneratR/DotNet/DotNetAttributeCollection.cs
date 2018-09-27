using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneratR.DotNet
{
    public class DotNetAttributeCollection : List<DotNetAttribute>
    {
        public DotNetAttributeCollection()
        {
        }

        ///<summary>
        /// Add an attribute to the collection. 
        /// Replaces any existing attribute with the same name.
        ///</summary>
        new public void Add(DotNetAttribute DotNetAttribute)
        {
            var item = GetByName(DotNetAttribute.Name, StringComparison.OrdinalIgnoreCase);
            if (item != null)
            {
                Remove(item);
            }
            base.Add(DotNetAttribute);
        }

        public void AddIfNotExists(DotNetAttribute DotNetAttribute)
        {
            if (!ExistsByName(DotNetAttribute.Name))
            {
                base.Add(DotNetAttribute);
            }
        }

        ///<summary>
        ///	Add attributes to the collection. 
        ///	Replaces any existing attributes with the same name.
        ///</summary>
        public void AddList(List<DotNetAttribute> DotNetAttributes)
        {
            if (DotNetAttributes != null && DotNetAttributes.Any())
            {
                foreach (var a in DotNetAttributes)
                {
                    Add(a);
                }
            }
        }

        public void Remove(string attributeName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var item = GetByName(attributeName, comparison);
            if (item != null)
            {
                Remove(item);
            }
        }

        public void RemoveList(List<string> attributeNames, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (attributeNames != null && attributeNames.Any())
            {
                foreach (string name in attributeNames)
                {
                    Remove(name, comparison);
                }
            }
        }

        public bool ExistsByName(string attributeName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var item = GetByName(attributeName, comparison);
            if (item != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public DotNetAttribute GetByName(string attributeName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return this.Where(q => q.Name.Equals(attributeName, comparison)).SingleOrDefault();
        }

        public string ToMultilineString()
        {
            var sb = new StringBuilder();
            foreach (var a in this)
            {
                sb.AppendLine(a.ToString());
            }
            return sb.ToString();
        }
    }
}
