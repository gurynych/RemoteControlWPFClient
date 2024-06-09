using System;
using MaterialDesignThemes.Wpf;
using NetworkMessage.DTO;

namespace RemoteControlWPFClient.BusinessLayer.DTO
{

    public sealed class FileInfoDTO
    {
        public string Name { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ChangingDate { get; set; }

        public string FullName { get; set; }

        public long? FileLength { get; set; }

        public PackIconKind FileTypeIcon { get; set; }

        public FileType FileType { get; set; }

        public FileInfoDTO(NetworkMessage.DTO.FileInfoDTO fileIntoDTO)
        {
            if (fileIntoDTO == null) throw new ArgumentNullException(nameof(fileIntoDTO));

            Name = fileIntoDTO.Name;
            FullName = fileIntoDTO.FullName;
            CreationDate = fileIntoDTO.CreationDate;
            ChangingDate = fileIntoDTO.ChangingDate;
            FileLength = fileIntoDTO.FileLength;
            FileType = Enum.Parse<FileType>(fileIntoDTO.FileType);
            if (FileType == FileType.File)
            {
                int extensionPos = Name?.LastIndexOf('.') ?? -1;
                FileTypeIcon = Name?.Substring(extensionPos == -1 ? 0 : extensionPos).ToLower() switch
                {
                    ".doc" or ".docx" or ".docm" => PackIconKind.FileWordOutline,
                    ".xls" or ".xlsx" or ".xkm" => PackIconKind.FileExcelOutline,
                    ".ppt" or ".pptx" or ".pptm" => PackIconKind.FilePowerpointOutline,
                    ".pdf" => PackIconKind.FilePdfBox,
                    ".cpp" or ".cs" or ".html" or ".cshtml" or ".kt" or ".java" or ".py" or ".xml"
                        or ".xaml" => PackIconKind
                            .FileCodeOutline,
                    ".zip" or ".rar" or ".tar" => PackIconKind.ArchiveOutline,
                    ".jpg" or ".jpeg" or ".png" or ".ico" or ".svg" or ".raw" or ".bpm" =>
                        PackIconKind.FileImageOutline,
                    ".mp4" or ".mov" or ".avi" or ".mpeg" => PackIconKind.FileVideoOutline,
                    ".mp3" => PackIconKind.FileMusicOutline,
                    ".db" => PackIconKind.DatabaseOutline,
                    ".txt" or ".rtf" => PackIconKind.FileDocumentOutline,
                    ".dll" or ".conf" or ".bat" or ".exe" => PackIconKind.FileCogOutline,
                    _ => PackIconKind.FileQuestionOutline
                };
            }
            else
            {
                FileTypeIcon = PackIconKind.FolderOutline;
            }
        }
    }
}