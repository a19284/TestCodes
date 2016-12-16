namespace IBM.WMQ
{
    using System;
    using System.Collections;
    using System.Text;

    internal class RFH2Folder : MQBase, IDisposable
    {
        private ArrayList children;
        private string content;
        internal string context;
        internal string copyOption;
        public const int DT_BINHEX = 2;
        public const int DT_BOOLEAN = 1;
        public const int DT_I1 = 3;
        public const int DT_I2 = 4;
        public const int DT_I4 = 5;
        public const int DT_I8 = 6;
        public const int DT_INT = 7;
        private string[] DT_LOOKUP_TABLE;
        private const int DT_NONE = -1;
        private const string DT_NULL_VALUE = " xsi:nil='true'>";
        public const int DT_R4 = 8;
        public const int DT_R8 = 9;
        public const int DT_STRING = 0;
        private const string FOLDER_CONTENT_ATTRIBUTE_SYNTAX = " content = 'properties'";
        internal string name;
        private bool ownRolled;
        private const string sccsid = "%Z% %W%  %I% %E% %U%";
        internal string support;
        private int type;
        private const string XML_CLOSE_TAG = ">";
        private const string XML_END_TAG = "</";
        private const string XML_MQPD_CONTEXT = "context=\"";
        private const string XML_MQPD_COPY = "copy=\"";
        private const string XML_MQPD_SUPPORT = "support=\"";
        private const string XML_OPEN_TAG = "<";

        internal RFH2Folder()
        {
            this.DT_LOOKUP_TABLE = new string[] { "string", "boolean", "bin.hex", "i1", "i2", "i4", "i8", "int", "r4", "r8" };
            this.type = -1;
            base.TrConstructor("%Z% %W%  %I% %E% %U%");
            this.ownRolled = true;
        }

        internal RFH2Folder(string folderName)
        {
            this.DT_LOOKUP_TABLE = new string[] { "string", "boolean", "bin.hex", "i1", "i2", "i4", "i8", "int", "r4", "r8" };
            this.type = -1;
            base.TrConstructor("%Z% %W%  %I% %E% %U%", new object[] { folderName });
            this.name = folderName;
        }

        internal void AddFolder(RFH2Folder child)
        {
            uint method = 0x2f5;
            this.TrEntry(method, new object[] { child });
            try
            {
                if (this.children == null)
                {
                    this.children = new ArrayList();
                }
                this.children.Add(child);
            }
            finally
            {
                base.TrExit(method);
            }
        }

        internal bool Contains(string folderName)
        {
            uint method = 0x2f2;
            this.TrEntry(method, new object[] { folderName });
            bool flag = false;
            try
            {
                if (this.children == null)
                {
                    return flag;
                }
                foreach (RFH2Folder folder in this.children)
                {
                    if (folder.name == folderName)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return flag;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool fromDispose)
        {
            this.children = null;
            if (!fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        internal RFH2Folder GetFolder(string folderName)
        {
            uint method = 0x2f3;
            this.TrEntry(method, new object[] { folderName });
            RFH2Folder result = null;
            if (this.children != null)
            {
                foreach (RFH2Folder folder2 in this.children)
                {
                    if (folder2.name == folderName)
                    {
                        result = folder2;
                    }
                }
            }
            base.TrExit(method, result);
            return result;
        }

        internal string Render()
        {
            uint method = 0x2f6;
            this.TrEntry(method);
            StringBuilder builder = null;
            try
            {
                builder = new StringBuilder();
                if (this.ownRolled)
                {
                    builder.Append(this.content);
                }
                else
                {
                    if (MQMessage.PROPERTIES_FOLDER_NAMES.Contains(this.name))
                    {
                        builder.Append("<" + this.name);
                    }
                    else
                    {
                        builder.Append("<" + this.name + " content = 'properties'");
                    }
                    if (this.type != -1)
                    {
                        builder.Append(" dt=\"" + this.DT_LOOKUP_TABLE[this.type] + "\"");
                    }
                    if ((this.children == null) && (this.content == null))
                    {
                        builder.Append(" xsi:nil='true'>");
                    }
                    else
                    {
                        builder.Append(">");
                    }
                    if (this.children != null)
                    {
                        int count = this.children.Count;
                        for (int i = 0; i < count; i++)
                        {
                            builder.Append(((RFH2Folder) this.children[i]).RenderNoPad());
                        }
                    }
                    else if (this.content != null)
                    {
                        builder.Append(this.content);
                    }
                    builder.Append("</" + this.name + ">");
                }
                int num4 = builder.Length % 4;
                if (num4 != 0)
                {
                    for (int j = num4; j < 4; j++)
                    {
                        builder.Append(" ");
                    }
                }
            }
            finally
            {
                base.TrExit(method);
            }
            return builder.ToString();
        }

        private string RenderNoPad()
        {
            uint method = 0x2f7;
            this.TrEntry(method);
            StringBuilder builder = null;
            try
            {
                builder = new StringBuilder();
                builder.Append("<" + this.name);
                if (this.type != -1)
                {
                    builder.Append(" dt=\"" + this.DT_LOOKUP_TABLE[this.type] + "\" ");
                }
                if ((this.context != null) && (this.content.Length > 0))
                {
                    builder.Append("context=\"" + this.context + "\" ");
                }
                if ((this.support != null) && (this.support.Length > 0))
                {
                    builder.Append("support=\"" + this.support + "\" ");
                }
                if ((this.copyOption != null) && (this.copyOption.Length > 0))
                {
                    builder.Append("copy=\"" + this.copyOption + "\" ");
                }
                if ((this.children == null) && (this.content == null))
                {
                    builder.Append(" xsi:nil='true'>");
                }
                else
                {
                    builder.Append(">");
                }
                if (this.children != null)
                {
                    int count = this.children.Count;
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append(((RFH2Folder) this.children[i]).RenderNoPad());
                    }
                }
                else if (this.content != null)
                {
                    builder.Append(this.content);
                }
                builder.Append("</" + this.name + ">");
            }
            finally
            {
                base.TrExit(method);
            }
            return builder.ToString();
        }

        internal void SetContent(string setContent)
        {
            this.SetContent(setContent, -1);
        }

        internal void SetContent(string setContent, int setType)
        {
            uint method = 0x2f4;
            this.TrEntry(method, new object[] { setContent, setType });
            bool flag = false;
            try
            {
                if (this.children != null)
                {
                    flag = true;
                }
                else if (this.ownRolled && (this.type != -1))
                {
                    flag = true;
                }
                else if (this.content != null)
                {
                    flag = true;
                }
                else
                {
                    this.content = setContent;
                    this.type = setType;
                }
                if (flag)
                {
                    MQException ex = new MQException(2, 0x975);
                    base.TrException(method, ex);
                    throw ex;
                }
            }
            finally
            {
                base.TrExit(method);
            }
        }
    }
}

