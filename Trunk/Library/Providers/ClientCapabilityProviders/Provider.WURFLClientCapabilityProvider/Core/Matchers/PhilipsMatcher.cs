/* 
 * This file is released under the GNU General Public License. 
 * Refer to the COPYING file distributed with this package.
 *
 * Copyright (c) 2008 WURFL-Pro S.r.l.
 */
using DotNetNuke.Services.ClientCapability.Core.Classifiers;
using DotNetNuke.Services.ClientCapability.Hanldlers;

namespace DotNetNuke.Services.ClientCapability.Matchers
{
    internal class PhilipsMatcher : AbstractMatcher
    {
        public PhilipsMatcher(Classification classification, IHandler<string> handler)
            : base(classification, handler)
        {
        }
    }
}