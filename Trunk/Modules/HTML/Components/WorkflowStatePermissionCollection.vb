'
' DotNetNukeŽ - http://www.dotnetnuke.com
' Copyright (c) 2002-2010
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Imports System
Imports System.Collections.Generic
Imports DotNetNuke.Common.Utilities

Namespace DotNetNuke.Security.Permissions

    ''' -----------------------------------------------------------------------------
    ''' Project	 : DotNetNuke
    ''' Namespace: DotNetNuke.Security.Permissions
    ''' Class	 : WorkflowStatePermissionCollection
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' DesktopModulePermissionCollection provides the a custom collection for WorkflowStatePermissionInfo
    ''' objects
    ''' </summary>
    ''' <history>
    ''' </history>
    ''' -----------------------------------------------------------------------------
    <Serializable()> Public Class WorkflowStatePermissionCollection
        Inherits CollectionBase

#Region "Constructors"

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal WorkflowStatePermissions As ArrayList)
            AddRange(WorkflowStatePermissions)
        End Sub

        Public Sub New(ByVal WorkflowStatePermissions As WorkflowStatePermissionCollection)
            AddRange(WorkflowStatePermissions)
        End Sub

        Public Sub New(ByVal WorkflowStatePermissions As ArrayList, ByVal WorkflowStatePermissionID As Integer)
            For Each permission As WorkflowStatePermissionInfo In WorkflowStatePermissions
                If permission.WorkflowStatePermissionID = WorkflowStatePermissionID Then
                    Add(permission)
                End If
            Next
        End Sub

#End Region

#Region "Public Properties"

        Default Public Property Item(ByVal index As Integer) As WorkflowStatePermissionInfo
            Get
                Return CType(List(index), WorkflowStatePermissionInfo)
            End Get
            Set(ByVal Value As WorkflowStatePermissionInfo)
                List(index) = Value
            End Set
        End Property

#End Region

#Region "Public Methods"

        Public Function Add(ByVal value As WorkflowStatePermissionInfo) As Integer
            Return List.Add(value)
        End Function

        Public Function Add(ByVal value As WorkflowStatePermissionInfo, ByVal checkForDuplicates As Boolean) As Integer
            Dim id As Integer = Null.NullInteger
            If Not checkForDuplicates Then
                id = Add(value)
            Else
                Dim isMatch As Boolean = False
                For Each permission As PermissionInfoBase In Me.List
                    If permission.PermissionID = value.PermissionID AndAlso permission.UserID = value.UserID AndAlso permission.RoleID = value.RoleID Then
                        isMatch = True
                        Exit For
                    End If
                Next
                If Not isMatch Then
                    id = Add(value)
                End If
            End If
            Return id
        End Function

        Public Sub AddRange(ByVal WorkflowStatePermissions As ArrayList)
            For Each permission As WorkflowStatePermissionInfo In WorkflowStatePermissions
                Add(permission)
            Next
        End Sub

        Public Sub AddRange(ByVal WorkflowStatePermissions As WorkflowStatePermissionCollection)
            For Each permission As WorkflowStatePermissionInfo In WorkflowStatePermissions
                Add(permission)
            Next
        End Sub

        Public Function CompareTo(ByVal objWorkflowStatePermissionCollection As WorkflowStatePermissionCollection) As Boolean
            If objWorkflowStatePermissionCollection.Count <> Me.Count Then
                Return False
            End If
            InnerList.Sort(New CompareWorkflowStatePermissions)
            objWorkflowStatePermissionCollection.InnerList.Sort(New CompareWorkflowStatePermissions)

            For i As Integer = 0 To Me.Count - 1
                If objWorkflowStatePermissionCollection(i).WorkflowStatePermissionID <> Me(i).WorkflowStatePermissionID _
                OrElse objWorkflowStatePermissionCollection(i).AllowAccess <> Me(i).AllowAccess Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Function Contains(ByVal value As WorkflowStatePermissionInfo) As Boolean
            Return List.Contains(value)
        End Function

        Public Function IndexOf(ByVal value As WorkflowStatePermissionInfo) As Integer
            Return List.IndexOf(value)
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As WorkflowStatePermissionInfo)
            List.Insert(index, value)
        End Sub

        Public Sub Remove(ByVal value As WorkflowStatePermissionInfo)
            List.Remove(value)
        End Sub

        Public Sub Remove(ByVal permissionID As Integer, ByVal roleID As Integer, ByVal userID As Integer)
            For Each permission As PermissionInfoBase In Me.List
                If permission.PermissionID = permissionID AndAlso permission.UserID = userID AndAlso permission.RoleID = roleID Then
                    List.Remove(permission)
                    Exit For
                End If
            Next
        End Sub

        Public Function ToList() As List(Of PermissionInfoBase)
            Dim list As New List(Of PermissionInfoBase)

            For Each permission As PermissionInfoBase In Me.List
                list.Add(permission)
            Next
            Return list
        End Function

        Public Overloads Function ToString(ByVal key As String) As String
            Return PermissionController.BuildPermissions(List, key)
        End Function
#End Region

    End Class

End Namespace
