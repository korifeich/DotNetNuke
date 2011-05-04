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

using System;
using System.Data;
using System.Drawing;
using System.IO;

using DotNetNuke.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using MbUnit.Framework;

using Moq;

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class FileManagerTests
    {
        #region Private Variables

        private FileManager _fileManager;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFolderPermissionController> _folderPermissionController;
        private Mock<IPortalController> _portalController;
        private Mock<IFolderMappingController> _folderMappingController;
        private Mock<IGlobals> _globals;
        private Mock<ICBO> _cbo;
        private Mock<DataProvider> _mockData;
        private Mock<FolderProvider> _mockFolder;
        private Mock<CachingProvider> _mockCache;
        private Mock<FileManager> _mockFileManager;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;

        #endregion

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
            _mockData = MockComponentProvider.CreateDataProvider();
            _mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            _mockCache = MockComponentProvider.CreateDataCacheProvider();

            _folderManager = new Mock<IFolderManager>();
            _folderPermissionController = new Mock<IFolderPermissionController>();
            _portalController = new Mock<IPortalController>();
            _folderMappingController = new Mock<IFolderMappingController>();
            _globals = new Mock<IGlobals>();
            _cbo = new Mock<ICBO>();

            _fileManager = new FileManager();

            FolderManager.RegisterInstance(_folderManager.Object);
            FolderPermissionControllerWrapper.RegisterInstance(_folderPermissionController.Object);
            PortalControllerWrapper.RegisterInstance(_portalController.Object);
            FolderMappingController.RegisterInstance(_folderMappingController.Object);
            GlobalsWrapper.RegisterInstance(_globals.Object);
            CBOWrapper.RegisterInstance(_cbo.Object);

            _mockFileManager = new Mock<FileManager> { CallBase = true };

            _folderInfo = new Mock<IFolderInfo>();
            _fileInfo = new Mock<IFileInfo>();
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        #endregion

        #region AddFile

        [Test]
        [ExpectedArgumentException]
        public void AddFile_Throws_On_Null_Folder()
        {
            _fileManager.AddFile(null, It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [Row(null)]
        [Row("")]
        [ExpectedArgumentException]
        public void AddFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            _fileManager.AddFile(_folderInfo.Object, fileName, It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [ExpectedArgumentException]
        public void AddFile_Throws_On_Null_FileContent()
        {
            _fileManager.AddFile(_folderInfo.Object, It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void AddFile_Throws_When_Permissions_Are_Not_Met()
        {
            _folderPermissionController.Setup(fpc => fpc.CanAddFolder(_folderInfo.Object)).Returns(false);

            _fileManager.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(), It.IsAny<bool>(), true, It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(NoSpaceAvailableException))]
        public void AddFile_Throws_When_Portal_Has_No_Space_Available()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(false);

            _fileManager.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
        }

        [Test]
        [ExpectedException(typeof(InvalidFileExtensionException))]
        public void AddFile_Throws_When_Extension_Is_Invalid()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(false);

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
        }

        [Test]
        [Ignore]
        public void AddFile_Calls_FolderProvider_AddFile_When_Overwritting_Or_File_Does_Not_Exist()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            _globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.ExistsFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);
            _mockFolder.Setup(mf => mf.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            _mockData.Setup(
                md =>
                md.AddFile(It.IsAny<int>(),
                           It.IsAny<Guid>(),
                           It.IsAny<Guid>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<long>(),
                           It.IsAny<int>(),
                           It.IsAny<int>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<int>(),
                           It.IsAny<int>(),
                           It.IsAny<string>()))
               .Returns(Constants.FOLDER_ValidFileId);

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, false, Constants.CONTENTTYPE_ValidContentType);

            _mockFolder.Verify();
        }

        [Test]
        public void AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwritting_And_File_Exists()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            _globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.ExistsFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);
            _mockFolder.Setup(mf => mf.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent));

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);

            _mockData.Setup(
                md =>
                md.AddFile(It.IsAny<int>(),
                           It.IsAny<Guid>(),
                           It.IsAny<Guid>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<long>(),
                           It.IsAny<int>(),
                           It.IsAny<int>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<int>(),
                           It.IsAny<int>(),
                           It.IsAny<string>()))
               .Returns(Constants.FOLDER_ValidFileId);

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);

            _mockFolder.Verify(mf => mf.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());
        }

        #endregion

        #region CopyFile

        [Test]
        [ExpectedArgumentException]
        public void CopyFile_Throws_On_Null_File()
        {
            _fileManager.CopyFile(null, _folderInfo.Object);
        }

        [Test]
        [ExpectedArgumentException]
        public void CopyFile_Throws_On_Null_DestinationFolder()
        {
            _fileManager.CopyFile(_fileInfo.Object, null);
        }

        [Test]
        public void CopyFile_Calls_FileManager_AddFile()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.ContentType).Returns(Constants.CONTENTTYPE_ValidContentType);

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var fileContent = new MemoryStream(bytes);

            _mockFileManager.Setup(mfm => mfm.GetFileContent(_fileInfo.Object)).Returns(fileContent);
            _mockFileManager.Setup(mfm => mfm.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<Stream>(), true, true, Constants.CONTENTTYPE_ValidContentType));

            _mockFileManager.Object.CopyFile(_fileInfo.Object, _folderInfo.Object);

            _mockFileManager.Verify(fm => fm.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, true, Constants.CONTENTTYPE_ValidContentType), Times.Once());
        }

        #endregion

        #region DeleteFile

        [Test]
        [ExpectedArgumentException]
        public void DeleteFile_Throws_On_Null_File()
        {
            _fileManager.DeleteFile(null);
        }

        [Test]
        public void DeleteFile_Calls_FolderProvider_DeleteFile()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanAdminFolder(_folderInfo.Object)).Returns(true);

            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.DeleteFile(_fileInfo.Object)).Verifiable();

            _fileManager.DeleteFile(_fileInfo.Object);

            _mockFolder.Verify();
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void DeleteFile_Throws_When_FolderProvider_Throws()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanAdminFolder(_folderInfo.Object)).Returns(true);

            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.DeleteFile(_fileInfo.Object)).Throws<Exception>();

            _fileManager.DeleteFile(_fileInfo.Object);
        }

        #endregion

        #region WriteFileToResponse

        [Test]
        [ExpectedArgumentException]
        public void DownloadFile_Throws_On_Null_File()
        {
            _fileManager.WriteFileToResponse(null, ContentDisposition.Inline);
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void DownloadFile_Throws_When_Permissions_Are_Not_Met()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(false);

            _fileManager.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);
        }

        [Test]
        public void DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(true);
            _mockFileManager.Setup(mfm => mfm.AutoSyncFile(_fileInfo.Object)).Verifiable();

            _mockFileManager.Object.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);

            _mockFileManager.Verify();
        }

        [Test]
        public void DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);

            _mockFileManager.Object.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);

            _mockFileManager.Verify(mfm => mfm.AutoSyncFile(_fileInfo.Object), Times.Never());
        }

        [Test]
        public void DownloadFile_Calls_FileManager_WriteBytesToHttpContext()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            _mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(_fileInfo.Object, It.IsAny<ContentDisposition>())).Verifiable();

            _mockFileManager.Object.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);

            _mockFileManager.Verify();
        }

        #endregion

        #region FileExists

        [Test]
        [ExpectedArgumentException]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            _fileManager.FileExists(null, It.IsAny<string>());
        }

        [Test]
        [Row(null)]
        [Row("")]
        [ExpectedArgumentException]
        public void ExistsFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            _fileManager.FileExists(_folderInfo.Object, fileName);
        }

        [Test]
        public void ExistsFile_Calls_FileManager_GetFile()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns<IFileInfo>(null).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _mockFileManager.Verify();
        }

        [Test]
        public void ExistsFile_Calls_FolderProvider_ExistsFile()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.ExistsFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true).Verifiable();

            _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _mockFolder.Verify();
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.ExistsFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);

            var result = _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.ExistsFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);

            var result = _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void ExistsFile_Throws_When_FolderProvider_Throws()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.ExistsFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Throws<Exception>();

            _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);
        }

        #endregion

        #region GetFile

        [Test]
        [Row(null)]
        [Row("")]
        [ExpectedArgumentException]
        public void GetFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            _fileManager.GetFile(_folderInfo.Object, fileName);
        }

        [Test]
        public void GetFile_Calls_DataProvider_GetFile()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId)).Returns(It.IsAny<IDataReader>()).Verifiable();

            _fileManager.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _mockData.Verify();
        }

        #endregion

        #region GetFile

        [Test]
        public void GetFileByID_Calls_DataCache_GetCache_First()
        {
            _mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(_fileInfo.Object).Verifiable();

            _fileManager.GetFile(It.IsAny<int>());

            _mockCache.Verify();
        }

        [Test]
        public void GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()
        {
            _mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            _fileManager.GetFile(Constants.FOLDER_ValidFileId);

            _mockData.Verify(md => md.GetFileById(Constants.FOLDER_ValidFileId), Times.Once());
        }

        #endregion

        #region MoveFile

        [Test]
        [ExpectedArgumentException]
        public void MoveFile_Throws_On_Null_File()
        {
            _fileManager.MoveFile(null, _folderInfo.Object);
        }

        [Test]
        [ExpectedArgumentException]
        public void MoveFile_Throws_On_Null_DestinationFolder()
        {
            _fileManager.MoveFile(_fileInfo.Object, null);
        }

        [Test]
        public void MoveFile_Calls_FileManager_CopyFile_And_DeleteFile()
        {
            _mockFileManager.Setup(mfm => mfm.CopyFile(_fileInfo.Object, _folderInfo.Object)).Verifiable();
            _mockFileManager.Setup(mfm => mfm.DeleteFile(_fileInfo.Object)).Verifiable();

            _mockFileManager.Object.MoveFile(_fileInfo.Object, _folderInfo.Object);

            _mockFileManager.Verify();
        }

        #endregion

        #region RenameFile

        [Test]
        [ExpectedArgumentException]
        public void RenameFile_Throws_On_Null_File()
        {
            _fileManager.RenameFile(null, It.IsAny<string>());
        }

        [Test]
        [Row(null)]
        [Row("")]
        [ExpectedArgumentException]
        public void RenameFile_Throws_On_Null_Or_Empty_NewFileName(string newFileName)
        {
            _fileManager.RenameFile(_fileInfo.Object, newFileName);
        }

        [Test]
        public void RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _mockFileManager.Setup(mfm => mfm.FileExists(_folderInfo.Object, Constants.FOLDER_OtherValidFileName)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.UpdateFile(_fileInfo.Object));

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            _mockFolder.Verify(mf => mf.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName), Times.Once());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            _fileManager.RenameFile(_fileInfo.Object, Constants.FOLDER_ValidFileName);

            _mockFolder.Verify(mf => mf.RenameFile(_fileInfo.Object, It.IsAny<string>()), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(FileAlreadyExistsException))]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _mockFileManager.Setup(mfm => mfm.FileExists(_folderInfo.Object, Constants.FOLDER_OtherValidFileName)).Returns(true);

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void RenameFile_Throws_When_FolderProvider_Throws()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _mockFileManager.Setup(mfm => mfm.FileExists(_folderInfo.Object, Constants.FOLDER_OtherValidFileName)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.UpdateFile(_fileInfo.Object));

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName)).Throws<Exception>();

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName);
        }

        #endregion

        #region UnzipFile

        [Test]
        [ExpectedArgumentException]
        public void UnzipFile_Throws_On_Null_File()
        {
            _fileManager.UnzipFile(null, It.IsAny<IFolderInfo>());
        }

        [Test]
        [ExpectedArgumentException]
        public void UnzipFile_Throws_On_Null_DestinationFolder()
        {
            _fileManager.UnzipFile(It.IsAny<IFileInfo>(), null);
        }

        [Test]
        [ExpectedArgumentException]
        public void UnzipFile_Throws_When_File_Extension_Is_Not_Zip()
        {
            _fileInfo.Setup(fi => fi.Extension).Returns("txt");

            _fileManager.UnzipFile(_fileInfo.Object, It.IsAny<IFolderInfo>());
        }

        [Test]
        public void UnzipFile_Calls_FileManager_ExtractFolders_And_ExtractFiles()
        {
            _fileInfo.Setup(fi => fi.Extension).Returns("zip");

            _mockFileManager.Setup(mfm => mfm.ExtractFolders(_fileInfo.Object, _folderInfo.Object)).Verifiable();
            _mockFileManager.Setup(mfm => mfm.ExtractFiles(_fileInfo.Object, _folderInfo.Object)).Verifiable();

            _mockFileManager.Object.UnzipFile(_fileInfo.Object, _folderInfo.Object);

            _mockFileManager.Verify();
        }

        #endregion

        #region UpdateFile

        [Test]
        [ExpectedArgumentException]
        public void UpdateFile_Throws_On_Null_File()
        {
            _fileManager.UpdateFile(null);
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFile()
        {
            _mockFileManager.Object.UpdateFile(_fileInfo.Object);

            _mockData.Verify(md => md.UpdateFile(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()),
                Times.Once());
        }

        [Test]
        [ExpectedArgumentException]
        public void UpdateFile_Throws_On_Null_File_Overload()
        {
            _fileManager.UpdateFile(null, It.IsAny<Stream>());
        }

        [Test]
        [ExpectedArgumentException]
        public void UpdateFile_Throws_On_Null_Stream()
        {
            _fileManager.UpdateFile(_fileInfo.Object, null);
        }

        [Test]
        public void UpdateFile_Sets_With_And_Height_When_File_Is_Image()
        {
            var image = new Bitmap(10, 20);

            _mockFileManager.Setup(mfm => mfm.IsImageFile(_fileInfo.Object)).Returns(true);
            _mockFileManager.Setup(mfm => mfm.GetImageFromStream(It.IsAny<Stream>())).Returns(image);

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            _mockFileManager.Object.UpdateFile(_fileInfo.Object, stream);

            _fileInfo.VerifySet(fi => fi.Width = 10);
            _fileInfo.VerifySet(fi => fi.Height = 20);
        }

        [Test]
        public void UpdateFile_Sets_SHA1Hash()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            _mockFileManager.Setup(mfm => mfm.IsImageFile(_fileInfo.Object)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.GetHash(stream)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            _mockFileManager.Object.UpdateFile(_fileInfo.Object, stream);

            _fileInfo.VerifySet(fi => fi.SHA1Hash = Constants.FOLDER_UnmodifiedFileHash);
        }

        [Test]
        public void UpdateFile_Calls_FileManager_UpdateFile_Overload()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            _mockFileManager.Setup(mfm => mfm.IsImageFile(_fileInfo.Object)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.GetHash(stream)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            _mockFileManager.Object.UpdateFile(_fileInfo.Object, stream);

            _mockFileManager.Verify(mfm => mfm.UpdateFile(_fileInfo.Object), Times.Once());
        }

        #endregion
    }
}