using System;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using NetworkMessage.Models;

namespace RemoteControlWPFClient.BusinessLayer.DTO
{

    public sealed class FileInfoDTO
    {
        public string Name { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ChangingDate { get; set; }

        public string FullName { get; set; }

        public long? FileLengthByte { get; set; }

        public long? FileLengthMb { get; set; }

        public PackIconKind FileTypeIcon { get; set; }

        public FileType FileType { get; set; }

        public FileInfoDTO(MyFileInfo myFileInfo)
        {
            if (myFileInfo == null) throw new ArgumentNullException(nameof(myFileInfo));

            Name = myFileInfo.Name;
            FullName = myFileInfo.FullName;
            CreationDate = myFileInfo.CreationDate == new DateTime() ? null : myFileInfo.CreationDate;
            ChangingDate = myFileInfo.ChangingDate == new DateTime() ? null : myFileInfo.ChangingDate;
            FileLengthByte = myFileInfo.FileLength;
            FileLengthMb = myFileInfo.FileLength / 1024;
            FileType = FileType.File;
            int extensionPos = Name?.LastIndexOf('.') ?? -1;
            FileTypeIcon = Name?.Substring(extensionPos == -1 ? 0 : extensionPos).ToLower() switch
            {
                ".doc" or ".docx" or ".docm" => PackIconKind.FileWordOutline,
                ".xls" or ".xlsx" or ".xkm" => PackIconKind.FileExcelOutline,
                ".ppt" or ".pptx" or ".pptm" => PackIconKind.FilePowerpointOutline,
                ".pdf" => PackIconKind.FilePdfBox,
                ".cpp" or ".cs" or ".html" or ".cshtml" or ".kt" or ".java" or ".py" or ".xml" or ".xaml" => PackIconKind
                    .FileCodeOutline,
                ".zip" or ".rar" or ".tar" => PackIconKind.ArchiveOutline,
                ".jpg" or ".jpeg" or ".png" or ".ico" or ".svg" or ".raw" or ".bpm" => PackIconKind.FileImageOutline,
                ".mp4" or ".mov" or ".avi" or ".mpeg" => PackIconKind.FileVideoOutline,
                ".mp3" => PackIconKind.FileMusicOutline,
                ".db" => PackIconKind.DatabaseOutline,
                ".txt" or ".rtf" => PackIconKind.FileDocumentOutline,
                ".dll" or ".conf" or ".bat" or ".exe" => PackIconKind.FileCogOutline,
                _ => PackIconKind.FileQuestionOutline
            };
        }

        public FileInfoDTO(MyDirectoryInfo myDirectoryInfo)
        {
            if (myDirectoryInfo == null) throw new ArgumentNullException(nameof(myDirectoryInfo));

            Name = myDirectoryInfo.Name;
            FullName = myDirectoryInfo.FullName;
            CreationDate = myDirectoryInfo.CreationDate == new DateTime() ? null : myDirectoryInfo.CreationDate;
            ChangingDate = myDirectoryInfo.ChangingDate == new DateTime() ? null : myDirectoryInfo.ChangingDate;
            FileLengthByte = null;
            FileLengthMb = null;
            FileType = FileType.Directory;
            FileTypeIcon = PackIconKind.FolderOutline;
        }
    }

    public enum FileType
    {
        File,
        Directory
    }
}