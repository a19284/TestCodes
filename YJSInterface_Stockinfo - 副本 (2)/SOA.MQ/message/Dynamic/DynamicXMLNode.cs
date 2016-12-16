﻿using System;
using System.Dynamic;
using System.Xml.Linq;
using System.Reflection;

namespace SOA.message.Dynamic
{
    /// <summary>
    /// 参考 http://blogs.msdn.com/b/csharpfaq/archive/2009/10/19/dynamic-in-c-4-0-creating-wrappers-with-dynamicobject.aspx
    /// </summary>
    public class DynamicXMLNode : DynamicObject
    {
        XElement node;

        public DynamicXMLNode(XElement node)
        {
            this.node = node;
        }

        public DynamicXMLNode()
        {
        }

        public DynamicXMLNode(String name)
        {
            node = new XElement(name);
        }

        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            XElement setNode = node.Element(binder.Name);
            if (setNode != null)
                setNode.SetValue(value);
            else
            {
                if (value.GetType() == typeof(DynamicXMLNode))
                    node.Add(new XElement(binder.Name));
                else
                    node.Add(new XElement(binder.Name, value));
            }
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            XElement getNode = node.Element(binder.Name);
            if (getNode != null)
            {
                result = new DynamicXMLNode(getNode);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TryConvert(
    ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(String))
            {
                result = node.Value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TryInvokeMember(
    InvokeMemberBinder binder, object[] args, out object result)
        {
            Type xmlType = typeof(XElement);
            try
            {
                result = xmlType.InvokeMember(
                          binder.Name,
                          BindingFlags.InvokeMethod |
                          BindingFlags.Public |
                          BindingFlags.Instance,
                          null, node, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
