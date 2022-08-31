using GeneratR.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.DotNet
{
    public class XmlDocumentationCodeModel
    {
        private readonly List<string> _summaryLines = new();

        public XmlDocumentationCodeModel() 
        {
        }

        public IEnumerable<string> SummaryLines => _summaryLines;

        public XmlDocumentationCodeModel AddSummaryLine(string value)
        {
            _summaryLines.Add(value);
            return this;
        }

        public bool HasContents => SummaryLines.Any();

        public string Build(DotNetGenerator dotNet)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"{dotNet.DocumentationOperator} <summary>");
            // TODO: Handle when a summaryline has linebreaks in the string.
            foreach (var item in _summaryLines)
            {
                sb.AppendLine($"{dotNet.DocumentationOperator} {item}");
            }
            sb.Append($"{dotNet.DocumentationOperator} </summary>");

            return sb.ToString();
        }
    }
}
