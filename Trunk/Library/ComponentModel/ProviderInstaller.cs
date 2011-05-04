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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Compilation;

using DotNetNuke.Framework.Providers;

#endregion

namespace DotNetNuke.ComponentModel
{
    public class ProviderInstaller : IComponentInstaller
    {
        private readonly ComponentLifeStyleType _ComponentLifeStyle;
        private readonly Type _ProviderInterface;
        private readonly string _ProviderType;

        public ProviderInstaller(string providerType, Type providerInterface)
        {
            _ComponentLifeStyle = ComponentLifeStyleType.Singleton;
            _ProviderType = providerType;
            _ProviderInterface = providerInterface;
        }

        public ProviderInstaller(string providerType, Type providerInterface, ComponentLifeStyleType lifeStyle)
        {
            _ComponentLifeStyle = lifeStyle;
            _ProviderType = providerType;
            _ProviderInterface = providerInterface;
        }

        #region IComponentInstaller Members

        public void InstallComponents(IContainer container)
        {
            ProviderConfiguration config = ProviderConfiguration.GetProviderConfiguration(_ProviderType);
            if (config != null)
            {
                InstallProvider(container, (Provider) config.Providers[config.DefaultProvider]);
                foreach (Provider provider in config.Providers.Values)
                {
                    if (!config.DefaultProvider.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        InstallProvider(container, provider);
                    }
                }
            }
        }

        #endregion

        private void InstallProvider(IContainer container, Provider provider)
        {
            if (provider != null)
            {
                Type type = BuildManager.GetType(provider.Type, false, true);
                if (type == null)
                {
                    throw new ConfigurationErrorsException(string.Format("Could not load provider {0}", provider.Type));
                }
                container.RegisterComponent(provider.Name, _ProviderInterface, type, _ComponentLifeStyle);
                var settingsDict = new Dictionary<string, string>();
                settingsDict.Add("providerName", provider.Name);
                foreach (string key in provider.Attributes.Keys)
                {
                    settingsDict.Add(key, provider.Attributes.Get(key));
                }
                container.RegisterComponentSettings(type.FullName, settingsDict);
            }
        }
    }
}