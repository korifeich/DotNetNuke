﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.261
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace DotNetNuke.Website.Specs.AdminFeatures
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Site Wizard")]
    public partial class SiteWizardFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SiteWizard.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Site Wizard", "In order to re-create the site\r\nAs n admin\r\nI want to use site wizard correctly", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Site should apply settings from site wizard")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void SiteShouldApplySettingsFromSiteWizard()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Site should apply settings from site wizard", new string[] {
                        "MustBeDefaultAdminCredentials"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I am on the site home page");
#line 9
 testRunner.And("I have logged in as the admin");
#line 10
 testRunner.When("I navigate to the admin page Site Wizard");
#line 11
 testRunner.And("I click the Site Wizard Next button");
#line 12
 testRunner.And("I click the Site Wizard Next button");
#line 13
 testRunner.And("I select the skin or container 2-Column-Left-Mega-Menu");
#line 14
 testRunner.And("I click the Site Wizard Next button");
#line 15
 testRunner.And("I select the skin or container PageTitle_Blue");
#line 16
 testRunner.And("I click the Site Wizard Next button");
#line 17
 testRunner.And("I set site name to New Site");
#line 18
 testRunner.And("I click the Site Wizard Finish button");
#line 19
 testRunner.And("I navigate to the admin page Site Settings");
#line 20
 testRunner.Then("Site\'s default skin should be 2-Column-Left-Mega-Menu");
#line 21
 testRunner.And("Site\'s default container should be PageTitle_Blue");
#line 22
 testRunner.And("Site\'s name should be New Site");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
