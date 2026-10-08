using System; // 예외와 변환 기능
using System.IO; // 파일 기능
using System.Text; // 문자 인코딩
using UnityEngine; // Unity 기본 기능

// 저장 파일 읽기와 쓰기 (기획서 15.8)
// 파일은 JSON이다. 진행 데이터는 간단히 가려서 payload에 넣고, 내용이 바뀌었는지 알 수 있게 검사값을 함께 적는다.
// 쓰는 순서: 임시 파일에 기록 -> 다시 읽어 검사 -> 기존 정상 파일을 백업 -> 임시 파일을 실제 파일로 교체.
public static partial class SaveFileSystem
{
    [Serializable]
    private class SaveFileEnvelope // 저장 파일의 겉 구조
    {
        public string format;   // 저장 파일 구분 문구
        public int version;     // 저장 데이터 버전
        public string checksum; // 무결성 검사값
        public string payload;  // 가린 진행 데이터
    }

    private const string FolderName = "Saves";     // 저장 폴더 이름
    private const string FileExtension = ".sav";   // 저장 파일
    private const string BackupExtension = ".bak"; // 직전 정상 파일의 백업
    private const string TempExtension = ".tmp";   // 쓰는 중인 임시 파일

    public static string FolderPath =>
        Path.Combine(Application.persistentDataPath, FolderName); // 저장 폴더 경로

    private static string GetPath(SaveSlot slot, string extension)
    {
        return Path.Combine(FolderPath, SaveRules.GetFileName(slot) + extension);
    }

    // 진행 데이터를 칸에 쓴다. 실패하면 기존 정상 파일은 그대로 남는다.
    public static bool Write(SaveSlot slot, SaveData data, out string error)
    {
        if (data == null)
        {
            error = "저장할 데이터가 없습니다.";
            return false;
        }

        string mainPath = GetPath(slot, FileExtension);
        string backupPath = GetPath(slot, BackupExtension);
        string tempPath = GetPath(slot, TempExtension);

        try
        {
            Directory.CreateDirectory(FolderPath);

            File.WriteAllText(tempPath, Encode(data), new UTF8Encoding(false)); // 1. 임시 파일에 기록

            if (!TryReadFile(tempPath, out _, out string checkError)) // 2. 다시 읽어 검사
            {
                File.Delete(tempPath);
                error = "저장 데이터 검사에 실패했습니다. " + checkError;
                return false;
            }

            if (File.Exists(mainPath))
            {
                if (TryReadFile(mainPath, out _, out _))
                {
                    File.Copy(mainPath, backupPath, true); // 3. 정상 파일만 백업으로 남긴다.
                }

                File.Delete(mainPath);
            }

            File.Move(tempPath, mainPath); // 4. 임시 파일을 실제 파일로 교체
        }
        catch (Exception exception)
        {
            error = "저장 파일을 쓰지 못했습니다. " + exception.Message;
            return false;
        }

        error = string.Empty;
        return true;
    }

    // 칸을 읽는다. 본 파일에 문제가 있으면 백업으로 복구를 시도한다. (기획서 15.4)
    public static SaveSlotInfo Read(SaveSlot slot)
    {
        string mainPath = GetPath(slot, FileExtension);
        string backupPath = GetPath(slot, BackupExtension);

        bool hasMain = File.Exists(mainPath);
        bool hasBackup = File.Exists(backupPath);

        if (!hasMain && !hasBackup)
        {
            return new SaveSlotInfo(slot, false, false, false, string.Empty, null);
        }

        string mainError = "저장 파일이 없습니다.";

        if (hasMain && TryReadFile(mainPath, out SaveData mainData, out mainError))
        {
            return new SaveSlotInfo(slot, true, true, false, string.Empty, mainData);
        }

        if (hasBackup && TryReadFile(backupPath, out SaveData backupData, out _))
        {
            return new SaveSlotInfo(slot, true, true, true, mainError, backupData);
        }

        return new SaveSlotInfo(slot, true, false, false, mainError, null);
    }

    // 칸을 지운다. 백업과 임시 파일도 함께 지운다. (기획서 15.28)
    public static bool Delete(SaveSlot slot, out string error)
    {
        try
        {
            File.Delete(GetPath(slot, FileExtension));
            File.Delete(GetPath(slot, BackupExtension));
            File.Delete(GetPath(slot, TempExtension));
        }
        catch (Exception exception)
        {
            error = "저장 파일을 지우지 못했습니다. " + exception.Message;
            return false;
        }

        error = string.Empty;
        return true;
    }

    // 불러올 수 있는 칸 가운데 저장 일시가 가장 최근인 칸을 찾는다. (기획서 15.4)
    public static SaveSlotInfo FindLatest()
    {
        SaveSlotInfo latest = null;

        foreach (SaveSlot slot in SaveRules.Slots)
        {
            SaveSlotInfo info = Read(slot);

            if (!info.IsValid) { continue; }

            if (latest == null || info.SavedAt > latest.SavedAt)
            {
                latest = info;
            }
        }

        return latest;
    }

    public static bool HasAnyFile() // 저장 파일이 하나라도 있는지 여부 (손상된 파일 포함)
    {
        foreach (SaveSlot slot in SaveRules.Slots)
        {
            if (File.Exists(GetPath(slot, FileExtension))) { return true; }
            if (File.Exists(GetPath(slot, BackupExtension))) { return true; }
        }

        return false;
    }
}
