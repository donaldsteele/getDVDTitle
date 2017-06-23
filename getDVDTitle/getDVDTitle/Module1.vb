Imports System.Xml.XPath
Imports System.IO
Imports System.Xml
''' <summary>
''' getDVdTitle - Attempt to get the movie title of a dvd in your dvd drive. only works for single title dvd's 
''' Created by Don Steele https://github.com/donaldsteele
''' released under the MIT License 
''' </summary>
Module Module1

    Function Main(args As String()) As Integer
        Try
            Dim dvdDrive As String

            If args.Count <> 1 Then
                DisplayHelp()
                Return -1
            End If

            If args(0).Length <> 2 Then
                DisplayHelp()
                Return -2
            End If
            dvdDrive = validateDVDdrive(args(0))

            If dvdDrive = vbNullString Then
                DisplayHelp()
                Return -3
            End If
            Console.Write(parseMetadata(GetMetaData(getDVDid(dvdDrive))))
            Return 0
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
        Return -4
    End Function

    Sub DisplayHelp()
        Console.Clear()
        Console.WriteLine("getDVDTitle - Attempt to get the movie title of a dvd in your dvd drive")
        Console.WriteLine("Created by Don Steele https://github.com/donaldsteele")
        Console.WriteLine()
        Console.WriteLine("Usage:")
        Console.WriteLine(" if D: is your cdrom drive then the command would be")
        Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName & " D:")

    End Sub

    Function validateDVDdrive(argDrive As String) As String
        If argDrive.Length = 2 Then
            For Each drive In System.IO.DriveInfo.GetDrives()
                If drive.DriveType = System.IO.DriveType.CDRom Then
                    If argDrive.ToUpper = drive.Name.Substring(0, 2).ToUpper Then
                        Return argDrive.ToUpper
                    End If
                End If
            Next
        End If
        Return vbNullString
    End Function


    Function getDVDid(DriveLetter As String)
        Dim filename As String = Path.Combine(IO.Path.GetTempPath, IO.Path.GetRandomFileName & ".exe")
        File.WriteAllBytes(filename, My.Resources.dvdid)
        File.SetAttributes(filename, FileAttributes.Normal)
        Dim oProcess As New Process()
        Dim oStartInfo As New ProcessStartInfo(filename, DriveLetter)
        oStartInfo.UseShellExecute = False
        oStartInfo.RedirectStandardOutput = True
        oStartInfo.WindowStyle = ProcessWindowStyle.Hidden
        oStartInfo.CreateNoWindow = True
        oProcess.StartInfo = oStartInfo
        oProcess.Start()
        Dim sOutput As String
        Using oStreamReader As System.IO.StreamReader = oProcess.StandardOutput
            sOutput = oStreamReader.ReadToEnd()
        End Using
        File.SetAttributes(filename, FileAttributes.Normal)
        System.IO.File.Delete(filename)
        Return sOutput.Replace("|", "")
    End Function

    Function GetMetaData(DVDid As String) As String
        Dim webClient As New Net.WebClient
        Dim WSURL As String = "http://metaservices.windowsmedia.com/pas_dvd_B/template/GetMDRDVDByCRC.xml?CRC="
        Dim result As String = webClient.DownloadString(WSURL & DVDid)
        Return result
    End Function


    Function parseMetadata(webserviceResults As String) As String

        Dim results As New List(Of String)
        Dim xpathDoc As New XmlDocument
        Dim xmlNav As XPathNavigator
        Dim xmlNI As XPathNodeIterator
        xpathDoc.LoadXml(webserviceResults)

        xmlNav = xpathDoc.CreateNavigator()
        xmlNI = xmlNav.Select("//dvdTitle")
        While (xmlNI.MoveNext())
            results.Add(xmlNI.Current.Value)
        End While
        Return String.Join("|", results)
    End Function
End Module
