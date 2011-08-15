﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
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

#endregion

namespace DotNetNuke.Services.Mobile
{
	public interface IRedirection
	{
		/// <summary>
		/// Primary ID.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Portal Id.
		/// </summary>
		int PortalId { get; }
		/// <summary>
		/// Redirection name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// if redirect by visit the whole portal, this value should be -1; 
		/// otherwise should be the exactly page id for redirection.
		/// </summary>
		int SourceTabId { get; }

		/// <summary>
		/// The redirection type: should be Mobile, Tablet, Both of mobile and tablet, and all other unknown devices.
		/// if this value is Other, should use MatchRules to match the special request need to redirect.
		/// </summary>
		RedirectionType Type { get; }

		/// <summary>
		/// request match rules.
		/// </summary>
		IList<IMatchRules> MatchRules { get; }

		/// <summary>
		/// Redirection target type.
		/// </summary>
		TargetType TargetType { get; }

		/// <summary>
		/// Redirection target value, can a portal id, tab id or a specific url.
		/// </summary>
		object TargetValue { get; }

		/// <summary>
		/// Redirection's Order
		/// </summary>
		int SortOrder { get; }
	}
}
