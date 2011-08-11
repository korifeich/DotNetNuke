﻿using NUnit.Framework;

namespace DotNetNuke.MSBuild.Tasks.Tests
{
    [TestFixture]
    class ExtensionPackagerFixture
    {
        [Test]
        public void ExtensionPackager_Loads_Manifest()
        {
            var extensionPackager = new ExtensionPackager()
                                        {
                                            Manifest = "dnn_HTML.dnn",
                                            WorkingFolder = @"C:\DotNetNuke\TFS\DotNetNuke\src\DotNetNuke_CS\Modules\HTML\"
                                        };

            Assert.IsTrue(extensionPackager.Execute());
        }

    }
}
