Option Explicit On

Public Class frmRef
    Dim Directorio As Scripting.FileSystemObject
    Dim Texto As System.IO.StreamReader
    Dim Texto1 As System.IO.StreamWriter
    Dim Txt As String
    Dim TxtVal As String
    Dim TextoBool As Boolean

    Dim Modelo As Tekla.Structures.Model.Model
    Dim ModelObjEnumBm As Tekla.Structures.Model.ModelObjectEnumerator
    Dim ModelObj As Tekla.Structures.Model.ModelObject
    Dim Dwg As Tekla.Structures.Drawing.DrawingHandler
    Dim DwgEn As Tekla.Structures.Drawing.DrawingEnumerator
    Dim DwgObj As Tekla.Structures.Drawing.Drawing
    Dim UI As Tekla.Structures.Model.UI.ModelObjectSelector

    Dim AssPos As String
    Dim Principal As Integer
    Dim CantMod As Long
    Dim Progreso As Long
    Dim Salida As Boolean
    Dim Material As String

    Dim CarpModelo As String
    Dim Plano As String
    Dim Marcas As String
    Dim Att As String
    Dim AttCont As String
    Dim DwgContar As String
    Dim Contar As Integer
    Dim Total As Integer
    Dim MisDoc As String

    Dim DXF As String
    Dim InfoDwg() As String
    Dim Info() As String
    Dim nn As Integer
    Dim Mt(0) As String
    Dim Mt1(0) As String
    Dim Fila As Long

    Private Sub cmdRef_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRef.Click
        On Error Resume Next

        Call CopiarMacros()

        UI = New Tekla.Structures.Model.UI.ModelObjectSelector
        ModelObjEnumBm = UI.GetSelectedObjects
        CantMod = ModelObjEnumBm.GetSize
        CarpModelo = Modelo.GetInfo.ModelPath

        If CantMod = 0 And cmbRef.Text <> "Herrick" Then
            MsgBox("Debe seleccionar los elementos en el modelo", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error")
            Exit Sub
        End If

        Directorio = New Scripting.FileSystemObject

        Dwg = New Tekla.Structures.Drawing.DrawingHandler
        DwgEn = Dwg.GetDrawingSelector.GetSelected

        ProgressBar1.Value = 0

        If DwgEn.GetSize = 0 Then
            MsgBox("No se han seleccionado los planos de montaje", MsgBoxStyle.Critical, "Error de Selección")
            Exit Sub
        Else
            Do While DwgEn.MoveNext
                DwgObj = DwgEn.Current
                If InStr(DwgObj.ToString, "GADrawing") = 0 Then
                    MsgBox("Existen planos seleccionados que no son de montaje", MsgBoxStyle.Critical, "Error de Selección")
                    Exit Sub
                End If
            Loop
        End If

        If cmbAtributos.Text = "" And cmbRef.Text <> "Herrick" Then
            MsgBox("Debe Seleccionar un Atributo", MsgBoxStyle.Critical, "Error de Atributo")
            Exit Sub
        End If

        If cmbRef.Text = "Herrick" Then
            If MsgBox("Se borraran los datos almacenados en el atributo '" & "Title 2" & "'" & Chr(13) & "¿Desea Continuar?", MsgBoxStyle.YesNo, _
                      "Confirmación") = MsgBoxResult.No Then
                Exit Sub
            End If
        Else
            If MsgBox("Se borraran los datos almacenados en el atributo '" & cmbAtributos.Text & "'" & Chr(13) & "¿Desea Continuar?", MsgBoxStyle.YesNo, _
                      "Confirmación") = MsgBoxResult.No Then
                Exit Sub
            End If
        End If

        If Dir$(CarpModelo & "\PlotFiles", FileAttribute.Directory) <> "" Then
            Directorio.DeleteFolder(CarpModelo & "\PlotFiles")
        End If

        lblStatus.Text = "Generando archivos DXF..."
        Tekla.Structures.Model.Operations.Operation.RunMacro("PLT_DXF.cs")
        Do While Tekla.Structures.Model.Operations.Operation.IsMacroRunning
            My.Application.DoEvents()
            Total = 0
            Contar = 0
            Do Until Total = DwgEn.GetSize
                DwgContar = Dir$(CarpModelo & "\PlotFiles\*.dxf")
                Contar = 0
                Do While DwgContar <> ""
                    Contar = Contar + 1
                    DwgContar = Dir$()
                Loop

                Total = Contar
                ProgressBar1.Value = Total / DwgEn.GetSize * 100
                Application.DoEvents()
            Loop
        Loop

        Call VerificarDXF()

        Att = cmbAtributos.Text

        If TextoBool = True Then
            If MsgBox("Los siguientes planos no tienen actualizadas las marcas, ¿Desea continuar?:" & Chr(13) & Chr(13) & _
                      TxtVal, MsgBoxStyle.YesNo, "Plano sin actualizar") = MsgBoxResult.No Then
                Exit Sub
            End If
        End If

        lblStatus.Text = "Generando Referencias..."
        Application.DoEvents()
        Salida = False
        Progreso = 0

        'Almacena info DXF en variables
        DXF = Dir$(CarpModelo & "\PlotFiles\*.dxf", FileAttribute.Archive)
        nn = 0
        Do While DXF <> ""
            Dim Plano As String

            If InStr(DXF, ".dxf") <> 0 Then
                nn = nn + 1
                ReDim Preserve Info(nn)
                ReDim Preserve InfoDwg(nn)

                Texto = System.IO.File.OpenText(CarpModelo & "\PlotFiles\" & DXF)
                Txt = Texto.ReadToEnd
                Texto.Close()
                InfoDwg(nn - 1) = Txt

                If InStr(DXF, " ") <> 0 Then
                    Plano = Mid(DXF, 1, InStr(DXF, " ") - 1)
                ElseIf InStr(DXF, "_") <> 0 Then
                    Plano = Mid(DXF, 1, InStr(DXF, "_") - 1)
                ElseIf InStr(DXF, "-") <> 0 Then
                    Plano = Mid(DXF, 1, InStr(DXF, "-") - 1)
                Else
                    Plano = Mid(DXF, 1, InStr(DXF, ".dxf") - 1)
                End If

                Info(nn - 1) = Plano
                DXF = Dir$()
            End If
        Loop
        Txt = ""

        'Para el caso de que sea Herrick, conduce al nuevo procedimiento que sólo utiliza información desde planos
        If cmbRef.Text = "Herrick" Then
                Call RecorrerHerrick()
        Else
            'Verifica cantidad de elementos que sean partes y recorre los planos de montaje buscando la marca de assembly en ellos
            Progreso = 0
            ProgressBar1.Value = 0
            Do While ModelObjEnumBm.MoveNext
                ModelObj = ModelObjEnumBm.Current

                If cmbRef.Text = "Estándar" Then
                    If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Or _
                    InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Then
                        Call RecorrerMontajes()
                    End If
                ElseIf cmbRef.Text = "Hanford" Then
                    If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Or _
                    InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Then
                        Call RecorrerMontajesHanford()
                    End If
                ElseIf cmbRef.Text = "Fluor Techint Referencias" Then
                    If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Or _
                    InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Then
                        Call RecorrerMontajesFluorRef()
                    End If
                ElseIf cmbRef.Text = "Fluor Techint" Then
                    If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Or _
                    InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Then
                        Call RecorrerFluorTechint()
                    End If
                End If

                If Salida = True Then
                    Exit Sub
                End If
                Progreso = Progreso + 1
                ProgressBar1.Value = Progreso / CantMod * 100
                Application.DoEvents()
            Loop
            ModelObjEnumBm = Nothing

            'Modelo.CommitChanges()

            'Ingreso a Hanford, asignación de marcas nuevamente
            If cmbRef.Text = "Hanford" Then
                ProgressBar1.Value = 0
                UI = New Tekla.Structures.Model.UI.ModelObjectSelector
                ModelObjEnumBm = UI.GetSelectedObjects
                CantMod = ModelObjEnumBm.GetSize
                Progreso = 0

                Do While ModelObjEnumBm.MoveNext
                    ModelObj = ModelObjEnumBm.Current

                    If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Or _
                        InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Then
                        Call HanfordRefMulti()
                    End If

                    Progreso = Progreso + 1
                    ProgressBar1.Value = Progreso / CantMod * 100
                    My.Application.DoEvents()
                Loop
            End If

            lblStatus.Text = "Verificando marcas en el modelo..."
            Call VerificarMarcas()

            'Si el modelo es de FluorTechint, se ponen las revisiones en PRELIM_MARK del modelo
            If cmbRef.Text = "Fluor Techint" Then
                PonerRevision()
                ImportarAtt()
            End If

            lblStatus.Text = ""
            ProgressBar1.Value = 100

            MsgBox("Completado. Debe grabar el modelo", MsgBoxStyle.Information, "Referencias Incorporadas")
        End If
    End Sub

    Private Sub frmRef_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim WScript As Object

        Modelo = New Tekla.Structures.Model.Model
        Directorio = New Scripting.FileSystemObject
        WScript = CreateObject("WScript.Shell")

        'Obtenemos la ruta del destino de Mis Documentos 
        MisDoc = WScript.SpecialFolders("MyDocuments")

        WScript = Nothing

        ProgressBar1.Value = 0

        If Modelo.GetConnectionStatus = False Then
            MsgBox("No se ha podido conectar con el modelo")
            Salida = True
            Exit Sub
        End If

        lblActualModel.Text = Modelo.GetInfo.ModelName

        Call CargarCombo()
    End Sub

    Private Sub RecorrerMontajesHanford()
        If ModelObj.GetReportProperty("MAIN_PART", Principal) Then
        End If

        If Principal = 1 Then
            If ModelObj.GetReportProperty("ASSEMBLY_POS", AssPos) Then
            End If

            If ModelObj.GetReportProperty("MATERIAL", Material) Then
            End If

            If InStr(AssPos, "REF") <> 0 Or InStr(Material, "REF") <> 0 Then
                Exit Sub
            End If

            AttCont = ""

            If ModelObj.SetUserProperty(Att, "") Then
            End If

            If ModelObj.SetUserProperty(Att, AttCont) Then
            End If

            Fila = Fila + 1
            ReDim Preserve Mt(Fila)
            ReDim Preserve Mt1(Fila)

            For i = 0 To nn - 1
                If InStr(InfoDwg(i), "  1" & Chr(13) & Chr(10) & AssPos & Chr(13)) <> 0 Then
                    If AttCont = "" Or AttCont = " " Then
                        If ModelObj.SetUserProperty(Att, Info(i)) Then
                        End If
                    ElseIf InStr(AttCont, Info(i)) = 0 Then
                        If ModelObj.SetUserProperty(Att, AttCont & " " & Info(i)) Then
                        End If
                    End If
                End If
                If ModelObj.GetUserProperty(Att, AttCont) Then
                End If

                If AttCont <> "" Then
                    For h = 0 To Fila - 1
                        If Mid(AssPos, 1, 5) = Mt(h) Then
                            If InStr(Mt1(h), AttCont) = 0 Then
                                If Mt1(h) = "" Then
                                    Mt1(h) = AttCont
                                Else
                                    Mt1(h) = Mt1(h) & " " & AttCont
                                End If
                            End If

                            Fila = Fila - 1
                            Exit Sub
                        End If
                    Next h

                    Mt(Fila) = Mid(AssPos, 1, 5)
                    Mt1(Fila) = AttCont
                    Exit Sub
                End If
            Next i
        End If
    End Sub

    Private Sub HanfordRefMulti()
        ModelObj.GetReportProperty("MAIN_PART", Principal)

        If Principal = 1 Then
            If ModelObj.GetReportProperty("ASSEMBLY_POS", AssPos) Then
            End If

            ModelObj.GetReportProperty("MATERIAL", Material)

            If InStr(AssPos, "REF") <> 0 Or InStr(Material, "REF") <> 0 Then
                Exit Sub
            End If

            AttCont = ""

            If ModelObj.SetUserProperty(Att, "") Then
            End If

            If ModelObj.SetUserProperty(Att, AttCont) Then
            End If

            AssPos = Mid(AssPos, 1, 5)

            For i = 0 To Fila
                If Mt(i) = AssPos Then
                    If AttCont = "" Or AttCont = " " Then
                        If ModelObj.SetUserProperty(Att, Mt1(i)) Then
                        End If
                    ElseIf InStr(AttCont, Mt1(i)) = 0 Then
                        If ModelObj.SetUserProperty(Att, AttCont & " " & Mt1(i)) Then
                        End If
                    End If
                End If
                If ModelObj.GetUserProperty(Att, AttCont) Then
                End If
            Next i
        End If

    End Sub

    Private Sub RecorrerMontajesFluorRef()
        Dim UF4 As String

        UF4 = ""

        ModelObj.GetReportProperty("MAIN_PART", Principal)

        If Principal = 1 Then
            ModelObj.GetReportProperty("ASSEMBLY_PREFIX", AssPos)
            ModelObj.GetReportProperty("USER_FIELD_4", UF4)

            AssPos = AssPos & UF4

            If ModelObj.GetReportProperty("MATERIAL", Material) Then
            End If

            If InStr(AssPos, "REF") <> 0 Or InStr(Material, "REF") <> 0 Then
                Exit Sub
            End If

            AttCont = ""

            If ModelObj.SetUserProperty(Att, "") Then
            End If

            If ModelObj.GetUserProperty(Att, AttCont) Then
            End If

            For i = 0 To nn - 1
                If InStr(InfoDwg(i), "  1" & Chr(13) & Chr(10) & AssPos & Chr(13)) <> 0 Then
                    If AttCont = "" Or AttCont = " " Then
                        If ModelObj.SetUserProperty(Att, Info(i)) Then
                        End If
                    ElseIf InStr(AttCont, Info(i)) = 0 Then
                        If ModelObj.SetUserProperty(Att, AttCont & " " & Info(i)) Then
                        End If
                    End If
                End If
                If ModelObj.GetUserProperty(Att, AttCont) Then
                End If
            Next i
        End If
    End Sub

    Private Sub RecorrerMontajes()
        If ModelObj.GetReportProperty("MAIN_PART", Principal) Then
        End If

        If Principal = 1 Then
            ModelObj.GetReportProperty("ASSEMBLY_POS", AssPos)


            ModelObj.GetReportProperty("MATERIAL", Material)

            If InStr(AssPos, "REF") <> 0 Or InStr(Material, "REF") <> 0 Then
                Exit Sub
            End If

            AttCont = ""

            ModelObj.SetUserProperty(Att, "")

            If ModelObj.GetUserProperty(Att, AttCont) Then
            End If

            For i = 0 To nn - 1
                If InStr(InfoDwg(i), "  1" & Chr(13) & Chr(10) & AssPos & Chr(13)) <> 0 Then
                    If AttCont = "" Or AttCont = " " Then
                        If ModelObj.SetUserProperty(Att, Info(i)) Then
                        End If
                    ElseIf InStr(AttCont, Info(i)) = 0 Then
                        If ModelObj.SetUserProperty(Att, AttCont & " " & Info(i)) Then
                        End If
                    End If
                End If
                If ModelObj.GetUserProperty(Att, AttCont) Then
                End If
            Next i
        End If
    End Sub

    Private Sub RecorrerHerrick()
        Dim AssDwg As Tekla.Structures.Drawing.Drawing

        MsgBox("Seleccionar sólo los planos tipo Assembly a referenciar", MsgBoxStyle.ApplicationModal + MsgBoxStyle.OkOnly)

        DwgEn = Nothing
        DwgEn = Dwg.GetDrawingSelector.GetSelected

        AttCont = ""

        For Each AssDwg In DwgEn
            If InStr(AssDwg.ToString, "Assembly") <> 0 Then
                Dim Marca As String
                Marca = AssDwg.Mark

                If InStr(Marca, " ") Then
                    Marca = Mid(Marca, 1, InStr(Marca, " ") - 1)
                End If

                Marca = Replace(Replace(Replace(Marca, ".", ""), "[", ""), "]", "")

                For i = 0 To nn - 1
                    If InStr(InfoDwg(i), "  1" & Chr(13) & Chr(10) & Marca & Chr(13)) <> 0 Then
                        If AttCont = "" Or AttCont = " " Then
                            AttCont = Info(i)
                        ElseIf InStr(AttCont, Info(i)) = 0 Then
                            AttCont = AttCont & "/" & Info(i)
                        End If
                    End If
                Next i

                AssDwg.Title2 = AttCont
                AssDwg.CommitChanges()
            End If
        Next
    End Sub

    Private Sub RecorrerFluorTechint()
        Dim NumObj As String

        NumObj = ""

        If ModelObj.GetReportProperty("MATERIAL", Material) Then
        End If

        If InStr(AssPos, "REF") <> 0 Or InStr(Material, "REF") <> 0 Then
            Exit Sub
        End If

        If ModelObj.GetReportProperty("CAST_UNIT_POS", AssPos) Then
        End If
        If ModelObj.GetReportProperty("USER_FIELD_4", NumObj) Then
        End If

        AssPos = Mid(AssPos, 1, 18) & NumObj

        AttCont = ""

        If ModelObj.SetUserProperty(Att, "") Then
        End If

        If ModelObj.GetUserProperty(Att, AttCont) Then
        End If

        For i = 0 To nn - 1
            If InStr(InfoDwg(i), "  1" & Chr(13) & Chr(10) & AssPos & Chr(13)) <> 0 Then
                If AttCont = "" Or AttCont = " " Then
                    If ModelObj.SetUserProperty(Att, Info(i)) Then
                    End If
                ElseIf InStr(AttCont, Info(i)) = 0 Then
                    If ModelObj.SetUserProperty(Att, AttCont & Att & Info(i)) Then
                    End If
                End If
            End If
        Next i
    End Sub

    Private Sub CopiarMacros()
        If Directorio.FolderExists("C:\MACROS") Then
            Directorio.DeleteFolder("C:\MACROS")
        End If
        Directorio.CopyFolder("T:\Tekla_Common\GESTION_TEKLA\MACROS", "C:\MACROS")
    End Sub

    Private Sub CargarCombo()
        cmbAtributos.Items.Add("REF_MONTAJE")
        cmbAtributos.Items.Add("comment")
        cmbAtributos.Items.Add("USER_FIELD_1")
        cmbAtributos.Items.Add("USER_FIELD_2")
        cmbAtributos.Items.Add("USER_FIELD_3")
        cmbAtributos.Items.Add("USER_FIELD_4")
        cmbAtributos.Items.Add("APPROVAL_OTHER")

        cmbRef.Items.Add("Estándar")
        cmbRef.Items.Add("Fluor Techint")
        cmbRef.Items.Add("Fluor Techint Referencias")
        cmbRef.Items.Add("Hanford")
        cmbRef.Items.Add("Herrick")
    End Sub

    Private Sub VerificarDXF()
        Dim DXFVal As String

        DXFVal = Dir$(CarpModelo & "\PlotFiles\", FileAttribute.Archive)
        Do While DXFVal <> ""
            Txt = ""
            If InStr(DXFVal, ".dxf") <> 0 Then
                Texto = System.IO.File.OpenText(CarpModelo & "\PlotFiles\" & DXFVal)
                Txt = Texto.ReadToEnd
                Texto.Close()
                If InStr(Txt, "(?)") <> 0 Then
                    TextoBool = True
                    If TxtVal = "" Then
                        TxtVal = Mid(DXFVal, 1, InStr(DXFVal, ".dxf") - 1)
                    Else
                        TxtVal = TxtVal & " " & Mid(DXFVal, 1, InStr(DXFVal, ".dxf") - 1)
                    End If
                End If
            End If
            DXFVal = Dir$()
        Loop
    End Sub

    Private Sub VerificarMarcas()
        Modelo = New Tekla.Structures.Model.Model
        UI = New Tekla.Structures.Model.UI.ModelObjectSelector
        ModelObjEnumBm = UI.GetSelectedObjects
        CantMod = ModelObjEnumBm.GetSize

        If Directorio.FileExists(MisDoc & "\MarkLog.txt") Then
            Directorio.DeleteFile(MisDoc & "\MarkLog.txt")
        End If

        Texto1 = My.Computer.FileSystem.OpenTextFileWriter(MisDoc & "\MarkLog.txt", False, System.Text.Encoding.Default)

        Progreso = 0
        'Verifica si las marcas contienen referencias a planos

        Do While ModelObjEnumBm.MoveNext
            Application.DoEvents()
            ModelObj = ModelObjEnumBm.Current
            If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Or _
                InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Then
                Call MarcasVacias()
            End If
            Progreso = Progreso + 1
            ProgresoBarra(Progreso, CantMod)
        Loop
        ModelObjEnumBm = Nothing

        Texto1.Close()

        If Marcas <> "" Then
            MsgBox("Las siguientes marcas no fueron encontradas en los montajes:" & Chr(13) & Chr(13) & Marcas _
            & Chr(13) & Chr(13) & "Revisar el archivo 'MarkLog.txt' en la carpeta documentos.", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Info")
        End If
    End Sub

    Private Sub MarcasVacias()
        If ModelObj.GetReportProperty("MAIN_PART", Principal) Then
        End If

        If Principal = 1 Then
            AttCont = ""
            ModelObj.GetReportProperty("ASSEMBLY_POS", AssPos)


            ModelObj.GetReportProperty("MATERIAL", Material)


            If InStr(AssPos, "REF") <> 0 Or InStr(Material, "REF") <> 0 Then
                Exit Sub
            End If

            ModelObj.GetUserProperty(Att, AttCont)

            If AttCont = "" Or AttCont = " " Then
                If Marcas = "" Then
                    Marcas = AssPos
                ElseIf InStr(Marcas, AssPos) = 0 Then
                    Marcas = Marcas & " " & AssPos
                End If
                Texto1.WriteLine(AssPos)
            End If
        End If
    End Sub

    Private Sub cmbRef_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbRef.TextChanged
        If cmbRef.SelectedIndex = 1 Then
            cmbAtributos.Text = "comment"
            cmbAtributos.Enabled = False
        Else
            cmbAtributos.Enabled = True
            cmbAtributos.Text = ""
        End If
    End Sub

    Private Sub PonerRevision()
        Modelo = New Tekla.Structures.Model.Model
        UI = New Tekla.Structures.Model.UI.ModelObjectSelector
        ModelObjEnumBm = UI.GetSelectedObjects
        CantMod = ModelObjEnumBm.GetSize
        Dim Revisiones As String
        Dim Archivo As System.IO.StreamReader

        Tekla.Structures.Model.Operations.Operation.RunMacro("REV_PLANOS.cs")
        Do While Tekla.Structures.Model.Operations.Operation.IsMacroRunning
        Loop

        Archivo = New System.IO.StreamReader(CarpModelo & "\Informes\MONT_REV.xsr")
        Revisiones = Archivo.ReadToEnd
        Archivo.Close()

        ProgressBar1.Value = 0
        Progreso = 0
        lblStatus.Text = "Buscando última revisión de plano..."

        Do While ModelObjEnumBm.MoveNext
            My.Application.DoEvents()
            ModelObj = ModelObjEnumBm.Current

            If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 _
            Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 Then
                BuscarRev(Revisiones)
            End If

            Progreso = Progreso + 1
            ProgresoBarra(Progreso, CantMod)
        Loop
    End Sub

    Private Sub BuscarRev(ByRef Lista As String)
        Dim RefPlano As String
        Dim Rev As String

        RefPlano = ""
        ModelObj.GetUserProperty("comment", RefPlano)

        If RefPlano = "" Then
            Exit Sub
        End If

        If InStr(Lista, RefPlano) <> 0 Then
            Rev = Trim((Mid(Lista, InStr(Lista, RefPlano) + 31, 2)))
            ModelObj.SetUserProperty("PRELIM_MARK", Rev)
        End If
    End Sub

    Private Sub ProgresoBarra(ByRef Aumento As Long, ByRef Elementos As Long)
        ProgressBar1.Value = Aumento * 100 / Elementos
        My.Application.DoEvents()
    End Sub

    Private Sub ImportarAtt()
        Dim Archivo As System.IO.StreamReader
        Dim Atributo As String

        UI = New Tekla.Structures.Model.UI.ModelObjectSelector
        ModelObjEnumBm = UI.GetSelectedObjects
        CantMod = ModelObjEnumBm.GetSize

        Tekla.Structures.Model.Operations.Operation.RunMacro("GEN_IMPORT.cs")
        Do While Tekla.Structures.Model.Operations.Operation.IsMacroRunning
        Loop

        Archivo = New System.IO.StreamReader(CarpModelo & "\Informes\IMPORT.xsr")
        Atributo = Archivo.ReadToEnd
        Archivo.Close()

        lblStatus.Text = "Importando atributos al modelo..."
        ProgressBar1.Value = 0
        Progreso = 0

        Do While ModelObjEnumBm.MoveNext
            My.Application.DoEvents()
            ModelObj = ModelObjEnumBm.Current

            If InStr(UCase(ModelObj.ToString), "BEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "CONTOURPLATE") <> 0 _
            Or InStr(UCase(ModelObj.ToString), "POLYBEAM") <> 0 Or InStr(UCase(ModelObj.ToString), "REBARGROUP") <> 0 _
            Or InStr(UCase(ModelObj.ToString), "SINGLEREBAR") <> 0 Then
                AgregarAtt(Atributo)
            End If

            Progreso = Progreso + 1
            ProgresoBarra(Progreso, CantMod)
        Loop
    End Sub

    Private Sub AgregarAtt(ByRef Texto As String)
        Dim CastU As String
        Dim comment As String
        Dim Prelim_Mark As String

        CastU = ""
        comment = ""
        Prelim_Mark = ""

        ModelObj.GetReportProperty("CAST_UNIT_POS", CastU)

        If InStr(Texto, CastU) <> 0 Then
            comment = Trim(Mid(Texto, InStr(Texto, CastU) + 32, 30))
            Prelim_mark = Trim(Mid(Texto, InStr(Texto, CastU) + 64, 1))

            ModelObj.SetUserProperty("comment", comment)
            ModelObj.SetUserProperty("PRELIM_MARK", Prelim_Mark)
        End If
    End Sub

    Private Sub cmbRef_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbRef.SelectedIndexChanged
        If cmbRef.Text = "Herrick" Then
            cmbAtributos.Enabled = False
            cmbAtributos.SelectedIndex = -1
        End If
    End Sub
End Class
