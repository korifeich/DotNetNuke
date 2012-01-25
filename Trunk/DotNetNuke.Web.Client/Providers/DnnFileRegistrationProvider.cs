﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2012
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
namespace DotNetNuke.Web.Client.Providers
{
    using System.Web;
    using ClientDependency.Core.FileRegistration.Providers;

    public abstract class DnnFileRegistrationProvider : WebFormsFileRegistrationProvider
    {
        public override int GetVersion(HttpContextBase http)
        {
            var portalHelper = new PortalHelper();
            var version = portalHelper.GetPortalVersion(http);
            return version.HasValue ? version.Value : base.GetVersion(http);
        }

        /// <summary>
        /// Checks if the composite files option is set for the current portal (DNN site settings).
        /// If not enabled at the portal level it defers to the core CDF setting (web.config).
        /// </summary>
        public override bool EnableCompositeFiles
        {
            get
            {
                var portalHelper = new PortalHelper();
                var optionSetForPortal = portalHelper.IsCompositeFilesOptionSetForPortal();
                return optionSetForPortal.HasValue ? optionSetForPortal.Value : base.EnableCompositeFiles;
            }
        }
    }
}