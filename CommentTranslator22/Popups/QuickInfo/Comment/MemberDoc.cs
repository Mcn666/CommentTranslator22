using System.Collections.Generic;
using System.Xml.Linq;

namespace CommentTranslator22.Popups.QuickInfo.Comment
{
    internal class MemberDoc
    {
        public string MemberName { get; set; }
        public string Summary { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public string Returns { get; set; }
        public string MemberType { get; set; } // M: 方法, P: 属性, T: 类型等

        public static MemberDoc FromXml(string xmlString)
        {
            var xml = XElement.Parse(xmlString);
            var doc = new MemberDoc();

            // 解析成员名
            var nameAttr = xml.Attribute("name");
            if (nameAttr != null)
            {
                doc.MemberName = nameAttr.Value;
                // 解析成员类型（M, P, T等）
                if (doc.MemberName.StartsWith("M:"))
                    doc.MemberType = "Method";
                else if (doc.MemberName.StartsWith("P:"))
                    doc.MemberType = "Property";
                else if (doc.MemberName.StartsWith("T:"))
                    doc.MemberType = "Type";
            }

            // 解析summary
            var summary = xml.Element("summary");
            if (summary != null)
                doc.Summary = summary.Value.Trim();

            // 解析parameters
            foreach (var param in xml.Elements("param"))
            {
                var paramName = param.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(paramName))
                    doc.Parameters[paramName] = param.Value.Trim();
            }

            // 解析returns
            var returns = xml.Element("returns");
            if (returns != null)
                doc.Returns = returns.Value.Trim();

            return doc;
        }
    }
}
