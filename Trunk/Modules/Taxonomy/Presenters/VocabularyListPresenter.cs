#region Copyright

// 
// DotNetNukeŽ - http://www.dotnetnuke.com
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
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Modules.Taxonomy.Views;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.Web.Mvp;


#endregion

namespace DotNetNuke.Modules.Taxonomy.Presenters
{
    public class VocabularyListPresenter : ModulePresenter<IVocabularyListView, VocabularyListModel>
    {
        private readonly IVocabularyController _VocabularyController;

        #region "Constructors"

        public VocabularyListPresenter(IVocabularyListView view) : this(view, new VocabularyController(new DataService()))
        {
        }

        public VocabularyListPresenter(IVocabularyListView listView, IVocabularyController vocabularyController) : base(listView)
        {
            Requires.NotNull("vocabularyController", vocabularyController);

            _VocabularyController = vocabularyController;

            View.AddVocabulary += AddVocabulary;
        }

        #endregion

        #region "Protected Methods"

        protected override void OnInit()
        {
            base.OnInit();

            View.Model.Vocabularies =
                (from v in _VocabularyController.GetVocabularies() where v.ScopeType.Type == "Application" || (v.ScopeType.Type == "Portal" && v.ScopeId == PortalId) select v).ToList();

            View.Model.IsEditable = IsEditable;
            View.Model.NavigateUrlFormatString = Globals.NavigateURL(TabId, "EditVocabulary", string.Format("mid={0}", ModuleId), "VocabularyId={0}");
        }

        #endregion

        protected override void OnLoad()
        {
            base.OnLoad();

            View.ShowAddButton(IsEditable);
        }

        #region "Public Methods"

        public void AddVocabulary(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(TabId, "CreateVocabulary", string.Format("mid={0}", ModuleId)));
        }

        #endregion
    }
}