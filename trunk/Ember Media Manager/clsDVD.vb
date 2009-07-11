'Class developed by blackducksoftware and modified for Ember Media Manager
Option Strict Off
Option Explicit On 

Imports System.IO

Public Class clsDVD
    Private Const ifo_SECTOR_SIZE As Short = 2048
    Private Const ifo_CellInfoSize As Short = 24

    Private ParsedIFOFile As struct_IFO_VST_Parse
    Private intNbOfIFOFile As Byte

    Private oEnc As System.Text.Encoding = System.Text.ASCIIEncoding.GetEncoding(1252)

    'Variables
    Dim mLanguages As New Hashtable
    Dim mAudioModes As New Hashtable
    Dim mVideoAspect As New Hashtable
    Dim mVideoCodingMode As New Hashtable
    Dim mVideoResolution(1) As Object

    'All these types are used for the IFO Parsing
    Private Structure VTS_PTT_SRPT
        Dim PackedString As String
        Dim NumberOfTitles As Integer
        Dim EndAddress_VST_PTT As Integer
        Dim OffsetTo_PPT As Integer
    End Structure

    'Program time information
    Private Structure DVD_Time_Type
        Dim hours As Byte
        Dim minutes As Byte
        Dim seconds As Byte
        Dim frame As Byte
    End Structure

    'Individual Cell information
    Private Structure PGC_Cell_Info_Type
        Dim CellPlayBackTime As DVD_Time_Type
    End Structure

    Private Structure struct_SRPT
        Dim Coding_Mode As Byte
        Dim Resolution As Byte
        Dim Bitrate As Boolean
    End Structure

    Private Structure struct_VideoAttributes_VTS_VOBS
        Dim Video_Standard As Byte
        Dim Coding_Mode As Byte
        Dim LetterBoxed As Boolean
        Dim Resolution As Byte
    End Structure

    'Audio Type
    Private Structure struct_AudioAttributes_VTSM_VTS
        Dim CodingMode As Byte
        Dim NumberOfChannels As Byte
        Dim LanguageCode As String
    End Structure

    'SubPicture Type
    Private Structure SubPictureAtt_VTSM_VTS_Type
        Dim CodingMode As Byte
        Dim LanguageCode As String
        Dim CodeExtention As Byte
    End Structure

    'IFO VST Program Chain Information
    Private Structure struct_Program_Chain_Type
        Dim NumberOfPrograms As Byte
        Dim NumberOfCells As Byte
        Dim PlayBackTime As DVD_Time_Type
        'Up to 64 Chapters
        Dim PChainInformation() As PGC_Cell_Info_Type
        'Currently only implamenting basic useful information
    End Structure

    'IFO VST information
    Private Structure struct_IFO_VST_Parse
        Dim IFO_FileName As String
        Dim FileType_Header As String
        Dim LastSectorOfTitle As Long
        Dim LastSectorOfIFO As Long
        Dim VSTCategory As Long
        Dim VersionNumber As Long
        Dim EndByteAddress_VTS_MAT As Long
        Dim StartSector_MenuVOB As Long
        Dim StartSector_TitleVOB As Long
        Dim SectorPointer_VTS_PTT_SRPT As Long
        Dim SectorPointer_VTS_PGCI As Long
        Dim SectorPointer_VTSM_PGCI_UT As Long
        Dim SectorPointer_VTS_TMAPTI As Long
        Dim SectorPointer_VSTM_C_ADT As Long
        Dim SectorPointer_VSTM_VOBU_ADMAP As Long
        Dim SectorPointer_VST_C_ADT As Long
        Dim SectorPointer_VTS_VOBU_ADMAP As Long
        Dim NumberOfProgramChains As Long
        Dim ProgramChainInformation() As struct_Program_Chain_Type
        Dim VideoAtt_VSTM_VOBS As struct_VideoAttributes_VTS_VOBS
        Dim NumOfAudioStreamsIn_VTSM_VOBS As Integer
        Dim AudioAtt_VTSM_VOBS() As struct_AudioAttributes_VTSM_VTS
        Dim NumSubPictureStreams_VTSM_VOBS As Integer
        Dim SubPictureAtt_VTSM_VOBS As SubPictureAtt_VTSM_VTS_Type
        Dim VideoAtt_VTS_VOBS As struct_VideoAttributes_VTS_VOBS
        Dim NumAudioStreams_VTS_VOBS As Integer
        Dim AudioAtt_VTS_VOBS() As struct_AudioAttributes_VTSM_VTS
        Dim NumSubPictureStreams_VTS_VOBS As Integer
        Dim SubPictureAtt_VTS_VOBS() As SubPictureAtt_VTSM_VTS_Type
    End Structure

    Private Function fctIsIFOFileExist(ByVal strDrive As String) As Boolean
        Dim objDI As New DirectoryInfo(strDrive & "VIDEO_TS\")
        Dim objIFOFile As FileInfo
        If objDI.Exists() Then
            If objDI.GetFiles("VTS*.IFO").Length > 0 Then
                fctIsIFOFileExist = True
            Else
                fctIsIFOFileExist = False
            End If
        Else
            fctIsIFOFileExist = False
        End If

        objIFOFile = Nothing
        objDI = Nothing
    End Function

    Public Function fctOpenIFOFile(ByVal strPath As String) As Boolean
        Dim IFOFiles As New ArrayList
        Dim tIFOFile As New struct_IFO_VST_Parse
        Dim currLongest As Integer = 0
        Dim currDuration As Integer = 0

        IFOFiles.AddRange(Directory.GetFiles(Directory.GetParent(strPath).FullName, "VTS*.IFO"))

        If IFOFiles.Count > 1 Then
            'find the one with the longest duration
            For Each fFile As String In IFOFiles
                ParsedIFOFile = fctParseIFO_VSTFile(intNbOfIFOFile, fFile)
                currDuration = Convert.ToInt32(GetProgramChainPlayBackTime(1, 1, True))
                If currDuration > currLongest Then
                    currLongest = currDuration
                    tIFOFile = ParsedIFOFile
                End If
            Next

            ParsedIFOFile = tIFOFile
            Return True
        ElseIf Path.GetExtension(strPath).ToLower = ".ifo" Then
            ParsedIFOFile = fctParseIFO_VSTFile(intNbOfIFOFile, strPath)
            Return True
        End If

        Return False
    End Function

    Public ReadOnly Property GetIFOAudioNumberOfTracks() As Integer
        Get
            Return ParsedIFOFile.NumAudioStreams_VTS_VOBS
        End Get
    End Property

    Public ReadOnly Property GetIFOSubPicNumberOf() As Integer
        Get
            Return ParsedIFOFile.NumSubPictureStreams_VTS_VOBS
        End Get
    End Property

    Public ReadOnly Property GetNumberIFOFileToRead() As Byte
        Get
            GetNumberIFOFileToRead = intNbOfIFOFile
        End Get
    End Property

    Public ReadOnly Property GetNumberProgramChains() As Integer
        Get
            Return ParsedIFOFile.NumberOfProgramChains
        End Get
    End Property

    Public ReadOnly Property GetProgramChainPlayBackTime(ByVal bytProChainIndex As Byte, Optional ByVal bytCellIndex As Byte = 0, Optional ByVal MinsOnly As Boolean = False) As String
        Get
            If bytProChainIndex <= ParsedIFOFile.NumberOfProgramChains Then
                bytProChainIndex = bytProChainIndex - 1

                If bytCellIndex = 0 Then
                    Return fctPlayBackTimeToString(ParsedIFOFile.ProgramChainInformation(bytProChainIndex).PlayBackTime, MinsOnly)
                Else
                    Return fctPlayBackTimeToString(ParsedIFOFile.ProgramChainInformation(bytProChainIndex).PChainInformation(bytCellIndex - 1).CellPlayBackTime, MinsOnly)
                End If
            End If
            Return String.Empty
        End Get
    End Property

    Public ReadOnly Property GetProgramChainNumChapters(ByVal bytProChainIndex As Byte) As Integer
        Get
            If bytProChainIndex <= ParsedIFOFile.NumberOfProgramChains Then
                bytProChainIndex = bytProChainIndex - 1
                Return ParsedIFOFile.ProgramChainInformation(bytProChainIndex).NumberOfPrograms
            End If
        End Get
    End Property

    Public ReadOnly Property GetIFOVideo() As String()
        Get
            Dim ReturnArray(2) As String
            If mVideoCodingMode.ContainsKey(ParsedIFOFile.VideoAtt_VTS_VOBS.Coding_Mode.ToString) Then
                ReturnArray(0) = mVideoCodingMode.Item(ParsedIFOFile.VideoAtt_VTS_VOBS.Coding_Mode.ToString)
            Else
                'assume mpeg2
                ReturnArray(0) = "mpeg2"
            End If
            ReturnArray(1) = mVideoResolution(ParsedIFOFile.VideoAtt_VTS_VOBS.Video_Standard)(ParsedIFOFile.VideoAtt_VTS_VOBS.Resolution)
            If ReturnArray(1).Contains("x") Then
                Dim sinAspect() As String
                sinAspect = ReturnArray(1).Split(New Char() {"x"})
                ReturnArray(2) = Convert.ToSingle(sinAspect(0)) / Convert.ToSingle(sinAspect(1))
            Else
                ReturnArray(2) = String.Empty
            End If

            Return ReturnArray
        End Get
    End Property

    Public ReadOnly Property GetIFOAudio(ByVal bytAudioIndex As Byte) As String()
        Get
            Dim ReturnArray(2) As String
            If bytAudioIndex <= ParsedIFOFile.NumAudioStreams_VTS_VOBS AndAlso bytAudioIndex > 0 Then
                bytAudioIndex -= 1
                If mAudioModes.ContainsKey(ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).CodingMode.ToString) Then
                    ReturnArray(0) = mAudioModes.Item(ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).CodingMode.ToString)
                Else
                    'assume ac3
                    ReturnArray(0) = "ac3"
                End If
                ReturnArray(1) = fctLang2CodeToLong(ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).LanguageCode)
                ReturnArray(2) = ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).NumberOfChannels.ToString
            End If
                Return ReturnArray
        End Get
    End Property

    Public ReadOnly Property GetIFOSubPic(ByVal bytSubPicIndex As Byte) As String
        Get
            If bytSubPicIndex <= ParsedIFOFile.NumSubPictureStreams_VTS_VOBS AndAlso bytSubPicIndex > 0 Then
                bytSubPicIndex -= 1
                Return fctLang2CodeToLong(ParsedIFOFile.SubPictureAtt_VTS_VOBS(bytSubPicIndex).LanguageCode)
            End If
            Return String.Empty
        End Get
    End Property

    Private Function fctLang2CodeToLong(ByVal strLanCode As String) As String
        If mLanguages.ContainsKey(strLanCode) Then
            Return mLanguages.Item(strLanCode)
        End If
        Return String.Empty
    End Function

    'Convert the DVD time type to a string HH:MM:SS
    Private Function fctPlayBackTimeToString(ByRef PlayBack As DVD_Time_Type, Optional ByVal MinsOnly As Boolean = False) As String
        If MinsOnly Then
            Return (PlayBack.hours * 60) + PlayBack.minutes
        Else
            Return String.Concat((PlayBack.hours).ToString("00"), "h ", (PlayBack.minutes).ToString("00"), "mn ", (PlayBack.seconds).ToString("00"), "s ")
        End If
    End Function

    'Fill in the Audio Header Information
    Private Function fctAudioAttVTSM_VTS(ByVal strAudioInfo As String) As struct_AudioAttributes_VTSM_VTS
        Dim byteInfo(8) As Byte
        Dim i As Integer
        Dim bytTempValue As Byte

        'Setup Byte info
        For i = 0 To 7
            byteInfo(i) = Convert.ToInt32(oEnc.GetBytes(((strAudioInfo).Substring(i, 1)).Chars(0))(0))
        Next

        If byteInfo(2) <> 0 AndAlso byteInfo(3) <> 0 Then
            fctAudioAttVTSM_VTS.LanguageCode = Convert.ToChar(byteInfo(2)) & Convert.ToChar(byteInfo(3)) ' & Convert.ToChar(byteInfo(4))
        Else
            fctAudioAttVTSM_VTS.LanguageCode = ""
        End If

        'Using Logic AND's to check if bits are set dec 176 -> bin 10110000
        bytTempValue = 0
        If (byteInfo(0) And 32) = 32 Then bytTempValue = 1
        If (byteInfo(0) And 64) = 64 Then bytTempValue = bytTempValue + 2
        If (byteInfo(0) And 128) = 128 Then bytTempValue = bytTempValue + 4
        fctAudioAttVTSM_VTS.CodingMode = bytTempValue


        If (byteInfo(1) And 1) = 1 Then bytTempValue = 1
        If (byteInfo(1) And 2) = 2 Then bytTempValue = bytTempValue + 2
        If (byteInfo(1) And 4) = 4 Then bytTempValue = bytTempValue + 4
        fctAudioAttVTSM_VTS.NumberOfChannels = bytTempValue + 1

    End Function

    Private Function fctHexOffset(ByVal strHexString As String) As Integer
        Return fctHexStrToHex(strHexString)
    End Function

    Private Function fctHexStrToHex(ByVal strHexString As String) As Integer
        Return String.Concat("&H", (strHexString).ToUpper)
    End Function

    'convert hex time style 0x01 0x25 0x30 to Dec 1,25,30, and not real amounts of 1,37,48
    Private Function fctHexTimeToDecTime(ByVal bytAmountHex As Byte) As Byte
        Select Case bytAmountHex
            Case Is < 9
                Return bytAmountHex
            Case Is < fctHexStrToHex("19")
                Return bytAmountHex - 6
            Case Is < fctHexStrToHex("29")
                Return bytAmountHex - 12
            Case Is < fctHexStrToHex("39")
                Return bytAmountHex - 18
            Case Is < fctHexStrToHex("49")
                Return bytAmountHex - 24
            Case Is < fctHexStrToHex("59")
                Return bytAmountHex - 30
        End Select
    End Function

    'Note:  This is for the Chapter Information
    Private Function fctPChainInformation(ByVal shoProgramChainNumber As Short, ByVal shoCellNumber As Short, ByRef strIFOFileBuffer As String, ByRef tmpIFO As struct_IFO_VST_Parse) As PGC_Cell_Info_Type
        Dim ChainLoc As Integer

        'Setup the Start loc for the File
        ChainLoc = (tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE) + fctStrByteToHex((strIFOFileBuffer).Substring(tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE + 12 + (shoProgramChainNumber) * 8, 4))
        ChainLoc = ChainLoc + fctStrByteToHex((strIFOFileBuffer).Substring(ChainLoc + 232, 2)) + 1 + (ifo_CellInfoSize * (shoCellNumber - 1))

        fctPChainInformation.CellPlayBackTime.hours = fctHexTimeToDecTime(Convert.ToInt32(oEnc.GetBytes(((strIFOFileBuffer).Substring(ChainLoc + 3, 1)).Chars(0))(0)))
        fctPChainInformation.CellPlayBackTime.minutes = fctHexTimeToDecTime(Convert.ToInt32(oEnc.GetBytes(((strIFOFileBuffer).Substring(ChainLoc + 4, 1)).Chars(0))(0)))
        fctPChainInformation.CellPlayBackTime.seconds = fctHexTimeToDecTime(Convert.ToInt32(oEnc.GetBytes(((strIFOFileBuffer).Substring(ChainLoc + 5, 1)).Chars(0))(0)))
    End Function

    Private Function fctProgramChainInformation(ByVal shoProgramChainNumber As Short, ByRef strIFOFileBuffer As String, ByRef tmpIFO As struct_IFO_VST_Parse) As struct_Program_Chain_Type
        Dim ChainLoc As Integer
        Dim PCT As New struct_Program_Chain_Type

        'Setup the Start loc for the File
        ChainLoc = (tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE) + fctStrByteToHex((strIFOFileBuffer).Substring(tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE + 12 + (shoProgramChainNumber) * 8, 4))

        'The Program Number
        PCT.NumberOfPrograms = fctStrByteToHex((strIFOFileBuffer).Substring(ChainLoc + 2, 1))

        'Number of Cells in Program Chain
        PCT.NumberOfCells = fctStrByteToHex((strIFOFileBuffer).Substring(ChainLoc + 3, 1))

        'Get DVD Chain Type Info
        PCT.PlayBackTime.hours = fctHexTimeToDecTime(Convert.ToInt32(((strIFOFileBuffer).Substring(ChainLoc + 4, 1)).Chars(0)))
        PCT.PlayBackTime.minutes = fctHexTimeToDecTime(Convert.ToInt32(((strIFOFileBuffer).Substring(ChainLoc + 5, 1)).Chars(0)))
        PCT.PlayBackTime.seconds = fctHexTimeToDecTime(Convert.ToInt32(((strIFOFileBuffer).Substring(ChainLoc + 6, 1)).Chars(0)))

        Return PCT
    End Function

    'Open an IFO file and return the Parsed Variable
    Private Function fctParseIFO_VSTFile(ByVal iIndexFile As Integer, ByVal strFileName As String) As struct_IFO_VST_Parse
        Dim strTmpIFOFileIn As String
        Dim tmpIFO As New struct_IFO_VST_Parse

        Dim intFileLength As Integer
        Dim i As Integer

        'Read in the Info file name
        Dim objFS As FileStream
        objFS = File.Open(strFileName, FileMode.Open, FileAccess.Read)
        Dim objBR As New BinaryReader(objFS)
        strTmpIFOFileIn = System.Text.Encoding.Default.GetString(objBR.ReadBytes(objFS.Length))
        intFileLength = strTmpIFOFileIn.Length
        objBR.Close()
        objBR = Nothing
        objFS.Close()
        objFS = Nothing

        'Save the Ifo File name
        tmpIFO.IFO_FileName = strFileName

        'See MPUcoder (http://www.mpucoder.com/DVD/ifo.html)
        'Parse the File in tmpIFOFileIn
        tmpIFO.FileType_Header = (strTmpIFOFileIn).Substring(fctHexOffset("0"), 12)
        tmpIFO.LastSectorOfTitle = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C"), 4))
        tmpIFO.LastSectorOfIFO = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("1C"), 4))
        tmpIFO.VersionNumber = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("20"), 2))
        tmpIFO.VSTCategory = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("22"), 4))

        tmpIFO.EndByteAddress_VTS_MAT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("80"), 4))

        tmpIFO.StartSector_MenuVOB = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C0"), 4))
        tmpIFO.StartSector_TitleVOB = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C4"), 4))

        tmpIFO.SectorPointer_VTS_PTT_SRPT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C8"), 4))
        tmpIFO.SectorPointer_VTS_PGCI = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("CC"), 4))
        tmpIFO.SectorPointer_VTSM_PGCI_UT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("D0"), 4))
        tmpIFO.SectorPointer_VTS_TMAPTI = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("D4"), 4))
        tmpIFO.SectorPointer_VSTM_C_ADT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("D8"), 4))
        tmpIFO.SectorPointer_VSTM_VOBU_ADMAP = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("DC"), 4))
        tmpIFO.SectorPointer_VST_C_ADT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("E0"), 4))
        tmpIFO.SectorPointer_VTS_VOBU_ADMAP = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("E4"), 4))

        'VTSM
        'Get Video of Main Information
        tmpIFO.VideoAtt_VSTM_VOBS = fctVideoAtt_VTS_VOBS((strTmpIFOFileIn).Substring(fctHexOffset("100"), 2))
        tmpIFO.NumOfAudioStreamsIn_VTSM_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("102"), 2))
        'Read in all Audio Track Information
        For intFileLength = 0 To tmpIFO.NumOfAudioStreamsIn_VTSM_VOBS - 1
            ReDim Preserve tmpIFO.AudioAtt_VTSM_VOBS(intFileLength)
            tmpIFO.AudioAtt_VTSM_VOBS(intFileLength) = fctAudioAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("104") + (intFileLength * 8), 8))
        Next

        'Get SubPicture Main info
        tmpIFO.NumSubPictureStreams_VTSM_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("154"), 2))
        tmpIFO.SubPictureAtt_VTSM_VOBS = fctSubPictureAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("156"), 6))

        'VTS
        'Get Video Information
        tmpIFO.VideoAtt_VTS_VOBS = fctVideoAtt_VTS_VOBS((strTmpIFOFileIn).Substring(fctHexOffset("200"), 2))

        'Get Audio Information
        tmpIFO.NumAudioStreams_VTS_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("203"), 1))
        For intFileLength = 0 To tmpIFO.NumAudioStreams_VTS_VOBS - 1
            ReDim Preserve tmpIFO.AudioAtt_VTS_VOBS(intFileLength)
            tmpIFO.AudioAtt_VTS_VOBS(intFileLength) = fctAudioAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("204") + (intFileLength * 8), 8))
        Next

        'Get SubPicture info
        tmpIFO.NumSubPictureStreams_VTS_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("255"), 1))
        For intFileLength = 0 To tmpIFO.NumSubPictureStreams_VTS_VOBS - 1
            ReDim Preserve tmpIFO.SubPictureAtt_VTS_VOBS(intFileLength)
            tmpIFO.SubPictureAtt_VTS_VOBS(intFileLength) = fctSubPictureAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("256") + (intFileLength * 6), 6))
        Next

        'Get Program Chain Information
        tmpIFO.NumberOfProgramChains = fctStrByteToHex((strTmpIFOFileIn).Substring(tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE, 2))
        For intFileLength = 0 To tmpIFO.NumberOfProgramChains - 1
            ReDim Preserve tmpIFO.ProgramChainInformation(intFileLength)
            tmpIFO.ProgramChainInformation(intFileLength) = fctProgramChainInformation(intFileLength, strTmpIFOFileIn, tmpIFO)
            For i = 0 To tmpIFO.ProgramChainInformation(intFileLength).NumberOfPrograms
                ReDim Preserve tmpIFO.ProgramChainInformation(intFileLength).PChainInformation(i)
                tmpIFO.ProgramChainInformation(intFileLength).PChainInformation(i) = fctPChainInformation(intFileLength, i, strTmpIFOFileIn, tmpIFO)
            Next
        Next

        'Setup the Return Value
        fctParseIFO_VSTFile = tmpIFO

        'Manually Release Memory
        strTmpIFOFileIn = ""
    End Function

    Public Function CovertByteToHex(ByVal BytConvert() As Byte) As String
        Dim hexStr As String = String.Empty
        Try

            Dim i As Integer
            For i = 0 To BytConvert.Length - 1
                hexStr = hexStr + (BytConvert(i)).ToString("X")
            Next i
            hexStr = hexStr.PadLeft(16, "0")
            hexStr = hexStr.Insert(0, "0x")

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return hexStr
    End Function

    'Convert a string of Bytes (0x00-0xFF) into a complete number
    Private Function fctStrByteToHex(ByVal strHexString As String) As Integer
        Dim i As Long
        Dim HexTotal As Double
        Dim HexMod As Double
        Dim CharNum As Long

        HexTotal = 0
        For i = 0 To (strHexString).Length - 1
            CharNum = Convert.ToInt32(oEnc.GetBytes(strHexString.Substring(i, 1).Chars(0))(0))
            If i <> (strHexString).Length Then
                HexMod = 256 ^ (((strHexString).Length - 1) - i)
                HexTotal = HexTotal + CharNum * HexMod
            Else
                HexTotal = HexTotal + CharNum
            End If
        Next
        Return HexTotal
    End Function


    'Creates the Video info from a string of 2 bytes
    Private Function fctSRPT(ByVal VideoInfo As String) As struct_SRPT
        Dim byte1 As Byte
        Dim byte2 As Byte
        Dim bytTmpValue As Byte

        'Split String for logic AND checks
        byte1 = Convert.ToInt32(oEnc.GetBytes(((VideoInfo).Substring(0, 1)).Chars(0))(0))
        byte2 = Convert.ToInt32(oEnc.GetBytes(((VideoInfo).Substring(1, 1)).Chars(0))(0))

        'BYTE 1

        'Revers bits

        bytTmpValue = 0
        If (byte1 And 64) = 64 Then bytTmpValue = 1
        If (byte1 And 128) = 128 Then bytTmpValue = bytTmpValue + 2
        fctSRPT.Coding_Mode = bytTmpValue

        'BYTE 2
        fctSRPT.Bitrate = (byte2 And 16)

        bytTmpValue = 0
        If (byte2 And 4) = 4 Then bytTmpValue = 1
        If (byte2 And 8) = 8 Then bytTmpValue = bytTmpValue + 2
        fctSRPT.Resolution = bytTmpValue

        bytTmpValue = 0
        If (byte2 And 32) = 32 Then bytTmpValue = 1
        If (byte2 And 64) = 64 Then bytTmpValue = bytTmpValue + 2
        If (byte2 And 128) = 128 Then bytTmpValue = bytTmpValue + 4
    End Function

    'Creates the Video info from a string of 2 bytes
    Private Function fctVideoAtt_VTS_VOBS(ByVal VideoInfo As String) As struct_VideoAttributes_VTS_VOBS
        Dim byte1 As Byte
        Dim byte2 As Byte
        Dim bytTmpValue As Byte

        'Split String for logic AND checks
        byte1 = Convert.ToInt32(oEnc.GetBytes(((VideoInfo).Substring(0, 1)).Chars(0))(0))
        byte2 = Convert.ToInt32(oEnc.GetBytes(((VideoInfo).Substring(1, 1)).Chars(0))(0))

        'BYTE 1
        'Revers bits

        bytTmpValue = 0
        If (byte1 And 16) = 16 Then bytTmpValue = 1
        If (byte1 And 32) = 32 Then bytTmpValue = bytTmpValue + 2
        fctVideoAtt_VTS_VOBS.Video_Standard = bytTmpValue

        bytTmpValue = 0
        If (byte1 And 64) = 64 Then bytTmpValue = 1
        If (byte1 And 128) = 128 Then bytTmpValue = bytTmpValue + 2
        fctVideoAtt_VTS_VOBS.Coding_Mode = bytTmpValue

        'BYTE 2
        fctVideoAtt_VTS_VOBS.LetterBoxed = (byte2 And 2)

        bytTmpValue = 0
        If (byte2 And 4) = 4 Then bytTmpValue = 1
        If (byte2 And 8) = 8 Then bytTmpValue = bytTmpValue + 2
        fctVideoAtt_VTS_VOBS.Resolution = bytTmpValue

        bytTmpValue = 0
        If (byte2 And 32) = 32 Then bytTmpValue = 1
        If (byte2 And 64) = 64 Then bytTmpValue = bytTmpValue + 2
        If (byte2 And 128) = 128 Then bytTmpValue = bytTmpValue + 4
    End Function

    Private Function fctSubPictureAttVTSM_VTS(ByVal strSubPictureInfo As String) As SubPictureAtt_VTSM_VTS_Type
        fctSubPictureAttVTSM_VTS.LanguageCode = (strSubPictureInfo).Substring(2, 1) & (strSubPictureInfo).Substring(3, 1)
        fctSubPictureAttVTSM_VTS.CodeExtention = Convert.ToInt32(oEnc.GetBytes((((strSubPictureInfo).Substring(5, 1)).Chars(0)))(0))
    End Function

    Public Sub New()
        MyBase.New()

        mLanguages.Add("", "Unknown")
        mLanguages.Add("aa", "Afar")
        mLanguages.Add("ab", "Abkhazian")
        mLanguages.Add("af", "Afrikaans")
        mLanguages.Add("ak", "Akan")
        mLanguages.Add("sq", "Albanian")
        mLanguages.Add("am", "Amharic")
        mLanguages.Add("ar", "Arabic")
        mLanguages.Add("an", "Aragonese")
        mLanguages.Add("hy", "Armenian")
        mLanguages.Add("as", "Assamese")
        mLanguages.Add("av", "Avaric")
        mLanguages.Add("ae", "Avestan")
        mLanguages.Add("ay", "Aymara")
        mLanguages.Add("az", "Azerbaijani")
        mLanguages.Add("ba", "Bashkir")
        mLanguages.Add("bm", "Bambara")
        mLanguages.Add("eu", "Basque")
        mLanguages.Add("be", "Belarusian")
        mLanguages.Add("bn", "Bengali")
        mLanguages.Add("bh", "Bihari")
        mLanguages.Add("bi", "Bislama")
        mLanguages.Add("bo", "Tibetan")
        mLanguages.Add("bs", "Bosnian")
        mLanguages.Add("br", "Breton")
        mLanguages.Add("bg", "Bulgarian")
        mLanguages.Add("my", "Burmese")
        mLanguages.Add("ca", "Catalan")
        mLanguages.Add("cs", "Czech")
        mLanguages.Add("ch", "Chamorro")
        mLanguages.Add("ce", "Chechen")
        mLanguages.Add("zh", "Chinese")
        mLanguages.Add("cu", "Slavic")
        mLanguages.Add("cv", "Chuvash")
        mLanguages.Add("kw", "Cornish")
        mLanguages.Add("co", "Corsican")
        mLanguages.Add("cr", "Cree")
        mLanguages.Add("cy", "Welsh")
        mLanguages.Add("cs", "Czech")
        mLanguages.Add("da", "Danish")
        mLanguages.Add("de", "German")
        mLanguages.Add("dv", "Divehi")
        mLanguages.Add("nl", "Dutch")
        mLanguages.Add("dz", "Dzongkha")
        mLanguages.Add("el", "Greek")
        mLanguages.Add("en", "English")
        mLanguages.Add("eo", "Esperanto")
        mLanguages.Add("et", "Estonian")
        mLanguages.Add("eu", "Basque")
        mLanguages.Add("ee", "Ewe")
        mLanguages.Add("fo", "Faroese")
        mLanguages.Add("fa", "Persian")
        mLanguages.Add("fj", "Fijian")
        mLanguages.Add("fi", "Finnish")
        mLanguages.Add("fr", "French")
        mLanguages.Add("fr", "French")
        mLanguages.Add("fy", "Western Frisian")
        mLanguages.Add("ff", "Fulah")
        mLanguages.Add("ka", "Georgian")
        mLanguages.Add("de", "German")
        mLanguages.Add("gd", "Gaelic")
        mLanguages.Add("ga", "Irish")
        mLanguages.Add("gl", "Galician")
        mLanguages.Add("gv", "Manx")
        mLanguages.Add("el", "Greek")
        mLanguages.Add("gn", "Guarani")
        mLanguages.Add("gu", "Gujarati")
        mLanguages.Add("ht", "Haitian")
        mLanguages.Add("ha", "Hausa")
        mLanguages.Add("he", "Hebrew")
        mLanguages.Add("hz", "Herero")
        mLanguages.Add("hi", "Hindi")
        mLanguages.Add("ho", "Hiri Motu")
        mLanguages.Add("hr", "Croatian")
        mLanguages.Add("hu", "Hungarian")
        mLanguages.Add("hy", "Armenian")
        mLanguages.Add("ig", "Igbo")
        mLanguages.Add("is", "Icelandic")
        mLanguages.Add("io", "Ido")
        mLanguages.Add("ii", "Sichuan Yi")
        mLanguages.Add("iu", "Inuktitut")
        mLanguages.Add("ie", "Occidental")
        mLanguages.Add("ia", "Interlingua")
        mLanguages.Add("id", "Indonesian")
        mLanguages.Add("ik", "Inupiaq")
        mLanguages.Add("is", "Icelandic")
        mLanguages.Add("it", "Italian")
        mLanguages.Add("jv", "Javanese")
        mLanguages.Add("ja", "Japanese")
        mLanguages.Add("kl", "Kalaallisut")
        mLanguages.Add("kn", "Kannada")
        mLanguages.Add("ks", "Kashmiri")
        mLanguages.Add("ka", "Georgian")
        mLanguages.Add("kr", "Kanuri")
        mLanguages.Add("kk", "Kazakh")
        mLanguages.Add("km", "Central Khmer")
        mLanguages.Add("ki", "Kikuyu")
        mLanguages.Add("rw", "Kinyarwanda")
        mLanguages.Add("ky", "Kirghiz")
        mLanguages.Add("kv", "Komi")
        mLanguages.Add("kg", "Kongo")
        mLanguages.Add("ko", "Korean")
        mLanguages.Add("kj", "Kuanyama")
        mLanguages.Add("ku", "Kurdish")
        mLanguages.Add("lo", "Lao")
        mLanguages.Add("la", "Latin")
        mLanguages.Add("lv", "Latvian")
        mLanguages.Add("li", "Limburgan")
        mLanguages.Add("ln", "Lingala")
        mLanguages.Add("lt", "Lithuanian")
        mLanguages.Add("lb", "Luxembourgish")
        mLanguages.Add("lu", "Luba-Katanga")
        mLanguages.Add("lg", "Ganda")
        mLanguages.Add("mk", "Macedonian")
        mLanguages.Add("mh", "Marshallese")
        mLanguages.Add("ml", "Malayalam")
        mLanguages.Add("mi", "Maori")
        mLanguages.Add("mr", "Marathi")
        mLanguages.Add("ms", "Malay")
        mLanguages.Add("mk", "Macedonian")
        mLanguages.Add("mg", "Malagasy")
        mLanguages.Add("mt", "Maltese")
        mLanguages.Add("mn", "Mongolian")
        mLanguages.Add("mi", "Maori")
        mLanguages.Add("ms", "Malay")
        mLanguages.Add("my", "Burmese")
        mLanguages.Add("na", "Nauru")
        mLanguages.Add("nv", "Navajo")
        mLanguages.Add("nr", "Ndebele")
        mLanguages.Add("nd", "Ndebele")
        mLanguages.Add("ng", "Ndonga")
        mLanguages.Add("ne", "Nepali")
        mLanguages.Add("nl", "Dutch")
        mLanguages.Add("nn", "Norwegian")
        mLanguages.Add("nb", "Norwegian")
        mLanguages.Add("no", "Norwegian")
        mLanguages.Add("ny", "Chichewa")
        mLanguages.Add("oc", "Occitan")
        mLanguages.Add("oj", "Ojibwa")
        mLanguages.Add("or", "Oriya")
        mLanguages.Add("om", "Oromo")
        mLanguages.Add("os", "Ossetian")
        mLanguages.Add("pa", "Panjabi")
        mLanguages.Add("fa", "Persian")
        mLanguages.Add("pi", "Pali")
        mLanguages.Add("pl", "Polish")
        mLanguages.Add("pt", "Portuguese")
        mLanguages.Add("ps", "Pushto")
        mLanguages.Add("qu", "Quechua")
        mLanguages.Add("rm", "Romansh")
        mLanguages.Add("ro", "Romanian")
        mLanguages.Add("rn", "Rundi")
        mLanguages.Add("ru", "Russian")
        mLanguages.Add("sg", "Sango")
        mLanguages.Add("sa", "Sanskrit")
        mLanguages.Add("si", "Sinhala")
        mLanguages.Add("sk", "Slovak")
        mLanguages.Add("sk", "Slovak")
        mLanguages.Add("sl", "Slovenian")
        mLanguages.Add("se", "Northern Sami")
        mLanguages.Add("sm", "Samoan")
        mLanguages.Add("sn", "Shona")
        mLanguages.Add("sd", "Sindhi")
        mLanguages.Add("so", "Somali")
        mLanguages.Add("st", "Sotho")
        mLanguages.Add("es", "Spanish")
        mLanguages.Add("sq", "Albanian")
        mLanguages.Add("sc", "Sardinian")
        mLanguages.Add("sr", "Serbian")
        mLanguages.Add("ss", "Swati")
        mLanguages.Add("su", "Sundanese")
        mLanguages.Add("sw", "Swahili")
        mLanguages.Add("sv", "Swedish")
        mLanguages.Add("ty", "Tahitian")
        mLanguages.Add("ta", "Tamil")
        mLanguages.Add("tt", "Tatar")
        mLanguages.Add("te", "Telugu")
        mLanguages.Add("tg", "Tajik")
        mLanguages.Add("tl", "Tagalog")
        mLanguages.Add("th", "Thai")
        mLanguages.Add("bo", "Tibetan")
        mLanguages.Add("ti", "Tigrinya")
        mLanguages.Add("to", "Tonga")
        mLanguages.Add("tn", "Tswana")
        mLanguages.Add("ts", "Tsonga")
        mLanguages.Add("tk", "Turkmen")
        mLanguages.Add("tr", "Turkish")
        mLanguages.Add("tw", "Twi")
        mLanguages.Add("ug", "Uighur")
        mLanguages.Add("uk", "Ukrainian")
        mLanguages.Add("ur", "Urdu")
        mLanguages.Add("uz", "Uzbek")
        mLanguages.Add("ve", "Venda")
        mLanguages.Add("vi", "Vietnamese")
        mLanguages.Add("vo", "Volap�k")
        mLanguages.Add("cy", "Welsh")
        mLanguages.Add("wa", "Walloon")
        mLanguages.Add("wo", "Wolof")
        mLanguages.Add("xh", "Xhosa")
        mLanguages.Add("yi", "Yiddish")
        mLanguages.Add("yo", "Yoruba")
        mLanguages.Add("za", "Zhuang")
        mLanguages.Add("zh", "Chinese")
        mLanguages.Add("zu", "Zulu")

        'Audio Format
        mAudioModes.Add("0", "ac3")
        mAudioModes.Add("1", String.Empty)
        mAudioModes.Add("2", "mpeg1")
        mAudioModes.Add("3", "mpeg2")
        mAudioModes.Add("4", "lpcm")
        mAudioModes.Add("5", String.Empty)
        mAudioModes.Add("6", "dca")
        mAudioModes.Add("7", String.Empty)

        'Video
        mVideoAspect.Add("0", "4:3")
        mVideoAspect.Add("1", String.Empty)
        mVideoAspect.Add("2", String.Empty)
        mVideoAspect.Add("3", "16:9")

        mVideoCodingMode.Add("0", "mpeg1")
        mVideoCodingMode.Add("1", "mpeg2")

        mVideoResolution(0) = New String() {"720x480", "704x480", "352x480", "352x240"}
        mVideoResolution(1) = New String() {"720x576", "704x576", "352x576", "352x288"}
    End Sub

    Protected Overrides Sub Finalize()
        mLanguages = Nothing
        mAudioModes = Nothing
        mVideoAspect = Nothing
        mVideoCodingMode = Nothing
        mVideoResolution = Nothing
        oEnc = Nothing

        MyBase.Finalize()
        GC.Collect()
    End Sub

    Public Sub Close()
        Me.Finalize()
    End Sub
End Class