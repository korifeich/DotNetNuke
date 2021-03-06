﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.239
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace DotNetNuke.Tests.BuildVerification
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Sites")]
    public partial class SitesFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SiteManagement.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Sites", "In order to customise my installation\r\nAs a host user\r\nI want to be able to creat" +
                    "e and maintain separate sites", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Add Child Site")]
        [NUnit.Framework.CategoryAttribute("ClearSmtpSettings")]
        public virtual void AddChildSite()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add Child Site", new string[] {
                        "ClearSmtpSettings"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I am on the site home page");
#line 9
 testRunner.And("I have logged in as the host");
#line 10
 testRunner.And("I am on the Host Page Site Management");
#line 11
 testRunner.When("I click the Add New Site Link");
#line 12
 testRunner.And("I fill in the portal form for a child portal with the alias testportal1 the title" +
                    " test portal");
#line 13
 testRunner.And("I select the current user for the site admin");
#line 14
 testRunner.And("I click the Create Site Link");
#line 15
 testRunner.Then("The child portal with the alias testportal1 and the title test portal should be c" +
                    "reated correctly");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
