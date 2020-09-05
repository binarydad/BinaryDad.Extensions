using System.Xml;

namespace BinaryDad.Extensions
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Null-safe retrieval of a <see cref="XmlNode.Attributes"/> collection. Returns null if node is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static T GetAttributeValue<T>(this XmlNode node, string attribute)
        {
            var selectedAttribute = node?.Attributes?[attribute];

            if (selectedAttribute != null)
            {
                return selectedAttribute.Value.To<T>();
            }

            return default;
        }

        /// <summary>
        /// Null-safe retrieval of a <see cref="XmlNode.Attributes"/> collection. Returns null if node is null.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this XmlNode node, string attribute) => node.GetAttributeValue<string>(attribute);

        /// <summary>
        /// Null-safe retrieval of a <see cref="XmlNode.InnerText"/>. Returns null if node is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static T GetInnerText<T>(this XmlNode node)
        {
            if (node?.InnerText != null)
            {
                return node.InnerText.To<T>();
            }

            return default;
        }

        /// <summary>
        /// Null-safe retrieval of a <see cref="XmlNode.InnerText"/>. Returns null if node is null.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetInnerText(this XmlNode node) => node.GetInnerText<string>();
    }
}
