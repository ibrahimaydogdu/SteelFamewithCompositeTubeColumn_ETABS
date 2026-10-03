Imports System.Xml
Imports System.IO
Public Class General
    Public Structure STEEL_I_SECTION
        Public SectionName As String
        Public Designation As String
        Public Depth As Double
        Public FlangeLength As Double
        Public FlangeTickness As Double
        Public WebTickness As Double
        Public KDES As Double
        Public Area As Double
        Public Imajor As Double
        Public PlasticModulusMajor As Double
        Public ShearAreaMajor As Double
        Public Iminor As Double
        Public PlasticModulusMinor As Double
        Public ShearAreaMinor As Double
        Public TorsionalConstant As Double
        Public SectionModulusMajorPos As Double
        Public SectionModulusMajorNeg As Double
        Public SectionModulusMinorPos As Double
        Public SectionModulusMinorNeg As Double
        Public RadiusofGyrationMajor As Double
        Public RadiusofGyrationMinor As Double
    End Structure
    Public Class Sections
        Public WSections As IEnumerable(Of STEEL_I_SECTION)
        Private Sub ReadSections()
            Dim xmldoc As New XmlDataDocument()
            Dim xmlnode As XmlNodeList
            Dim i As Integer
            Dim ISections() As STEEL_I_SECTION
            Dim fs As New FileStream("AISC14M.xml", FileMode.Open, FileAccess.Read)
            xmldoc.Load(fs)
            xmlnode = xmldoc.GetElementsByTagName("STEEL_I_SECTION")
            ReDim ISections(xmlnode.Count - 1)
            For i = 0 To xmlnode.Count - 1
                ISections(i).SectionName = xmlnode(i).ChildNodes.Item(0).InnerText
                ISections(i).Designation = xmlnode(i).ChildNodes.Item(2).InnerText
                ISections(i).Depth = xmlnode(i).ChildNodes.Item(3).InnerText
                ISections(i).FlangeLength = xmlnode(i).ChildNodes.Item(4).InnerText
                ISections(i).FlangeTickness = xmlnode(i).ChildNodes.Item(5).InnerText
                ISections(i).WebTickness = xmlnode(i).ChildNodes.Item(6).InnerText
                ISections(i).KDES = xmlnode(i).ChildNodes.Item(7).InnerText
                ISections(i).Area = xmlnode(i).ChildNodes.Item(8).InnerText
                ISections(i).Imajor = xmlnode(i).ChildNodes.Item(9).InnerText
                ISections(i).PlasticModulusMajor = xmlnode(i).ChildNodes.Item(10).InnerText
                ISections(i).ShearAreaMajor = xmlnode(i).ChildNodes.Item(11).InnerText
                ISections(i).Iminor = xmlnode(i).ChildNodes.Item(12).InnerText
                ISections(i).PlasticModulusMinor = xmlnode(i).ChildNodes.Item(13).InnerText
                ISections(i).ShearAreaMinor = xmlnode(i).ChildNodes.Item(14).InnerText
                ISections(i).TorsionalConstant = xmlnode(i).ChildNodes.Item(15).InnerText
                ISections(i).SectionModulusMajorPos = xmlnode(i).ChildNodes.Item(16).InnerText
                ISections(i).SectionModulusMajorNeg = xmlnode(i).ChildNodes.Item(17).InnerText
                ISections(i).SectionModulusMinorPos = xmlnode(i).ChildNodes.Item(18).InnerText
                ISections(i).SectionModulusMinorNeg = xmlnode(i).ChildNodes.Item(19).InnerText
                ISections(i).RadiusofGyrationMajor = xmlnode(i).ChildNodes.Item(20).InnerText
                ISections(i).RadiusofGyrationMinor = xmlnode(i).ChildNodes.Item(21).InnerText
            Next
            WSections = ISections.Where(Function(c) c.Designation = "W").OrderBy(Function(c) c.Area)

        End Sub
    End Class

    Public Class ETABSCommands
        Public SapModel As ETABSv1.cSapModel
        Public myETABSObject As ETABSv1.cOAPI
        'Initilizate model
        Public Sub InitilizeETABS()
            Dim SapModel As ETABSv1.cSapModel
            Dim myETABSObject As ETABSv1.cOAPI
            'myETABSObject = Nothing 'dimension the ETABS Object as cOAPI type
            'SapModel = Nothing
            Dim ret As Integer 'Use ret to check if functions return successfully (ret = 0) or fail (ret = nonzero)
            'set the following flag to true to attach to a running instance of the program 
            'otherwise a new instance of the program will be started 
            Dim AttachToInstance As Boolean = False

            'full path to the program executable set it to the installation folder 
            'Note, the path below may need to be modified if you have ETABS installed in a different location 
            Dim ProgramPath As String = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("PROGRAMFILES"), "Computers and Structures", "ETABS v1", "ETABS.exe")
            If AttachToInstance Then
                'attach to a running instance of ETABS 
                Try
                    'get the active ETABS object
                    myETABSObject = DirectCast(System.Runtime.InteropServices.Marshal.GetActiveObject("CSI.ETABS.API.ETABSObject"), ETABSv1.cOAPI)
                Catch ex As Exception
                    MsgBox("No running instance of the program found or failed to attach.")
                    Return
                End Try
            Else
                'create a new instance of ETABS 
                Try

                    'create API helper object 
                    Dim myHelper As ETABSv1.cHelper

                    myHelper = New ETABSv1.Helper

                    'create ETABS object
                    myETABSObject = myHelper.CreateObject(ProgramPath)
                Catch ex As Exception
                    MsgBox("Cannot start a new instance of the program.")
                    Return
                End Try
                'start ETABS application
                ret = myETABSObject.ApplicationStart()
            End If

            'Get a reference to cSapModel to access all OAPI classes and functions 
            SapModel = myETABSObject.SapModel
            Return
        End Sub
        'Open ETABS file
        Public Function OpenFile(ByVal Path As String)
            OpenFile = SapModel.File.OpenFile(Path)     'open an existing file
        End Function
        'Close ETABS

    End Class
    Public Sub InitilizeProg(ByVal Path As String)
        Dim Command As ETABSCommands
        Dim Main As Main
        Command.InitilizeETABS()
        Command.OpenFile(Main.saplocation.Text)
    End Sub
End Class