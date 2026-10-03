Imports ETABSv1
Public Class Main
    Public SectionName1_S(), SectionName2_S() As String
    Public cost_sec_com_s(), cost_sec_steel_s() As Double
    Public ret As Integer
    Private Sub evaluate(ByRef SapModel As ETABSv1.cSapModel)
        Dim sect_ind(Int(nofgroup.Text) - 1) As Integer
        Dim IsComposite(Int(nofgroup.Text) - 1) As Boolean
        sect_ind = {15, 18, 21, 36, 10, 11, 10, 8, 6, 4, 5, 3, 7, 6, 16}
        IsComposite = {False, False, False, False, True, True, True, True, True, True, True, True, True, True}
        '_______________________________________________________________________________________________
        'check if model is locked
        If SapModel.GetModelIsLocked = True Then                'Unlock model
            ret = SapModel.SetModelIsLocked(False)
            If (ret <> 0) Then : MsgBox("Problem occured on :Unlock model") : Stop : End If
        End If
        '_______________________________________________________________________________________________
        'set frame section property
        For i = 0 To Int(nofgroup.Text) - 1
            Name = CStr(i)
            If IsComposite(i) = False Then
                ret = SapModel.FrameObj.SetSection(Name, SectionName1_S(sect_ind(i)))
                If (ret <> 0) Then : MsgBox("Problem occured on :FrameObj.SetSection") : Stop : End If
            Else
                ret = SapModel.FrameObj.SetSection(Name, SectionName2_S(sect_ind(i)))
                If (ret <> 0) Then : MsgBox("Problem occured on :FrameObj.SetSection") : Stop : End If
            End If
        Next i
        '_______________________________________________________________________________________________
        'run model
        ret = SapModel.File.Save(saplocation.Text)
        If (ret <> 0) Then : MsgBox("Problem occured in :File.Save") : Stop : End If
        ret = SapModel.Analyze.RunAnalysis
        If (ret <> 0) Then : MsgBox("Problem occured on :RunAnalysis") : Stop : End If
        '_______________________________________________________________________________________________
        'start Steel design
        ret = SapModel.DesignSteel.SetCode(dcode.Text)
        If (ret <> 0) Then : MsgBox("Problem occured on :DesignSteel.SetCode") : Stop : End If
        ret = SapModel.DesignSteel.StartDesign
        If (ret <> 0) Then : MsgBox("Problem occured on :DesignSteel.StartDesign") : Stop : End If
        '_______________________________________________________________________________________________
        'Calculate Steel Cost
        Dim steelcost As Double
        Call calculate_steel_cost(steelcost, sect_ind, SapModel)
        '_______________________________________________________________________________________________
        'start composite Column design
        ret = SapModel.DesignCompositeBeam.StartDesign
        If (ret <> 0) Then : MsgBox("Problem occured on :DesignSteel.StartDesign") : Stop : End If


    End Sub
    Private Sub calculate_steel_cost(ByRef steelcost As Double, ByVal sect_ind() As Integer, ByRef SapModel As ETABSv1.cSapModel)
        steelcost = 0
        Dim GroupLenght(Int(nofgroup.Text) - 1) As Double
        Dim NumberItems, ObjectType() As Integer
        Dim ObjectName(), Point1, Point2 As String
        ReDim ObjectType(1), ObjectName(1)
        Dim x1, x2, y1, y2, z1, z2 As Double
        For j1 = 0 To Int(nofgroup.Text) - 1
            'get group assignments
            ret = SapModel.GroupDef.GetAssignments(CStr(j1 + 1), NumberItems, ObjectType, ObjectName)
            If (ret <> 0) Then : MsgBox("Problem occured on :GroupDef.GetAssignments") : Stop : End If
            GroupLenght(j1) = 0
            Point1 = ""
            Point2 = ""
            For k = 0 To NumberItems - 1
                'get names of points
                If ObjectType(k) = 2 Then ' if statement is put to use only frame member
                    ret = SapModel.FrameObj.GetPoints(ObjectName(k), Point1, Point2)
                    If (ret <> 0) Then : MsgBox("Problem occured on :FrameObj.GetPoints") : Stop : End If
                End If

                'get point coordinates of the point1
                ret = SapModel.PointObj.GetCoordCartesian(Point1, x1, y1, z1)
                If (ret <> 0) Then : MsgBox("Problem occured on :PointObj.GetCoordCartesian") : Stop : End If

                'get point coordinates of the point2
                ret = SapModel.PointObj.GetCoordCartesian(Point2, x2, y2, z2)
                If (ret <> 0) Then : MsgBox("Problem occured on :PointObj.GetCoordCartesian") : Stop : End If

                GroupLenght(j1) = GroupLenght(j1) + Math.Sqrt((x2 - x1) ^ 2 + (y2 - y1) ^ 2 + (z2 - z1) ^ 2)
            Next k

            steelcost = steelcost + cost_sec_steel_s(sect_ind(j1)) * GroupLenght(j1)
        Next j1
    End Sub
    Private Sub Read_Section(ByRef SapModel As ETABSv1.cSapModel)
        ret = SapModel.SetPresentUnits(6)
        If (ret <> 0) Then : MsgBox("Problem occured on :SetPresentUnits") : Stop : End If
        ret = SapModel.GroupDef.Count : nofgroup.Text = ret - 1           'return number of defined groups
        ret = SapModel.FrameObj.Count : nofmember.Text = ret      'return number of frame objects
        ret = SapModel.PointElm.Count : NofJoint.Text = ret             'return number of defined joint patterns
        '_______________________________________________________________________________________________
        'Read Section properties, calculate unit weight and unit cost of steel sections
        Dim t3(1), t2(1), tf(1), tw(1), t2b(1), tfb(1) As Double
        Dim NumberNames As Integer
        Dim Myname(1) As String
        Dim Proptype(1) As ETABSv1.eFramePropType
        ret = SapModel.PropFrame.GetAllFrameProperties(NumberNames, Myname, Proptype, t3, t2, tf, tw, t2b, tfb)
        If (ret <> 0) Then : MsgBox("PropFrame.GetAllFrameProperties") : Stop : End If
        Dim nofsteel, nofcomposite As Integer
        nofsteel = 0 : nofcomposite = 0
        For i = 0 To NumberNames - 1
            If Proptype(i) = 1 Then
                nofsteel = nofsteel + 1
            ElseIf Proptype(i) = 27 Then
                nofcomposite = nofcomposite + 1
            End If
        Next i
        nofsection1.Text = nofsteel : nofsection2.Text = nofcomposite
        Dim SectionName1(nofsteel - 1), SectionName2(nofcomposite - 1) As String
        Dim ist, iconc As Integer
        Dim area, as2, as3, Torsion, I22, I33, S22, S33, Z22, Z33, R22, R33 As Double
        Dim cost_sec_steel(nofsteel - 1), cost_sec_com(nofcomposite - 1) As Double
        ist = 0 : iconc = 0
        For i = 0 To NumberNames - 1
            If Proptype(i) = 1 Then
                SectionName1(ist) = Myname(i)
                'get frame section areas (ALL W sections)
                ret = SapModel.PropFrame.GetSectProps(SectionName1(ist), area, as2, as3, Torsion, I22, I33, S22, S33, Z22, Z33, R22, R33)
                If (ret <> 0) Then : MsgBox("Problem occured on :PropFrame.GetSectProps") : Stop : End If
                cost_sec_steel(ist) = area * 7849 * Val(Unit_C_Steel.Text)
                ist = ist + 1
            ElseIf Proptype(i) = 27 Then
                SectionName2(iconc) = Myname(i)
                cost_sec_com(iconc) = (t3(i) - 2 * tf(i)) * (t2(i) - 2 * tw(i)) * Val(Unit_C_Conc.Text) + (t3(i) * t2(i) - (t3(i) - 2 * tf(i)) * (t2(i) - 2 * tw(i))) * 7849 * Val(Unit_C_Steel.Text)
                iconc = iconc + 1
            End If
        Next i
        ReDim cost_sec_steel_s(nofsteel - 1), cost_sec_com_s(nofcomposite - 1), SectionName1_S(nofsteel - 1), SectionName2_S(nofcomposite - 1)
        Dim ind(nofsteel - 1) As Integer
        Call Sorting(nofsteel - 1, cost_sec_steel, nofsteel - 1, cost_sec_steel_s, ind, 1)
        For i = 0 To nofsteel - 1
            SectionName1_S(i) = SectionName1(ind(i))
        Next i
        ReDim ind(nofcomposite - 1)
        Call Sorting(nofcomposite - 1, cost_sec_com, nofcomposite - 1, cost_sec_com_s, ind, 1)
        For i = 0 To nofcomposite - 1
            SectionName2_S(i) = SectionName2(ind(i))
        Next i
    End Sub
    Private Sub Control(ByRef durdur As Integer)
        '_______________________________________________________________________________________________
        'Control Input Output file locations
        '_______________________________________________________________________________________________
        If My.Computer.FileSystem.FileExists(saplocation.Text) = False Then : MessageBox.Show("File Not Found: " & saplocation.Text) : durdur = True : Exit Sub : End If
        If My.Computer.FileSystem.FileExists(poollocation1.Text) = False Then : MessageBox.Show("File Not Found: " & poollocation1.Text) : durdur = True : Exit Sub : End If
        If My.Computer.FileSystem.FileExists(poollocation2.Text) = False Then : MessageBox.Show("File Not Found: " & poollocation2.Text) : durdur = True : Exit Sub : End If
        '______________________________________________________________________________________________
        'Control SSO Parameters
        '______________________________________________________________________________________________
        If Not IsNumeric(NofSpider.Text) Or NofSpider.Text = vbEmpty Then : MsgBox("Number of Spider is not defined correctly") : durdur = True : Exit Sub : End If
        If Not IsNumeric(maxiter.Text) Or maxiter.Text = vbEmpty Then : MsgBox("MaxIteration is not defined correctly") : durdur = True : Exit Sub : End If
        If Not IsNumeric(PrFemale.Text) Or PrFemale.Text = vbEmpty Then : MsgBox("Probability of female movement direction is not defined correctly") : durdur = True : Exit Sub : End If
        If Not IsNumeric(tole.Text) Or tole.Text = vbEmpty Then : MsgBox("Tolerance is not defined correctly") : durdur = True : Exit Sub : End If
        '______________________________________________________________________________________________
        'Control Frame Parameters
        '______________________________________________________________________________________________
        If Not IsNumeric(displimit.Text) Or displimit.Text = vbEmpty Then : MsgBox("Displacement limit is not defined correctly") : durdur = True : Exit Sub : End If
        If Not IsNumeric(TS_Limit.Text) Or TS_Limit.Text = vbEmpty Then : MsgBox("Top Story limit is not defined correctly") : durdur = True : Exit Sub : End If
        If Not IsNumeric(IS_Limit.Text) Or IS_Limit.Text = vbEmpty Then : MsgBox("Inter Story limit is not defined correctly") : durdur = True : Exit Sub : End If
    End Sub
    Private Sub Initilize(ByRef SapModel As ETABSv1.cSapModel, ByRef myETABSObject As ETABSv1.cOAPI)
        'dimension the ETABS Object as cOAPI type
        myETABSObject = Nothing
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
        'Dim SapModel As ETABSv1.cSapModel
        SapModel = myETABSObject.SapModel

        'ret = myETABSObject.Hide                       'hide application

        ret = SapModel.File.OpenFile(saplocation.Text)     'open an existing file

        Dim islocked As Boolean
        islocked = SapModel.GetModelIsLocked       'check if model is locked
        If islocked = True Then                    'Unlock model
            ret = SapModel.SetModelIsLocked(False)
            If (ret <> 0) Then : MsgBox("Problem occured on :Unlock model") : Stop : End If
        End If                                      'Unlock finish 

    End Sub
    Private Sub Sorting(ByRef dis As Integer, ByRef DEV() As Double, ByRef JS As Integer, ByRef SEV() As Double, ByRef SES() As Integer, ByVal ISC As Integer)
        Dim i, j, k As Integer
        Dim H1(JS), H3(JS)


        '      HERHANGiBiR BiR DiZiNi SIRALAMA PROGRAMI
        '****  dis=DiZiN SAYISI
        '****  DEV=DiZiN iÇiNDEKi ELEMAN DEGERLERiN VEKTÖRÜ
        '****  JS=SIRALANACAK ELEMAN SAYISI
        '****  SEV=SIARALANACAK ELEMAN DEGERLERiN VEKTÖRÜ
        '****  SES=SIRALANAN ELEMANLARIN BAsLANGIÇ SIRALARI
        '   ISC'ARTAN SIRALAMA=1 AZALAN SIRALAMA=0'


        If (ISC = 0) Then
            For i = 0 To JS
                SEV(i) = 0
                H1(i) = 0
            Next i
            For i = 0 To dis
                For j = 0 To JS
                    If (DEV(i) > SEV(j)) Then
                        For k = j + 1 To JS
                            H1(k) = SEV(k - 1)
                            H3(k) = SES(k - 1)
                        Next k
                        H1(j) = DEV(i)
                        H3(j) = i
                        For k = j To JS
                            SEV(k) = H1(k)
                            SES(k) = H3(k)
                        Next k
                        GoTo 1
                    End If
                Next j
1:              ' Continue
            Next i
        Else
            For i = 0 To JS
                SEV(i) = 10000000000.0
                H1(i) = 10000000000.0
            Next i
            For i = 0 To dis
                For j = 0 To JS
                    If (DEV(i) < SEV(j)) Then
                        For k = j + 1 To JS
                            H3(k) = SES(k - 1)
                            H1(k) = SEV(k - 1)
                        Next k
                        H1(j) = DEV(i)
                        H3(j) = i
                        For k = j To JS
                            SEV(k) = H1(k)
                            SES(k) = H3(k)
                        Next k
                        GoTo 2
                    End If
                Next j
2:              'Continue
            Next i
        End If
    End Sub
    Private Sub loadoutput_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadoutput.Click
        Dim saveFileDialog1 As New Windows.Forms.SaveFileDialog()
        saveFileDialog1.Filter = "Text Files|*.txt"
        saveFileDialog1.Title = "Output Dosyasını Giriniz"
        saveFileDialog1.ShowDialog()
        OutputLoc.Text = saveFileDialog1.FileName.ToString
    End Sub
    Private Sub loadpoolfile2_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadpoolfile2.Click
        Dim openFileDialog1 As New Windows.Forms.OpenFileDialog()
        openFileDialog1.Filter = "Text Files|*.txt"
        openFileDialog1.Title = "Select Section File"
        openFileDialog1.ShowDialog()
        poollocation2.Text = openFileDialog1.FileName.ToString
    End Sub
    Private Sub loadpoolfile1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadpoolfile1.Click
        Dim openFileDialog1 As New Windows.Forms.OpenFileDialog()
        openFileDialog1.Filter = "Text Files|*.txt"
        openFileDialog1.Title = "Select Section File"
        openFileDialog1.ShowDialog()
        poollocation1.Text = openFileDialog1.FileName.ToString
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'Dim ETABSCommands As General.ETABSCommands
        'Dim istop As Boolean
        'Call Control(istop)
        'If istop = True Then : Exit Sub : End If
        'ETABSCommands.InitilizeETABS()
        'ETABSCommands.OpenFile(saplocation.Text)
    End Sub

    Private Sub loadETABSfile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadETABSfile.Click
        Dim openFileDialog1 As New Windows.Forms.OpenFileDialog()
        openFileDialog1.Filter = "ETABS File|*.EDB"
        openFileDialog1.Title = "Select ETABS file"
        openFileDialog1.ShowDialog()
        saplocation.Text = openFileDialog1.FileName.ToString
    End Sub

    Private Sub start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles start.Click
        Dim istop As Integer

        Call Control(istop)
        If istop = 1 Then : Exit Sub : End If
        Dim SapModel As ETABSv1.cSapModel = Nothing
        Dim ETABSObject As ETABSv1.cOAPI = Nothing
        Call Initilize(SapModel, ETABSObject)
        '--------------------------------------------------------------------------

        '--------------------------------------------------------------------------
        Dim startDate As Date = Date.Now
        TextBox10.Text = Format(Now, "hh:mm:ss")        'Start Time @ textbox 10

        Call Read_Section(SapModel)
        Call evaluate(sapmodel)
        TextBox11.Text = Format(Now, "hh:mm:ss")        'Finish Time @ textbox 11
        Dim endDate As Date = Date.Now
        Dim timeSpan As TimeSpan = endDate.Subtract(startDate)
        Dim timedmin As Integer = timeSpan.Minutes * 60
        Dim timedsec As Integer = timeSpan.Seconds
        Dim avtime As Double
        'avtime = Math.Round(((timedmin + timedsec) / iter), 2)
        TextBox12.Text = timeSpan.Days & "D:" & timeSpan.Hours & "H:" & timeSpan.Minutes & "M:" & timeSpan.Seconds & "S," & "Ave=" & avtime & "sec"


        'Clean up variables
        SapModel = Nothing
        ETABSObject = Nothing
        'Check ret value 
        If ret = 0 Then
            MsgBox("API script completed successfully.")
        Else
            MsgBox("API script FAILED to complete.")
        End If
    End Sub

End Class
