using System.Collections.Generic;
using System.Xml.Linq;

namespace CommentTranslator22.Popups.QuickInfo.Comment
{
    internal class MemberDoc
    {
        public string MemberName { get; set; }
        public string Summary { get; set; }
        public string Remarks { get; set; }
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
                else if (doc.MemberName.StartsWith("F:"))
                    doc.MemberType = "Field";
                else if (doc.MemberName.StartsWith("E:"))
                    doc.MemberType = "Event";
            }

            // 解析summary - 处理<see cref="..."/>标签
            var summary = xml.Element("summary");
            if (summary != null)
            {
                doc.Summary = ExtractSeeCrefText(summary);
            }

            // 解析remarks - 处理<see cref="..."/>标签
            var remarks = xml.Element("remarks");
            if (remarks != null)
            {
                doc.Remarks = ExtractSeeCrefText(remarks);
            }

            // 解析parameters
            foreach (var param in xml.Elements("param"))
            {
                var paramName = param.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(paramName))
                {
                    // 同样处理param中的<see>标签
                    doc.Parameters[paramName] = ExtractSeeCrefText(param);
                }
            }

            // 解析returns - 处理<see cref="..."/>标签
            var returns = xml.Element("returns");
            if (returns != null)
            {
                doc.Returns = ExtractSeeCrefText(returns);
            }

            return doc;
        }

        /// <summary>
        /// 从XElement中提取文本，将<see cref="..."/>标签转换为对应的引用文本
        /// </summary>
        private static string ExtractSeeCrefText(XElement element)
        {
            if (element == null) return "";

            // 获取所有节点（包括文本节点）
            var nodes = element.Nodes();
            var result = "";

            foreach (var node in nodes)
            {
                if (node is XText textNode)
                {
                    // 文本节点直接添加
                    result += textNode.Value;
                }
                else if (node is XElement elem)
                {
                    if (elem.Name == "see")
                    {
                        // 处理<see>标签
                        var crefAttr = elem.Attribute("cref");
                        if (crefAttr != null)
                        {
                            result += GetSeeCrefDisplayName(crefAttr.Value);
                        }
                        else
                        {
                            // 如果没有cref属性，尝试获取标签内容
                            result += elem.Value;
                        }
                    }
                    else if (elem.Name == "paramref")
                    {
                        // 处理<paramref>标签
                        var nameAttr = elem.Attribute("name");
                        if (nameAttr != null)
                        {
                            result += nameAttr.Value;
                        }
                        else
                        {
                            result += elem.Value;
                        }
                    }
                    else
                    {
                        // 递归处理其他元素及其内容
                        result += ExtractSeeCrefText(elem);
                    }
                }
            }

            return result.Trim();
        }

        /// <summary>
        /// 将cref值转换为显示名称
        /// </summary>
        private static string GetSeeCrefDisplayName(string cref)
        {
            if (string.IsNullOrEmpty(cref))
                return cref;

            // 去掉前缀 (T:, M:, P:, F:, E:, N:)
            var displayName = cref;
            if (cref.Contains(":"))
            {
                displayName = cref.Substring(cref.IndexOf(':') + 1);
            }

            // 如果包含方法参数，去掉参数部分
            var parenIndex = displayName.IndexOf('(');
            if (parenIndex > 0)
            {
                displayName = displayName.Substring(0, parenIndex);
            }

            // 去掉命名空间，只保留类名和方法名
            var parts = displayName.Split('.');
            if (parts.Length > 0)
            {
                // 如果是方法，保留最后两部分（类名.方法名）
                // 如果是类型，保留最后一部分（类名）
                if (cref.StartsWith("M:") && parts.Length >= 2)
                {
                    // 获取最后两部分：类名.方法名
                    return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
                }
                else
                {
                    // 其他情况只取最后一部分
                    return parts[parts.Length - 1];
                }
            }

            return displayName;
        }
    }
}
