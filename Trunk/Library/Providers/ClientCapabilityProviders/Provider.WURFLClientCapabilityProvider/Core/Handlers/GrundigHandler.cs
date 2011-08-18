/* 
 * This file is released under the GNU General Public License. 
 * Refer to the COPYING file distributed with this package.
 *
 * Copyright (c) 2008 WURFL-Pro S.r.l.
 */
namespace DotNetNuke.Services.ClientCapability.Hanldlers
{
    internal class GrundigHandler : AbstractHandler
    {
        public GrundigHandler(string id) : base(id)
        {
        }

        /// <summary>
        /// Intercept all UAs starting with "Grundig" or "GRUNDIG"
        /// </summary>
        /// <param name="userAgent">The user agent.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified user agent; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanHandle(string userAgent)
        {
            return (userAgent.StartsWith("Grundig") || userAgent.StartsWith("GRUNDIG"));
        }
    }
}