using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Xml.Linq;

namespace SOA.message.Dynamic
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlMarkupBuilder : DynamicObject
    {
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            XElement xml = new XElement(binder.Name);

            var attributeCount = binder.CallInfo.ArgumentNames.Count;
            var elementCount = args.Length - attributeCount;

            //添加XML元素
            for (int i = 0; i < elementCount; i++)
            {
                xml.Add(args[i]);
            }

            //添加特性
            for (var i = 0; i < attributeCount; i++)
            {
                var attributeName = binder.CallInfo.ArgumentNames[i];
                if (attributeName[0] == '@') attributeName = attributeName.Substring(1);

                xml.Add(new XAttribute(attributeName, args[i + elementCount]));
            }

            result = xml;
            return true;
        }
    }
}
