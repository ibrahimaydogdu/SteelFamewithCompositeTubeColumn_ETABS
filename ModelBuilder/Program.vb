Imports System.Globalization
Imports System.IO

'Command line:
'  ModelBuilder.exe /template <file.csv>                       writes a CSV with all columns and the default values
'  ModelBuilder.exe /plan <examples.csv>                       geometry and groups of every example (no ETABS)
'  ModelBuilder.exe /build <examples.csv> /out <folder> [/hide] [/overwrite] [/only <name>]
'                                                              one ETABS model <name>.EDB and report per example
Public Module Program
    Private LogFile As String

    Private Sub Say(ByVal Text As String)
        Dim Line As String = Date.Now.ToString("HH:mm:ss") & " " & Text
        Console.WriteLine(Line)
        If LogFile IsNot Nothing Then
            Try
                File.AppendAllText(LogFile, Line & Environment.NewLine)
            Catch
            End Try
        End If
    End Sub

    Private Function Arg(ByVal A() As String, ByVal Name As String) As String
        Dim i As Integer = Array.FindIndex(A, Function(x) x.Equals(Name, StringComparison.OrdinalIgnoreCase))
        Return If(i >= 0 AndAlso i + 1 < A.Length, A(i + 1), Nothing)
    End Function

    Private Function Has(ByVal A() As String, ByVal Name As String) As Boolean
        Return A.Any(Function(x) x.Equals(Name, StringComparison.OrdinalIgnoreCase))
    End Function

    Public Function Main(ByVal A() As String) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8
        Try
            If Has(A, "/template") Then
                Dim F As String = Arg(A, "/template")
                If F Is Nothing Then Return Usage()
                BuildParams_.WriteTemplate(F)
                Say("template written: " & F)
                Return 0
            End If
            Dim Csv As String = If(Has(A, "/plan"), Arg(A, "/plan"), If(Has(A, "/build"), Arg(A, "/build"), Nothing))
            If Csv Is Nothing Then Return Usage()
            Dim Errors As List(Of String) = Nothing
            Dim List = BuildParams_.ReadCsv(Csv, Errors)
            If Errors.Count > 0 Then
                For Each E In Errors : Say("error: " & E) : Next
                Return 1
            End If
            Dim Only As String = Arg(A, "/only")
            If Only IsNot Nothing Then List = List.Where(Function(p) p.Name.Equals(Only, StringComparison.OrdinalIgnoreCase)).ToList()
            If List.Count = 0 Then Say("no example to process") : Return 1
            If Has(A, "/plan") Then
                For Each P In List
                    Say("== " & P.Name)
                    Console.Write(BuildPlan_.Create(P).Describe())
                Next
                Return 0
            End If
            Return BuildAll(List, A)
        Catch ex As Exception
            Say("error: " & ex.Message)
            Return 1
        End Try
    End Function

    Private Function Usage() As Integer
        Console.WriteLine("ModelBuilder.exe /template <file.csv>")
        Console.WriteLine("ModelBuilder.exe /plan <examples.csv> [/only <name>]")
        Console.WriteLine("ModelBuilder.exe /build <examples.csv> /out <folder> [/hide] [/overwrite] [/only <name>]")
        Return 2
    End Function

    Private Function BuildAll(ByVal List As List(Of BuildParams_), ByVal A() As String) As Integer
        Dim OutDir As String = Arg(A, "/out")
        If OutDir Is Nothing Then Return Usage()
        Directory.CreateDirectory(OutDir)
        OutDir = Path.GetFullPath(OutDir)
        If OutDir.Length > 150 Then Say("error: the output folder path is too long (" & OutDir.Length & " characters, at most 150)") : Return 1
        LogFile = Path.Combine(OutDir, "builder.log")
        Dim Overwrite As Boolean = Has(A, "/overwrite")
        Dim Todo = List.Where(Function(p)
                                  Dim Present As Boolean = File.Exists(Path.Combine(OutDir, p.Name & ".EDB"))
                                  If Present AndAlso Not Overwrite Then Say("skipped (the model exists, use /overwrite): " & p.Name)
                                  Return Overwrite OrElse Not Present
                              End Function).ToList()
        If Todo.Count = 0 Then Return 0
        Dim Builder As New EtabsBuilder With {.Log = AddressOf Say, .Hide = Has(A, "/hide")}
        Dim LibFile As String = EtabsBuilder.FindLibrary(EtabsBuilder.FindETABS())
        If LibFile Is Nothing Then Say("error: section library not found (SectionPropertyDataPath)") : Return 1
        Dim Failed As Integer
        Try
            Builder.Start()
            For Each P In Todo
                Say("== " & P.Name)
                Try
                    Dim Plan = BuildPlan_.Create(P)
                    Dim Edb As String = Builder.Build(Plan, LibFile, OutDir)
                    Say("done: " & P.Name)
                Catch ex As Exception
                    Failed += 1
                    Say("FAILED: " & P.Name & ": " & ex.Message)
                End Try
            Next
        Catch ex As Exception
            Say("error: " & ex.Message)
            Failed += 1
        Finally
            Builder.Shutdown()
        End Try
        Say(Todo.Count - Failed & " of " & Todo.Count & " models built")
        Return If(Failed = 0, 0, 1)
    End Function
End Module
