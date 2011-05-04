#region Copyright

// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System.Collections.Generic;
using System.Xml;

using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinComponentWriter class handles creating the manifest for Skin Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	02/04/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class SkinComponentWriter : FileComponentWriter
    {
        private readonly string _SkinName;

        public SkinComponentWriter(string skinName, string basePath, Dictionary<string, InstallFile> files, PackageInfo package) : base(basePath, files, package)
        {
            _SkinName = skinName;
        }

        protected override string CollectionNodeName
        {
            get
            {
                return "skinFiles";
            }
        }

        protected override string ComponentType
        {
            get
            {
                return "Skin";
            }
        }

        protected override string ItemNodeName
        {
            get
            {
                return "skinFile";
            }
        }

        protected virtual string SkinNameNodeName
        {
            get
            {
                return "skinName";
            }
        }

        protected override void WriteCustomManifest(XmlWriter writer)
        {
            writer.WriteElementString(SkinNameNodeName, _SkinName);
        }
    }
}